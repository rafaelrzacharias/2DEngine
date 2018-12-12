using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace GameStateManager
{
    public enum InputType
    {
        NONE,
        KEYBOARD,
        GAMEPAD,
        TOUCH
    }

#if DESKTOP
    public enum MouseButtons
    {
        LEFT,
        MIDDLE,
        RIGHT
    }
#endif

    [Flags]
    public enum Action
    {
        NONE = 0,
        UI_SELECT = 1 << 0,
        UI_BACK = 1 << 1,
        UP = 1 << 2,
        DOWN = 1 << 3,
        LEFT = 1 << 4,
        RIGHT = 1 << 5,
        LP = 1 << 6,
        HP = 1 << 7,
        LK = 1 << 8,
        HK = 1 << 9,
        LB = 1 << 10,
        LT = 1 << 11,
        RB = 1 << 12,
        RT = 1 << 13,
        START = 1 << 14,

        DEBUG = 1 << 15,
        CONSOLE = 1 << 16
    }


    // Helper for reading input from keyboard, gamepad, and touchscreen. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        public const int MAX_USERS = 4;
        public static User[] Users;

        public static Action[] Actions { get; private set; }
        public static string[] ActionNames { get; private set; }

        private static Dictionary<Buttons, Texture2D> platformButtons;
#if DESKTOP
        public static KeyboardState LastKeyboardState;
        public static KeyboardState CurrentKeyboardState;
        public static MouseState LastMouseState;
        public static MouseState CurrentMouseState;
        private static GameWindow Window;

        private static Keys lastPressedKey;
        private static float keyRepeatTimer;
        private static float keyRepeatStartDuration = 0.3f;
        private static float keyRepeatDuration = 0.05f;

        private static bool isLeftMouseDown;
        private const int dragThreshold = 3;
        public static bool isDragging;
        public static bool isDragComplete;

        private static Vector2 dragMouseStart;
        public static Vector2 MouseDragStartPosition { get { return dragMouseStart; } }

        private static Vector2 dragMouseEnd;
        public static Vector2 MouseDragEndPosition { get { return dragMouseEnd; } }

        public static Vector2 MouseDragDelta { get; private set; }
        public static float MouseDragDistance { get; private set; }

        public static bool CanSwapControllerType = true;
#endif
#if DESKTOP || CONSOLE
        public static GamePadState[] LastGamePadState;
        public static GamePadState[] CurrentGamePadState;
#endif
#if MOBILE
        public static TouchCollection TouchState;
        public static GestureSample[] Gestures;

        private static Dictionary<Action, List<GestureSample>> gestures;
#endif

        public static void Initialize(Game game)
        {
#if DESKTOP
            Window = game.Window;
            Actions = (Action[])Enum.GetValues(typeof(Action));
            ActionNames = new string[]
            {
                "None",
                "Select",
                "Back",
                "Up",
                "Down",
                "Left",
                "Right",
                "Low Punch",
                "High Punch",
                "Low Kick",
                "High Kick",
                "Crouch",
                "Block",
                "Special 1",
                "Special 2",
                "Start",

                "Debug",
                "Console"
            };

            platformButtons = new Dictionary<Buttons, Texture2D>()
            {
                { (Buttons)0, Resources.GetTexture("none") },
                { Buttons.DPadUp, Resources.GetTexture("up") },
                { Buttons.DPadDown, Resources.GetTexture("down") },
                { Buttons.DPadLeft, Resources.GetTexture("left") },
                { Buttons.DPadRight, Resources.GetTexture("right") },
                { Buttons.A, Resources.GetTexture("aButton") },
                { Buttons.B, Resources.GetTexture("bButton") },
                { Buttons.X, Resources.GetTexture("xButton") },
                { Buttons.Y, Resources.GetTexture("yButton") },
                { Buttons.LeftShoulder, Resources.GetTexture("lbButton") },
                { Buttons.LeftTrigger, Resources.GetTexture("ltButton") },
                { Buttons.RightShoulder, Resources.GetTexture("rbButton") },
                { Buttons.RightTrigger, Resources.GetTexture("rtButton") },
                { Buttons.Start, Resources.GetTexture("menuButton") },
                { Buttons.Back, Resources.GetTexture("windowsButton") },
            };

            // The buffered input for each player
            BufferedInput.Initialize();

            LastKeyboardState = new KeyboardState();
            CurrentKeyboardState = new KeyboardState();
            LastMouseState = new MouseState();
            CurrentMouseState = new MouseState();
#endif
#if DESKTOP || CONSOLE
            LastGamePadState = new GamePadState[MAX_USERS];
            CurrentGamePadState = new GamePadState[MAX_USERS];

            for (int i = 0; i < MAX_USERS; i++)
            {
                LastGamePadState[i] = new GamePadState();
                CurrentGamePadState[i] = new GamePadState();
            }
#endif
#if MOBILE
            TouchState = new TouchState();
            Gestures = new GestureSample[MAX_GESTURES];

            for (int i = 0; i < MAX_GESTURES; i++)
                Gestures[i] = new GestureSample();

            gestures = new Dictionary<Action, List<GestureSample>>
            {

            };
#endif
            Users = new User[MAX_USERS];

            for (int i = 0; i < MAX_USERS; i++)
                Users[i] = new User();
        }


        // Gets the primary user.
        public static User GetPrimaryUser()
        {
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].IsPrimaryUser)
                    return Users[i];
            }

            return null;
