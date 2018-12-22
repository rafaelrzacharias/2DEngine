using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace GameStateManager
{
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


    // A struct that represents an input state from a controller.
    public struct ActionState
    {
        public bool IsPressed; // If the action is being pressed.
        public bool IsTriggered; // If the action was just pressed this frame.
        public float Magnitude; // The action's intensity (for analog buttons).
        public double Duration; // How long the action has being pressed for.


        // Reset all fields to its default values.
        public void Reset()
        {
            IsPressed = false;
            IsTriggered = false;
            Magnitude = 0.0f;
            Duration = 0.0;
        }
    }


    // Helper for reading input from keyboard, gamepad, and touchscreen. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        public static GameTime GameTime { get; private set; }

        public const int MAX_USERS = 4;
        public static Controller[] Controllers;

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

        private static Buttons[] buttonList;
        private static Action previousAction;
        private static double elapsedTime;
        private const double ELAPSED_LIMIT = 0.2;
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
            buttonList = (Buttons[])Enum.GetValues(typeof(Buttons));

            LastGamePadState = new GamePadState[MAX_USERS];
            CurrentGamePadState = new GamePadState[MAX_USERS];
            Controllers = new Controller[MAX_USERS];

            for (int i = 0; i < MAX_USERS; i++)
            {
                LastGamePadState[i] = new GamePadState();
                CurrentGamePadState[i] = new GamePadState();
                Controllers[i] = new Controller();
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
        }


        // Gets the primary user.
        public static Controller GetPrimaryUser()
        {
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Controllers[i].IsPrimaryUser)
                    return Controllers[i];
            }

            return null;
#endif
#if MOBILE
            return Users[0];
#endif
        }


        // Sets the primary user.
        public static void SetPrimaryUser(Controller controller)
        {
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
                Controllers[i].IsPrimaryUser = false;

            controller.IsPrimaryUser = true;
#endif
#if MOBILE
            Users[0].IsPrimaryUser = true;
#endif
        }


        // Set the user controller type based on the given controller slot.
        public static void SetUserControllerType(Controller controller, int slot)
        {
#if DESKTOP || CONSOLE
            if (slot != MAX_USERS)
                controller.Type = ControllerType.GAMEPAD;
            else
                controller.Type = ControllerType.KEYBOARD;

            controller.Slot = slot;

            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Controllers[i].Slot == slot && Controllers[i] != controller)
                {
                    ResetUser(Controllers[i]);
                    break;
                }
            }
#endif
#if MOBILE
            Users[0].InputType = InputType.TOUCH;
