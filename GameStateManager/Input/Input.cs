using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
    public enum MouseButton
    {
        LEFT,
        MIDDLE,
        RIGHT
    }
#endif

    public enum Action
    {
        UI_UP,
        UI_DOWN,
        UI_LEFT,
        UI_RIGHT,
        UI_CONFIRM,
        UI_BACK,
        UI_PAGE_LEFT,
        UI_PAGE_RIGHT,
        DEBUG,
        PAUSE
    }


    // Helper for reading input from keyboard, gamepad, and touchscreen. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        public const int MAX_USERS = 4;
        public static User[] Users;
#if DESKTOP
        public static KeyboardState LastKeyboardState;
        public static KeyboardState CurrentKeyboardState;
        public static MouseState LastMouseState;
        public static MouseState CurrentMouseState;

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

        private static Dictionary<Action, List<Keys>> keys;
        private static Dictionary<Action, MouseButton> mouseButtons;
#endif
#if DESKTOP || CONSOLE
        public static GamePadState[] LastGamePadState;
        public static GamePadState[] CurrentGamePadState;

        private static Dictionary<Action, List<Buttons>> buttons;
#endif
#if MOBILE
        public static TouchCollection TouchState;
        public static GestureSample[] Gestures;

        private static Dictionary<Action, List<GestureSample>> gestures;