#endif
#if MOBILE
            return Users[0];
#endif
        }


        // Sets the primary user.
        public static void SetPrimaryUser(User user)
        {
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
                Users[i].IsPrimaryUser = false;

            user.IsPrimaryUser = true;
#endif
#if MOBILE
            Users[0].IsPrimaryUser = true;
#endif
        }


        // Set the user controller type based on the given controller index.
        public static void SetUserControllerType(User user, int controllerIndex)
        {
#if DESKTOP || CONSOLE
            if (controllerIndex != MAX_USERS)
                user.InputType = InputType.GAMEPAD;
            else
                user.InputType = InputType.KEYBOARD;

            user.ControllerIndex = controllerIndex;

            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].ControllerIndex == controllerIndex && Users[i] != user)
                {
                    ResetUser(Users[i]);
                    break;
                }
            }
#endif
#if MOBILE
            Users[0].InputType = InputType.TOUCH;
#endif
        }


        // Reset the user to a default state.
        public static void ResetUser(User user)
        {
#if DESKTOP
            if (GetUserCount() == 1)
            {
                user.ControllerIndex = MAX_USERS;
                user.InputType = InputType.KEYBOARD;
                user.IsPrimaryUser = true;
            }
            else
#endif
            {
                user.ControllerIndex = -1;
                user.InputType = InputType.NONE;
                user.IsPrimaryUser = false;
            }

            if (GetUserCount() == 1)
            {
                for (int i = 0; i < MAX_USERS; i++)
                {
                    if (Users[i].IsActive)
                    {
                        Users[i].IsPrimaryUser = true;
                        break;
                    }
                }
            }
        }


        // Checks if the user is the only one currently active.
        public static int GetUserCount()
        {
#if DESKTOP || CONSOLE
            int count = 0;
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].IsActive)
                    count++;
            }

            return count;
#endif
#if MOBILE
            return 1;