#endif
        }


        // Reset the user to a default state.
        public static void ResetUser(Controller controller)
        {
#if DESKTOP
            if (GetUserCount() == 1)
            {
                controller.Slot = MAX_USERS;
                controller.Type = ControllerType.KEYBOARD;
                controller.IsPrimaryUser = true;
            }
            else
#endif
            {
                controller.Slot = -1;
                controller.Type = ControllerType.NONE;
                controller.IsPrimaryUser = false;
            }

            if (GetUserCount() == 1)
            {
                for (int i = 0; i < MAX_USERS; i++)
                {
                    if (Controllers[i].IsActive)
                    {
                        Controllers[i].IsPrimaryUser = true;
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
                if (Controllers[i].IsActive)
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
        public static void Update(GameTime gameTime)
        {
            GameTime = gameTime;
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

            int slot = -1;
            if (CanSwapControllerType && GetUserCount() == 1)
                slot = WasAnyButtonPressed(false, false);

            if (slot != -1)
            {
                Controller controller = GetPrimaryUser();
                for (int i = 0; i < MAX_USERS; i++)
                {
                    if (Controllers[i].Slot == slot)
                        continue;

                    switch (controller.Type)
                    {
                        case ControllerType.KEYBOARD:
                            {
                                if (slot != MAX_USERS)
                                {
                                    controller.Slot = slot;
                                    controller.Type = ControllerType.GAMEPAD;

                                    OnUserControllerTypeChanged(i);
                                }
                            }
                            break;
                        case ControllerType.GAMEPAD:
                            {
                                if (slot == MAX_USERS)
                                {
                                    controller.Slot = slot;
                                    controller.Type = ControllerType.KEYBOARD;

                                    OnUserControllerTypeChanged(i);
                                }
                            }
                            break;
                    }

                    break;
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

                if (Controllers[i].IsActive)
                {
                    Controllers[i].Update(gameTime);
                    BufferedInput.Update(gameTime, i);
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
        public delegate void ControllerDisconnectedEventHandler(int slot);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(int slot)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Controllers[i].Slot == slot)
                {
                    ResetUser(Controllers[i]);
                    break;
                }
            }

            if (ControllerDisconnected != null)
                ControllerDisconnected.Invoke(slot);
        }


        // Event raised when a controller is connected.
        public delegate void ControllerConnectedEventHandler(int slot);
        public static event ControllerConnectedEventHandler ControllerConnected;

        public static void OnControllerConnected(int slot)
        {
            if (ControllerConnected != null)
                ControllerConnected.Invoke(slot);
        }


        // Event raised when an active user changes its controller type.
        public delegate void ControllerTypeChangedEventHandler(int slot);
        public static event ControllerTypeChangedEventHandler ControllerTypeChanged;

        public static void OnUserControllerTypeChanged(int slot)
        {
            if (ControllerTypeChanged != null)
                ControllerTypeChanged.Invoke(slot);
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


        // Checks if a connected gamePad has changed its state since last frame.
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


        // Checks if the mouse is currently hovering an interactible area.
        public static bool IsMouseOver(Rectangle area)
        {
            return (CurrentMouseState.Position.X > area.X &&
                CurrentMouseState.Position.X < area.X + area.Width &&
                CurrentMouseState.Position.Y > area.Y &&
                CurrentMouseState.Position.Y < area.Y + area.Height);
        }


        // Checks whether or not the mouse has moved on a player controlling a keyboard and mouse.
        public static bool HasMouseMoved()
        {
            return CurrentMouseState.Position != LastMouseState.Position;
        }


        // Returns the distance that the mouse travelled since last frame.
        public static float GetMouseDistance()
        {
            return Vector2.Distance(
                CurrentMouseState.Position.ToVector2(), 
                LastMouseState.Position.ToVector2());
        }


        // Checks whether or not the scroll wheel changed.
        public static bool HasScrollWheelChanged()
        {
            return CurrentMouseState.ScrollWheelValue != LastMouseState.ScrollWheelValue;
        }


        // Returns true if the mouse wheel scrolled up.
        public static bool HasMouseScrolledUp()
        {
            return CurrentMouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue;
        }


        // Returns true if the mouse wheel scrolled down.
        public static bool HasMouseScrolledDown()
        {
            return CurrentMouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue;
        }


        // Returns the mouse wheel scroll distance since last frame.
        public static int GetMouseScrolledDistance()
        {
            return CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue;
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

        // Returns a platform button, given an action.
        public static Texture2D GetPlatformButton(Buttons button)
        {
            return platformButtons[button];
        }   


        // Returns the state of an action, given the action and the controller.
        public static ActionState GetAction(Action action, Controller controller)
        {
            if (action == Action.CONSOLE)
            {
                ActionState actionState = new ActionState();
                actionState.IsTriggered = CurrentKeyboardState.IsKeyDown(Keys.OemTilde) && LastKeyboardState.IsKeyUp(Keys.OemTilde);
                return actionState;
            }

            if (controller == null)
                return new ActionState();

            return controller.ActionStates[GetActionIndex(action)];
        }


        // Returns true if the given action was just pressed on this frame,
        // or if it's still being pressed after a certain amount of time.
        public static bool GetTimedAction(Action action, Controller controller)
        {
            if (controller == null)
                return false;

            ActionState actionState = controller.ActionStates[GetActionIndex(action)];

            // If its a new key press, return true and restart the timer.
            if (actionState.IsTriggered)
            {
                previousAction = action;
                elapsedTime = 0.0;
                return true;
            }

            // If we are currently holding down the key
            if (actionState.IsPressed && previousAction == action)
            {
                // Return true if holding for long enough and reset the timer.
                if (elapsedTime >= ELAPSED_LIMIT)
                {
                    elapsedTime = 0.0;
                    return true;
                }
                else
                {
                    // Otherwise, increase the timer by a frame's worth of time and do nothing.
                    elapsedTime += GameTime.ElapsedGameTime.TotalSeconds;
                }
            }

            // Return false if we either don't have a new key press,
            // or we have one but haven't held it for long enough.
            return false;
        }


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
        public static void SetActionMap(Controller controller, Action action, Keys key)
        {
            ActionMap[] actionMaps = controller.ActionMaps;
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
        public static void SetActionMap(Controller controller, Action action, Buttons button)
        {
            ActionMap[] actionMaps = controller.ActionMaps;
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
        public static void SetActionMaps(Controller controller, ActionMap[] actionMaps)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Controllers[i] == controller)
                {
                    for (int j = 0; j < actionMaps.Length; j++)
                    {
                        Controllers[i].ActionMaps[j].Keys = new List<Keys>(actionMaps[j].Keys);
                        Controllers[i].ActionMaps[j].MouseButtons = new List<MouseButtons>(actionMaps[j].MouseButtons);
                        Controllers[i].ActionMaps[j].Buttons = new List<Buttons>(actionMaps[j].Buttons);
                    }

                    break;
                }
            }
        }
    }
}