#endif


        public static void Initialize()
        {
#if DESKTOP
            LastKeyboardState = new KeyboardState();
            CurrentMouseState = new MouseState();
            LastMouseState = new MouseState();
            CurrentKeyboardState = new KeyboardState();

            keys = new Dictionary<Action, List<Keys>>
            {
                { Action.UI_UP, new List<Keys> { Keys.Up, Keys.W } },
                { Action.UI_DOWN, new List<Keys> { Keys.Down, Keys.S } },
                { Action.UI_LEFT, new List<Keys> { Keys.Left, Keys.A } },
                { Action.UI_RIGHT, new List<Keys> { Keys.Right, Keys.D } },
                { Action.UI_CONFIRM, new List<Keys> { Keys.Space } },
                { Action.UI_BACK, new List<Keys> { Keys.Escape } },
                { Action.UI_PAGE_LEFT, new List<Keys> { Keys.Q, Keys.PageUp } },
                { Action.UI_PAGE_RIGHT, new List<Keys> { Keys.E, Keys.PageDown } },
                { Action.DEBUG, new List<Keys> { Keys.OemTilde } },
                { Action.PAUSE, new List<Keys> { Keys.Escape } }
            };

            mouseButtons = new Dictionary<Action, MouseButton>
            {
                { Action.UI_CONFIRM, MouseButton.LEFT },
                { Action.UI_BACK, MouseButton.RIGHT }
            };
#endif
#if DESKTOP || CONSOLE
            LastGamePadState = new GamePadState[MAX_USERS];
            CurrentGamePadState = new GamePadState[MAX_USERS];

            for (int i = 0; i < MAX_USERS; i++)
            {
                LastGamePadState[i] = new GamePadState();
                CurrentGamePadState[i] = new GamePadState();
            }

            buttons = new Dictionary<Action, List<Buttons>>
            {
                { Action.UI_UP, new List<Buttons> { Buttons.DPadUp, Buttons.LeftThumbstickUp } },
                { Action.UI_DOWN, new List<Buttons> { Buttons.DPadDown, Buttons.LeftThumbstickDown } },
                { Action.UI_LEFT, new List<Buttons> { Buttons.DPadLeft, Buttons.LeftThumbstickLeft } },
                { Action.UI_RIGHT, new List<Buttons> { Buttons.DPadRight, Buttons.LeftThumbstickRight } },
                { Action.UI_CONFIRM, new List<Buttons> { Buttons.A } },
                { Action.UI_BACK, new List<Buttons> { Buttons.B } },
                { Action.UI_PAGE_LEFT, new List<Buttons> { Buttons.LeftShoulder } },
                { Action.UI_PAGE_RIGHT, new List<Buttons> { Buttons.RightShoulder } },
                { Action.DEBUG, new List<Buttons> { Buttons.Back } },
                { Action.PAUSE, new List<Buttons> { Buttons.Start } }
            };
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

            Users[0].IsPrimaryUser = true;
        }


        // Gets the primary user.
        public static User GetPrimaryUser()
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].IsPrimaryUser)
                    return Users[i];
            }

            return null;
        }


        // Sets the primary user.
        public static void SetPrimaryUser(User user)
        {
            for (int i = 0; i < MAX_USERS; i++)
                Users[i].IsPrimaryUser = false;

            user.IsPrimaryUser = true;
        }


        // Reads the latest state of the keyboard and gamepad.
        public static void Update()
        {
#if DESKTOP
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

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

            // If dragging distance was above threshold (5 pixels), set dragging to true.
            if (isLeftMouseDown && isDragging == false)
            {
                Vector2 delta = dragMouseStart - CurrentMouseState.Position.ToVector2();

                if (delta.Length() > dragThreshold)
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


        // Event raised when a controller is disconnected.
        public delegate void ControllerDisconnectedEventHandler(int controllerIndex);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(int controllerIndex)
        {
            if (ControllerDisconnected != null)
                ControllerDisconnected.Invoke(controllerIndex);
        }


        // Event raised when a controller is disconnected.
        public delegate void ControllerConnectedEventHandler(int controllerIndex);
        public static event ControllerConnectedEventHandler ControllerConnected;

        public static void OnControllerConnected(int controllerIndex)
        {
            if (ControllerConnected != null)
                ControllerConnected.Invoke(controllerIndex);
        }


#if DESKTOP
        // Checks if the mouse is currently hovering an interactible area.
        public static bool IsMouseOver(Rectangle area)
        {
            return (CurrentMouseState.Position.X > area.X && CurrentMouseState.Position.X < area.X + area.Width &&
                CurrentMouseState.Position.Y > area.Y && CurrentMouseState.Position.Y < area.Y + area.Height);
        }
#endif

        // Checks if any key was pressed on each connected gamepad, keyboard and mouse.
        public static bool WasAnyButtonPressed(bool ignoreMouseMovement = true, bool ignoreThumbStickMovement = true)
        {
#if DESKTOP
            if (CurrentKeyboardState != LastKeyboardState)
                return true;

            if (CurrentMouseState != LastMouseState)
            {
                if (ignoreMouseMovement == false || CurrentMouseState.Position == LastMouseState.Position)
                    return true;
            }
#endif
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (CurrentGamePadState[i] != LastGamePadState[i])
                {
                    if (ignoreThumbStickMovement == false ||
                        (CurrentGamePadState[i].ThumbSticks.Left.Length() == LastGamePadState[i].ThumbSticks.Left.Length() &&
                        CurrentGamePadState[i].ThumbSticks.Right.Length() == LastGamePadState[i].ThumbSticks.Right.Length()))
                        return true;
                }
            }
#endif
#if MOBILE
            if (Gestures.Length != 0)
                return true;
#endif

            return false;
        }


        // Checks if a key was pressed during this update. The controllingPlayer parameter specifies
        // which player to read input for. If this is null, it will accept input from any player.
        // When a keypress is detected, the output playerIndex reports which player pressed it.
        public static bool WasButtonPressed(Action action, User user = null)
        {
#if DESKTOP
            if (keys.ContainsKey(action))
            {
                for (int i = 0; i < keys[action].Count; i++)
                {
                    if (LastKeyboardState.IsKeyUp(keys[action][i]) && CurrentKeyboardState.IsKeyDown(keys[action][i]))
                        return true;
                }
            }

            if (mouseButtons.ContainsKey(action))
            {
                switch (mouseButtons[action])
                {
                    case MouseButton.LEFT:
                        {
                            if (LastMouseState.LeftButton == ButtonState.Released && CurrentMouseState.LeftButton == ButtonState.Pressed)
                                return true;
                        }
                        break;
                    case MouseButton.RIGHT:
                        {
                            if (LastMouseState.RightButton == ButtonState.Released && CurrentMouseState.RightButton == ButtonState.Pressed)
                                return true;
                        }
                        break;
                    case MouseButton.MIDDLE:
                        {
                            if (LastMouseState.MiddleButton == ButtonState.Released && CurrentMouseState.MiddleButton == ButtonState.Pressed)
                                return true;
                        }
                        break;
                }
            }
#endif
#if DESKTOP || CONSOLE
            if (buttons.ContainsKey(action))
            {
                for (int i = 0; i < buttons[action].Count; i++)
                {
                    for (int j = 0; j < MAX_USERS; j++)
                    {
                        if (LastGamePadState[j].IsButtonUp(buttons[action][i]) && CurrentGamePadState[j].IsButtonDown(buttons[action][i]))
                            return true;
                    }
                }
            }
#endif
#if MOBILE

#endif
            return false;

            //int i;
            //if (user != null)
            //    i = user.ControllerIndex;
            //else
            //    i = 0;

            //while (i < MAX_USERS)
            //{
            //    switch (Users[i].InputType)
            //    {
            //        case InputType.KEYBOARD:
            //            {
            //                if (keys.ContainsKey(action))
            //                {
            //                    for (int j = 0; j < keys[action].Count; j++)
            //                    {
            //                        if (Users[i].LastKeyboardState.IsKeyUp(keys[action][j]) &&
            //                            Users[i].CurrentKeyboardState.IsKeyDown(keys[action][j]))
            //                            return true;
            //                    }
            //                }

            //                if (mouseButtons.ContainsKey(action))
            //                {
            //                    switch (mouseButtons[action])
            //                    {
            //                        case MouseButton.LEFT:
            //                            {
            //                                if (Users[i].LastMouseState.LeftButton == ButtonState.Released &&
            //                                    Users[i].CurrentMouseState.LeftButton == ButtonState.Pressed)
            //                                    return true;
            //                            }
            //                            break;
            //                        case MouseButton.RIGHT:
            //                            {
            //                                if (Users[i].LastMouseState.RightButton == ButtonState.Released &&
            //                                    Users[i].CurrentMouseState.RightButton == ButtonState.Pressed)
            //                                    return true;
            //                            }
            //                            break;
            //                        case MouseButton.MIDDLE:
            //                            {
            //                                if (Users[i].LastMouseState.MiddleButton == ButtonState.Released &&
            //                                    Users[i].CurrentMouseState.MiddleButton == ButtonState.Pressed)
            //                                    return true;
            //                            }
            //                            break;
            //                    }
            //                }
            //            }
            //            break;
            //        case InputType.GAMEPAD:
            //            {
            //                if (buttons.ContainsKey(action))
            //                {
            //                    for (int j = 0; j < buttons[action].Count; j++)
            //                    {
            //                        if (Users[i].LastGamePadState.IsButtonUp(buttons[action][j]) &&
            //                            Users[i].CurrentGamePadState.IsButtonDown(buttons[action][j]))
            //                            return true;
            //                    }
            //                }
            //            }
            //            break;
            //    }

            //    if (user != null && i == user.controllerIndex)
            //        return false;

            //    i++;
            //}
            //return false;
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
        public static bool IsMouseDown(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.LEFT:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed;
                case MouseButton.MIDDLE:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                case MouseButton.RIGHT:
                    return CurrentMouseState.RightButton == ButtonState.Pressed;
            }

            return false;
        }
    }
#endif
}