#endif
        }


        // Reads the latest state of the keyboard and gamepad
        // and uses it to update the input history buffer.
        public static void Update()
        {
#if DESKTOP
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState(Window);

            if (CurrentMouseState.LeftButton == ButtonState.Released && isDragging)
            {
                isLeftMouseDown = false;
                isDragging = false;
                isDragComplete = true;
                dragMouseEnd = CurrentMouseState.Position.ToVector2();

                MouseDragDistance = Vector2.Distance(dragMouseStart, dragMouseEnd);
                MouseDragDelta = dragMouseEnd - dragMouseStart;
            }

            if (isLeftMouseDown == false && CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    CurrentMouseState.Equals(LastMouseState) == false)
            {
                isLeftMouseDown = true;
                isDragComplete = false;
                dragMouseStart = CurrentMouseState.Position.ToVector2();
            }

            if (isLeftMouseDown && CurrentMouseState.LeftButton == ButtonState.Released &&
                    CurrentMouseState.Equals(LastMouseState) == false)
                isLeftMouseDown = false;

            if (isLeftMouseDown && isDragging == false)
            {
                Vector2 delta = dragMouseStart - CurrentMouseState.Position.ToVector2();

                if (delta.Length() > dragThreshold) // if above threshold (5 px), set dragging to true
                {
                    isDragging = true;
                    dragMouseStart = CurrentMouseState.Position.ToVector2();
                }
            }
#endif
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                LastGamePadState[i] = CurrentGamePadState[i];
                CurrentGamePadState[i] = GamePad.GetState(i);

                if (CurrentGamePadState[i].IsConnected == false && LastGamePadState[i].IsConnected)
                    OnControllerDisconnected(i);

                if (CurrentGamePadState[i].IsConnected && LastGamePadState[i].IsConnected == false)
                    OnControllerConnected(i);

                if (Users[i].IsActive)
                    BufferedInput.Update(i);
            }
#endif
#if DESKTOP
            int index = -1;
            if (CanSwapControllerType && GetUserCount() == 1)
                index = WasAnyButtonPressed(false, false);

            if (index != -1)
            {
                User user = GetPrimaryUser();
                for (int i = 0; i < MAX_USERS; i++)
                {
                    if (Users[i].ControllerIndex == index)
                        continue;

                    switch (user.InputType)
                    {
                        case InputType.KEYBOARD:
                            {
                                if (index != MAX_USERS)
                                {
                                    user.ControllerIndex = index;
                                    user.InputType = InputType.GAMEPAD;

                                    OnUserControllerTypeChanged(i);
                                }
                            }
                            break;
                        case InputType.GAMEPAD:
                            {
                                if (index == MAX_USERS)
                                {
                                    user.ControllerIndex = index;
                                    user.InputType = InputType.KEYBOARD;

                                    OnUserControllerTypeChanged(i);
                                }
                            }
                            break;
                    }

                    break;
                }
            }
#endif
#if MOBILE
            TouchState = TouchPanel.GetState();
            Gestures.Clear();

            for (int i = 0; i < MAX_GESTURES; i++)
                Gestures[i] = null;

            int i = 0;
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures[i] = TouchPanel.ReadGesture();
                i++
            }
#endif
        }


#if DESKTOP || CONSOLE
        // Event raised when a controller is disconnected.
        public delegate void ControllerDisconnectedEventHandler(int controllerIndex);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(int controllerIndex)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].ControllerIndex == controllerIndex)
                {
                    ResetUser(Users[i]);
                    break;
                }
            }

            if (ControllerDisconnected != null)
                ControllerDisconnected.Invoke(controllerIndex);
        }


        // Event raised when a controller is connected.
        public delegate void ControllerConnectedEventHandler(int controllerIndex);
        public static event ControllerConnectedEventHandler ControllerConnected;

        public static void OnControllerConnected(int controllerIndex)
        {
            if (ControllerConnected != null)
                ControllerConnected.Invoke(controllerIndex);
        }


        // Event raised when an active user changes its controller type.
        public delegate void ControllerTypeChangedEventHandler(int controllerIndex);
        public static event ControllerTypeChangedEventHandler ControllerTypeChanged;

        public static void OnUserControllerTypeChanged(int userIndex)
        {
            if (ControllerTypeChanged != null)
                ControllerTypeChanged.Invoke(userIndex);
        }
#endif

