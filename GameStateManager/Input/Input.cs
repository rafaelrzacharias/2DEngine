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


    // Helper for reading input from keyboard, gamepad, and touch input. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        // Number of users that the Input system with keep track off.
        private const int MAX_USERS = 4;
        public static List<User> Users;

#if DESKTOP
        // Key that pressed last frame.
        private static Keys pressedKey;

        // Timer for key repeating.
        private static float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private static float keyRepeatStartDuration = 0.3f;

        // Key repeat duration in seconds after the first key press.
        private static float keyRepeatDuration = 0.05f;
        
        private static Dictionary<Action, List<Keys>> keys;
        private static Dictionary<Action, MouseButton> mouseButtons;
#endif
        private static Dictionary<Action, List<Buttons>> buttons;


        public static void Initialize()
        {
            Users = new List<User>(MAX_USERS);

            for (int i = 0; i < MAX_USERS; i++)
                Users.Add(new User());

            Users[0].IsPrimaryUser = true;
#if DESKTOP
            keys = new Dictionary<Action, List<Keys>>
            {
                { Action.UI_UP, new List<Keys> { Keys.Up, Keys.W } },
                { Action.UI_DOWN, new List<Keys> { Keys.Down, Keys.S } },
                { Action.UI_LEFT, new List<Keys> { Keys.Left, Keys.A } },
                { Action.UI_RIGHT, new List<Keys> { Keys.Right, Keys.D } },
                { Action.UI_CONFIRM, new List<Keys> { Keys.Space } },
                { Action.UI_BACK, new List<Keys> { Keys.Escape, Keys.Back } },
                { Action.UI_PAGE_LEFT, new List<Keys> { Keys.Q, Keys.PageUp } },
                { Action.UI_PAGE_RIGHT, new List<Keys> { Keys.E, Keys.PageDown } },
                { Action.DEBUG, new List<Keys> { Keys.OemTilde } },
                { Action.PAUSE, new List<Keys> { Keys.Enter } }
            };

            mouseButtons = new Dictionary<Action, MouseButton>
            {
                { Action.UI_CONFIRM, MouseButton.LEFT },
                { Action.UI_BACK, MouseButton.RIGHT }
            };
#endif
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

            Users[(int)user.Index].IsPrimaryUser = true;
        }


        // Reads the latest state of the keyboard and gamepad.
        public static void Update()
        {
            for (int i = 0; i < MAX_USERS; i++)
                Users[i].UpdateInput();
        }

        // Event raised when a controller is disconnected.
        public delegate void ControllerDisconnectedEventHandler(User user);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(User user)
        {
            IISMessageBoxScreen messageBox = new IISMessageBoxScreen(
                "Reconnect controller " + user.Index.ToString() + " and press any key to continue.");

            messageBox.OnShow();

            if (ControllerDisconnected != null)
                ControllerDisconnected.Invoke(user);
        }


#if DESKTOP
        // Checks if the mouse is currently hovering an interactible area.
        public static bool IsMouseOver(Rectangle area)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                {
                    Point mousePosition = Users[i].CurrentMouseState.Position;
                    return (mousePosition.X > area.X && mousePosition.X < area.X + area.Width &&
                        mousePosition.Y > area.Y && mousePosition.Y < area.Y + area.Height);
                }
            }

            return false;
        }
