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


    public enum MouseButton
    {
        Left,
        Middle,
        Right
    }


    // Helper for reading input from keyboard, gamepad, and touch input. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        // Number of users that the Input system with keep track off.
        private const int MAX_USERS = 4;
        public static List<User> Users;

        // Key that pressed last frame.
        private static Keys pressedKey;

        // Timer for key repeating.
        private static float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private static float keyRepeatStartDuration = 0.3f;

        // Key repeat duration in seconds after the first key press.
        private static float keyRepeatDuration = 0.05f;


        public static void Initialize()
        {
            Users = new List<User>(MAX_USERS);

            for (int i = 0; i < MAX_USERS; i++)
                Users.Add(new User());

            Users[0].IsPrimaryUser = true;
        }


        // Gets the primary user.
        public static User GetPrimaryUser()
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].IsPrimaryUser)
                    return Users[i];
            }

            return null;
        }


        // Sets the primary user.
        public static void SetPrimaryUser(PlayerIndex playerIndex)
        {
            for (int i = 0; i < Users.Count; i++)
                Users[i].IsPrimaryUser = false;

            Users[(int)playerIndex].IsPrimaryUser = true;
        }


        // Reads the latest state of the keyboard and gamepad.
        public static void Update()
        {
            for (int i = 0; i < Users.Count; i++)
                Users[i].UpdateInput();
        }

        // Event raised when a controller is disconnected.
        public delegate void ControllerDisconnectedEventHandler(PlayerIndex playerIndex);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(PlayerIndex playerIndex)
        {
            IISMessageBoxScreen messageBox = new IISMessageBoxScreen(
                "Reconnect controller " + playerIndex.ToString() + " and press any key to continue.");

            messageBox.OnShow();

            if (ControllerDisconnected != null)
                ControllerDisconnected.Invoke(playerIndex);
        }


        // Checks if the mouse is currently hovering an interactible area.
        public static bool IsMouseOver(Rectangle area)
        {
            for (int i = 0; i < Users.Count; i++)
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


        // Checks if any key was pressed on each connected gamepad, keyboard and mouse.
        public static bool WasAnyButtonPressed(out PlayerIndex playerIndex)
        {
            if (WasKeyPressed(Keys.OemTilde, out playerIndex))
                return false;

            for (int i = 0; i < Users.Count; i++)
            {
                switch (Users[i].InputType)
                {
                    case InputType.KEYBOARD:
                        {
                            if (Users[i].CurrentKeyboardState != Users[i].LastKeyboardState)
                            {
                                SetPrimaryUser(Users[i].Index);
                                GetPrimaryUser().InputType = InputType.KEYBOARD; // WFT???
                                return true;
                            }

                            if (WasMouseClicked(MouseButton.Left, out playerIndex) ||
                                WasMouseClicked(MouseButton.Middle, out playerIndex) ||
                                WasMouseClicked(MouseButton.Right, out playerIndex))
                            //if (Users[i].CurrentMouseState != Users[i].LastMouseState)
                            {
                                SetPrimaryUser(Users[i].Index);
                                GetPrimaryUser().InputType = InputType.KEYBOARD; // WTF???
                                return true;
                            }
                        }
                        break;
                    case InputType.GAMEPAD:
                        {
                            if (Users[i].CurrentGamePadState.IsConnected && Users[i].LastGamePadState.IsConnected && // I might not need to check for IsConnected...
                                Users[i].CurrentGamePadState != Users[i].LastGamePadState)
                            {
                                if (Users[i].CurrentGamePadState.ThumbSticks.Left.Length() != Users[i].LastGamePadState.ThumbSticks.Left.Length() ||
                                    Users[i].CurrentGamePadState.ThumbSticks.Right.Length() != Users[i].LastGamePadState.ThumbSticks.Right.Length())
                                    continue;

                                SetPrimaryUser(Users[i].Index);
                                GetPrimaryUser().InputType = InputType.GAMEPAD; // WFT???
                                return true;
                            }
                        }
                        break;
                    case InputType.TOUCH:
                        {
                            if (Users[i].Gestures.Count != 0)
                            {
                                SetPrimaryUser(Users[i].Index);
                                GetPrimaryUser().InputType = InputType.TOUCH; // WFT???
                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }


        public static Keys[] GetPressedKeys()
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentKeyboardState.GetPressedKeys();
            }

            return null;
        }


        // Checks if a key was pressed during this update. The controllingPlayer parameter specifies
        // which player to read input for. If this is null, it will accept input from any player.
        // When a keypress is detected, the output playerIndex reports which player pressed it.
        public static bool WasKeyPressed(Keys key, out PlayerIndex playerIndex, User user = null)
        {
            // Read keyboard input from the specified player, if he is a keyboard user.
            if (user != null)
            {
                playerIndex = user.Index;

                if (user.InputType == InputType.KEYBOARD)
                    return user.CurrentKeyboardState.IsKeyDown(key) && user.LastKeyboardState.IsKeyUp(key);

                return false;
            }
            else
            {
                bool result = false;
                playerIndex = PlayerIndex.One;

                // Accept input from any player.
                for (int i = 0; i < Users.Count; i++)
                {
                    result = WasKeyPressed(key, out playerIndex, Users[i]);

                    if (result)
                    {
                        playerIndex = Users[i].Index;
                        return result;
                    }
                }

                return result;
            }
        }


        // Checks if a key was pressed and allow repeating the key press after some time.
        public static bool IsKeyPressed(Keys key, float dt)
        {
            for (int i = 0; i < Users.Count; i++)
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
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentKeyboardState.IsKeyDown(key);
            }

            return false;
        }


        // Checks whether or not the mouse has moved on a player controlling a keyboard and mouse.
        public static bool HasMouseMoved()
        { 
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentMouseState.Position != Users[i].LastMouseState.Position;
            }

            return false;
        }


        // Checks whether or not the scroll wheel changed.
        public static bool HasScrollWheelChanged()
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].InputType == InputType.KEYBOARD)
                    return Users[i].CurrentMouseState.ScrollWheelValue != Users[i].LastMouseState.ScrollWheelValue;
            }

            return false;
        }


        // Helper for checking if a mouse button was newly pressed during this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a mouse click is detected, the output playerIndex reports which player performed the click.
        public static bool WasMouseClicked(MouseButton mouseButton, out PlayerIndex playerIndex, User user = null)
        {
            if (user != null)
            {
                // Read mouse input from the specified player.
                playerIndex = user.Index;

                if (user.InputType == InputType.KEYBOARD)
                {
                    switch (mouseButton)
                    {
                        case MouseButton.Left:
                            return user.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                                user.LastMouseState.LeftButton == ButtonState.Released;
                        case MouseButton.Middle:
                            return user.CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                                user.LastMouseState.MiddleButton == ButtonState.Released;
                        case MouseButton.Right:
                            return user.CurrentMouseState.RightButton == ButtonState.Pressed &&
                                user.LastMouseState.RightButton == ButtonState.Released;
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
                    result = WasMouseClicked(mouseButton, out playerIndex, Users[i]);

                    if (result)
                    {
                        playerIndex = Users[i].Index;
                        return result;
                    }
                }

                return result;
            }
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
                        case MouseButton.Left:
                            return user.CurrentMouseState.LeftButton == ButtonState.Pressed;
                        case MouseButton.Middle:
                            return user.CurrentMouseState.MiddleButton == ButtonState.Pressed;
                        case MouseButton.Right:
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


        // Helper for checking if a button was newly pressed during this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a button press is detected, the output playerIndex reports which player pressed it.
        public static bool WasButtonPressed(Buttons button, out PlayerIndex playerIndex, User user = null)
        {
            if (user != null)
            {
                // Read gamepad input from the specified player.
                playerIndex = user.Index;

                if (user.InputType == InputType.GAMEPAD)
                    return user.CurrentGamePadState.IsButtonDown(button) && user.LastGamePadState.IsButtonUp(button);

                return false;
            }
            else
            {
                bool result = false;
                playerIndex = PlayerIndex.One;

                // Accept input from any player.
                for (int i = 0; i < Users.Count; i++)
                {
                    result = WasButtonPressed(button, out playerIndex, Users[i]);

                    if (result)
                    {
                        playerIndex = Users[i].Index;
                        return result;
                    }
                }

                return result;
            }
        }


        // Checks for a "pause the game" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player.
        public static bool WasGamePaused(out PlayerIndex playerIndex, User user = null)
        {
            // Read gamepad input from the specified player.
            if (user != null)
            {
                playerIndex = user.Index;

                switch (user.InputType)
                {
                    case InputType.KEYBOARD:
                        return WasKeyPressed(Keys.Escape, out playerIndex, user);
                    case InputType.GAMEPAD:
                        return WasButtonPressed(Buttons.Start, out playerIndex, user);
                    //case InputType.TOUCH:
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
                    result = WasGamePaused(out playerIndex, Users[i]);

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