#if DESKTOP
        // Checks if the mouse is currently hovering an interactible area.
        public static bool IsMouseOver(Rectangle area)
        {
            return (CurrentMouseState.Position.X > area.X && 
                CurrentMouseState.Position.X < area.X + area.Width &&
                CurrentMouseState.Position.Y > area.Y && 
                CurrentMouseState.Position.Y < area.Y + area.Height);
        }
#endif

        // Checks if any key was pressed on each connected gamepad, keyboard and mouse.
        public static int WasAnyButtonPressed(bool ignoreMouseMovement = true, bool ignoreThumbStickMovement = true)
        {
#if DESKTOP
            if (Console.State == State.OPENED ||
                CurrentKeyboardState.IsKeyDown(Keys.OemTilde) || LastKeyboardState.IsKeyDown(Keys.OemTilde))
                return -1;

            if (CurrentKeyboardState != LastKeyboardState)
                return MAX_USERS;

            if (CurrentMouseState != LastMouseState)
            {
                if (ignoreMouseMovement == false || CurrentMouseState.Position == LastMouseState.Position)
                    return MAX_USERS;
            }
#endif
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (HasGamePadStateChanged(CurrentGamePadState[i], LastGamePadState[i]))
                {
                    if (ignoreThumbStickMovement == false ||
                        (CurrentGamePadState[i].ThumbSticks.Left.Length() == LastGamePadState[i].ThumbSticks.Left.Length() &&
                        CurrentGamePadState[i].ThumbSticks.Right.Length() == LastGamePadState[i].ThumbSticks.Right.Length()))
                        return i;
                }
            }
#endif
#if MOBILE
            if (Gestures.Length != 0)
                return MAX_USERS;
#endif
            return -1;
        }


        private static Buttons[] buttonList = (Buttons[])Enum.GetValues(typeof(Buttons));      

        private static bool HasGamePadStateChanged(GamePadState currentGamePadState, GamePadState lastGamePadState)
        {
            if (currentGamePadState.IsConnected)
            {
                for (int b = 0; b < buttonList.Length; b++)
                {
                    Buttons button = buttonList[b];
                    if (currentGamePadState.IsButtonDown(button) != lastGamePadState.IsButtonDown(button) &&
                        currentGamePadState.IsButtonUp(button) != lastGamePadState.IsButtonUp(button))
                        return true;
                }
            }

            return false;
        }

  
#if DESKTOP
        // Return all pressed keys in a keyboard that are currently being pressed.
        public static Keys[] GetPressedKeys()
        {
            return CurrentKeyboardState.GetPressedKeys();
        }


        // Checks if a key was pressed and allow repeating the key press after some time.
        public static bool IsKeyPressed(Keys key, float dt)
        {

            // Treat it as pressed if the given key wasn't pressed in previous frame.
            if (LastKeyboardState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                lastPressedKey = key;
                return true;
            }

            // Handling key repeating if given key has pressed in previous frame.
            if (key == lastPressedKey)
            {
                keyRepeatTimer -= dt;
                if (keyRepeatTimer <= 0.0f)
                {
                    keyRepeatTimer += keyRepeatDuration;
                    return true;
                }
            }

            return false;
        }


        // Checks if a keyboard key is currently being pressed.
        public static bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }


        // Checks whether or not the mouse has moved on a player controlling a keyboard and mouse.
        public static bool HasMouseMoved()
        {
            return CurrentMouseState.Position != LastMouseState.Position;
        }


        // Checks whether or not the scroll wheel changed.
        public static bool HasScrollWheelChanged()
        {
            return CurrentMouseState.ScrollWheelValue != LastMouseState.ScrollWheelValue;
        }


        // Helper for checking if a mouse button is currently being pressed on this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a mouse click is detected, the output playerIndex reports which player performed the click.
        public static bool IsMouseDown(MouseButtons mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButtons.LEFT:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed;
                case MouseButtons.MIDDLE:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                case MouseButtons.RIGHT:
                    return CurrentMouseState.RightButton == ButtonState.Pressed;
            }

            return false;
        }