#endif

        // Checks if any key was pressed on each connected gamepad, keyboard and mouse.
        public static bool WasAnyButtonPressed()
        {
            if (WasButtonPressed(Action.DEBUG).Count > 0)
                return false;

            for (int i = 0; i < MAX_USERS; i++)
            {
#if DESKTOP
                if (Users[i].CurrentKeyboardState != Users[i].LastKeyboardState)
                {
                    Users[i].InputType = InputType.KEYBOARD;
                    SetPrimaryUser(Users[i]);
                    return true;
                }

                if (Users[i].CurrentMouseState != Users[i].LastMouseState)
                {
                    if (Users[i].CurrentMouseState.Position != Users[i].LastMouseState.Position)
                        continue;

                    Users[i].InputType = InputType.KEYBOARD;
                    SetPrimaryUser(Users[i]);
                    return true;
                }
#endif
                if (Users[i].CurrentGamePadState.IsConnected && Users[i].LastGamePadState.IsConnected && // I might not need to check for IsConnected...
                    Users[i].CurrentGamePadState != Users[i].LastGamePadState)
                {
                    if (Users[i].CurrentGamePadState.ThumbSticks.Left.Length() != Users[i].LastGamePadState.ThumbSticks.Left.Length() ||
                        Users[i].CurrentGamePadState.ThumbSticks.Right.Length() != Users[i].LastGamePadState.ThumbSticks.Right.Length())
                        continue;

                    Users[i].InputType = InputType.GAMEPAD;
                    SetPrimaryUser(Users[i]);
                    return true;
                }
#if MOBILE
                if (Users[i].Gestures.Count != 0)
                {
                    Users[i].InputType = InputType.TOUCH;
                    SetPrimaryUser(Users[i]);
                    return true;
                }
#endif
            }

            return false;
        }


        public static Keys[] GetPressedKeys()
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentKeyboardState.GetPressedKeys();
            }

            return null;
        }


        // Checks if a key was pressed during this update. The controllingPlayer parameter specifies
        // which player to read input for. If this is null, it will accept input from any player.
        // When a keypress is detected, the output playerIndex reports which player pressed it.
        public static List<User> WasButtonPressed(Action action)
        {
            List<User> users = new List<User>();

            for (int i = 0; i < MAX_USERS; i++)
            {
                switch (Users[i].InputType)
                {
                    case InputType.KEYBOARD:
                        {
                            if (keys.ContainsKey(action))
                            {
                                for (int j = 0; j < keys[action].Count; j++)
                                {
                                    if (Users[i].LastKeyboardState.IsKeyUp(keys[action][j]) &&
                                        Users[i].CurrentKeyboardState.IsKeyDown(keys[action][j]))
                                    {
                                        users.Add(Users[i]);
                                        break;
                                    }
                                }
                            }
                            
                            if (mouseButtons.ContainsKey(action))
                            {
                                switch (mouseButtons[action])
                                {
                                    case MouseButton.LEFT:
                                        {
                                            if (Users[i].LastMouseState.LeftButton == ButtonState.Released &&
                                                Users[i].CurrentMouseState.LeftButton == ButtonState.Pressed)
                                            {
                                                users.Add(Users[i]);
                                            }
                                        }
                                        break;
                                    case MouseButton.RIGHT:
                                        {
                                            if (Users[i].LastMouseState.RightButton == ButtonState.Released &&
                                                Users[i].CurrentMouseState.RightButton == ButtonState.Pressed)
                                            {
                                                users.Add(Users[i]);
                                            }
                                        }
                                        break;
                                    case MouseButton.MIDDLE:
                                        {
                                            if (Users[i].LastMouseState.MiddleButton == ButtonState.Released &&
                                                Users[i].CurrentMouseState.MiddleButton == ButtonState.Pressed)
                                            {
                                                users.Add(Users[i]);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case InputType.GAMEPAD:
                        {
                            if (buttons.ContainsKey(action))
                            {
                                for (int j = 0; j < buttons[action].Count; j++)
                                {
                                    if (Users[i].LastGamePadState.IsButtonUp(buttons[action][j]) &&
                                        Users[i].CurrentGamePadState.IsButtonDown(buttons[action][j]))
                                    {
                                        users.Add(Users[i]);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return users;
        }


        // Checks if a key was pressed and allow repeating the key press after some time.
        public static bool IsKeyPressed(Keys key, float dt)
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                {
                    // Treat it as pressed if the given key wasn't pressed in previous frame.
                    if (Users[i].LastKeyboardState.IsKeyUp(key))
                    {
                        keyRepeatTimer = keyRepeatStartDuration;
                        pressedKey = key;
                        return true;
                    }
                }
            }

            // Handling key repeating if given key has pressed in previous frame.
            if (key == pressedKey)
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
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentKeyboardState.IsKeyDown(key);
            }

            return false;
        }


        // Checks whether or not the mouse has moved on a player controlling a keyboard and mouse.
        public static bool HasMouseMoved()
        { 
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentMouseState.Position != Users[i].LastMouseState.Position;
            }

            return false;
        }


        // Checks whether or not the scroll wheel changed.
        public static bool HasScrollWheelChanged()
        {
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentMouseState.ScrollWheelValue != Users[i].LastMouseState.ScrollWheelValue;
            }

            return false;
        }


        // Helper for checking if a mouse button is currently being pressed on this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a mouse click is detected, the output playerIndex reports which player performed the click.
        public static bool IsMouseDown(MouseButton mouseButton, out PlayerIndex playerIndex, User user = null)
        {
            if (user != null)
            {
                // Read mouse input from the specified player.
                playerIndex = user.Index;

                if (user.InputType == InputType.KEYBOARD)
                {
                    switch (mouseButton)
                    {
                        case MouseButton.LEFT:
                            return user.CurrentMouseState.LeftButton == ButtonState.Pressed;
                        case MouseButton.MIDDLE:
                            return user.CurrentMouseState.MiddleButton == ButtonState.Pressed;
                        case MouseButton.RIGHT:
                            return user.CurrentMouseState.RightButton == ButtonState.Pressed;
                    }

                    return false;
                }

                return false;
            }
            else
            {
                bool result = false;
                playerIndex = PlayerIndex.One;

                // Accept input from any player.
                for (int i = 0; i < Users.Count; i++)
                {
                    result = IsMouseDown(mouseButton, out playerIndex, Users[i]);

                    if (result)
                    {
                        playerIndex = Users[i].Index;
                        return result;
                    }
                }

                return result;
            }
        }
    }
}