#endif


        // Reset the action maps to their default values.
        public static void ResetActionMaps(ActionMap[] actionMaps)
        {
            for (int action = 0; action < actionMaps.Length; action++)
                actionMaps[action] = new ActionMap();

            int i = GetActionIndex(Action.UI_SELECT);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.Space);
            actionMaps[i].MouseButtons.Add(MouseButtons.LEFT);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.A);
#endif
            i = GetActionIndex(Action.UI_BACK);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.Escape);
            actionMaps[i].MouseButtons.Add(MouseButtons.RIGHT);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.B);
#endif
            i = GetActionIndex(Action.UP);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.W);
            actionMaps[i].Keys.Add(Keys.Up);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.DPadUp);
            actionMaps[i].Buttons.Add(Buttons.LeftThumbstickUp);
#endif
            i = GetActionIndex(Action.DOWN);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.S);
            actionMaps[i].Keys.Add(Keys.Down);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.DPadDown);
            actionMaps[i].Buttons.Add(Buttons.LeftThumbstickDown);
#endif
            i = GetActionIndex(Action.LEFT);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.A);
            actionMaps[i].Keys.Add(Keys.Left);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.DPadLeft);
            actionMaps[i].Buttons.Add(Buttons.LeftThumbstickLeft);
#endif
            i = GetActionIndex(Action.RIGHT);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.D);
            actionMaps[i].Keys.Add(Keys.Right);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.DPadRight);
            actionMaps[i].Buttons.Add(Buttons.LeftThumbstickRight);
#endif
            i = GetActionIndex(Action.LK);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.K);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.A);
#endif
            i = GetActionIndex(Action.HK);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.L);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.B);
#endif
            i = GetActionIndex(Action.LP);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.J);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.X);
#endif
            i = GetActionIndex(Action.HP);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.I);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.Y);
#endif
            i = GetActionIndex(Action.START);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.Enter);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.Start);
#endif
            i = GetActionIndex(Action.LB);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.E);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.LeftShoulder);
#endif
            i = GetActionIndex(Action.LT);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.D3);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.LeftTrigger);
#endif
            i = GetActionIndex(Action.RB);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.O);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.RightShoulder);
#endif
            i = GetActionIndex(Action.RT);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.D9);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.RightTrigger);
#endif
            i = GetActionIndex(Action.DEBUG);
#if DESKTOP
            actionMaps[i].Keys.Add(Keys.F1);
#endif
#if DESKTOP || CONSOLE
            actionMaps[i].Buttons.Add(Buttons.Back);
#endif

#if DESKTOP
            i = GetActionIndex(Action.CONSOLE);
            actionMaps[i].Keys.Add(Keys.OemTilde);
#endif
        }


        // Returns a platform button, given an action.
        public static Texture2D GetPlatformButton(Buttons button)
        {
            return platformButtons[button];
        }


        // Check if an action was just performed in the most recent update.
        public static bool IsActionPressed(Action action, User user, bool checkLastFrame = true)
        {
            if (action == Action.CONSOLE)
                return CurrentKeyboardState.IsKeyDown(Keys.OemTilde) && LastKeyboardState.IsKeyUp(Keys.OemTilde);

            if (user == null /*|| ignoredActions[user].Contains(action)*/)
                return false;

            ActionMap actionMap = user.ActionMaps[GetActionIndex(action)];

            switch (user.InputType)
            {
                case InputType.KEYBOARD:
                    {
                        for (int i = 0; i < actionMap.Keys.Count; i++)
                        {
                            Keys key = actionMap.Keys[i];

                            if (CurrentKeyboardState.IsKeyDown(key))
                            {
                                if (checkLastFrame)
                                {
                                    if (LastKeyboardState.IsKeyUp(key))
                                        return true;
                                }
                                else
                                    return true;
                            }
                        }

                        for (int i = 0; i < actionMap.MouseButtons.Count; i++)
                        {
                            switch (actionMap.MouseButtons[i])
                            {
                                case MouseButtons.LEFT:
                                    {
                                        if (CurrentMouseState.LeftButton == ButtonState.Pressed)
                                        {
                                            if (checkLastFrame)
                                            {
                                                if (LastMouseState.LeftButton == ButtonState.Released)
                                                    return true;
                                            }
                                            else
                                                return true;
                                        }
                                    }
                                    break;
                                case MouseButtons.RIGHT:
                                    {
                                        if (CurrentMouseState.RightButton == ButtonState.Pressed)
                                        {
                                            if (checkLastFrame)
                                            {
                                                if (LastMouseState.RightButton == ButtonState.Released)
                                                    return true;
                                            }
                                            else
                                                return true;
                                        }
                                    }
                                    break;
                                case MouseButtons.MIDDLE:
                                    {
                                        if (CurrentMouseState.MiddleButton == ButtonState.Pressed)
                                        {
                                            if (checkLastFrame)
                                            {
                                                if (LastMouseState.MiddleButton == ButtonState.Released)
                                                    return true;
                                            }
                                            else
                                                return true;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                case InputType.GAMEPAD:
                    {
                        for (int i = 0; i < actionMap.Buttons.Count; i++)
                        {
                            Buttons button = actionMap.Buttons[i];

                            if (CurrentGamePadState[user.ControllerIndex].IsButtonDown(button))
                            {
                                if (checkLastFrame)
                                {
                                    if (LastGamePadState[user.ControllerIndex].IsButtonUp(button))
                                        return true;
                                }
                                else
                                    return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }


        // Returns the index in Actions, given an action.
        public static int GetActionIndex(Action action)
        {
            for (int i = 0; i < Actions.Length; i++)
            {
                if (Actions[i] == action)
                    return i;
            }

            return -1;
        }


        // Returns the name in ActionNames, given an action.
        public static string GetActionFriendlyName(Action action)
        {
            return ActionNames[GetActionIndex(action)];
        }


        // Maps a new key to a given action.
        public static void SetActionMap(User user, Action action, Keys key)
        {
            ActionMap[] actionMaps = user.ActionMaps;
            ActionMap targetActionMap = actionMaps[GetActionIndex(action)];

            if (targetActionMap.Keys.Contains(key) == false)
                targetActionMap.Keys.Add(key);

            for (int i = 0; i < actionMaps.Length; i++)
            {
                if (actionMaps[i].Keys.Contains(key))
                    actionMaps[i].Keys.Remove(key);
            }
        }


        // Maps a new button to a given action.
        public static void SetActionMap(User user, Action action, Buttons button)
        {
            ActionMap[] actionMaps = user.ActionMaps;
            ActionMap targetActionMap = actionMaps[GetActionIndex(action)];

            if (targetActionMap.Buttons.Contains(button) == false)
                targetActionMap.Buttons.Add(button);

            for (int i = 0; i < actionMaps.Length; i++)
            {
                if (actionMaps[i].Buttons.Contains(button))
                    actionMaps[i].Buttons.Remove(button);
            }
        }


        // Set the entire actionMaps of a user, based on a given actionMaps.
        public static void SetActionMaps(User user, ActionMap[] actionMaps)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i] == user)
                {
                    for (int j = 0; j < actionMaps.Length; j++)
                    {
                        Users[i].ActionMaps[j].Keys = new List<Keys>(actionMaps[j].Keys);
                        Users[i].ActionMaps[j].MouseButtons = new List<MouseButtons>(actionMaps[j].MouseButtons);
                        Users[i].ActionMaps[j].Buttons = new List<Buttons>(actionMaps[j].Buttons);
                    }

                    break;
                }
            }
        }
    }
}