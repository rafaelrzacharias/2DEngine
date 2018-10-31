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
        private static List<User> Users;

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
            if (WasKeyPressed(Keys.OemTilde, GetPrimaryUser().Index, out playerIndex))
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

                            if (WasMouseClicked(MouseButton.Left, GetPrimaryUser().Index, out playerIndex) ||
                                WasMouseClicked(MouseButton.Middle, GetPrimaryUser().Index, out playerIndex) ||
                                WasMouseClicked(MouseButton.Right, GetPrimaryUser().Index, out playerIndex))
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


        // Checks if a key was pressed during this update. The controllingPlayer parameter specifies
        // which player to read input for. If this is null, it will accept input from any player.
        // When a keypress is detected, the output playerIndex reports which player pressed it.
        public static bool WasKeyPressed(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read keyboard input from the specified player.
                playerIndex = controllingPlayer.Value;
                return CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
            }
            else
            {
                // Accept input from any player.
                return (WasKeyPressed(key, PlayerIndex.One, out playerIndex) ||
                        WasKeyPressed(key, PlayerIndex.Two, out playerIndex) ||
                        WasKeyPressed(key, PlayerIndex.Three, out playerIndex) ||
                        WasKeyPressed(key, PlayerIndex.Four, out playerIndex));
            }
        }


        // Checks if a key was pressed and allow repeating the key press after some time.
        public static bool IsKeyPressed(Keys key, float dt)
        {
            // Treat it as pressed if the given key wasn't pressed in previous frame.
            if (LastKeyboardState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                pressedKey = key;
                return true;
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
        public static bool WasMouseClicked(MouseButton mouseButton, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read mouse input from the specified player.
                playerIndex = controllingPlayer.Value;

                switch (mouseButton)
                {
                    case MouseButton.Left:
                        return CurrentMouseState.LeftButton == ButtonState.Pressed &&
                            LastMouseState.LeftButton == ButtonState.Released;
                    case MouseButton.Middle:
                        return CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                            LastMouseState.MiddleButton == ButtonState.Released;
                    case MouseButton.Right:
                        return CurrentMouseState.RightButton == ButtonState.Pressed &&
                            LastMouseState.RightButton == ButtonState.Released;
                    default:
                        return false;
                }
            }
            else
            {
                // Accept input from any player.
                return (WasMouseClicked(mouseButton, PlayerIndex.One, out playerIndex) ||
                        WasMouseClicked(mouseButton, PlayerIndex.Two, out playerIndex) ||
                        WasMouseClicked(mouseButton, PlayerIndex.Three, out playerIndex) ||
                        WasMouseClicked(mouseButton, PlayerIndex.Four, out playerIndex));
            }
        }


        // Helper for checking if a mouse button is currently being pressed on this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a mouse click is detected, the output playerIndex reports which player performed the click.
        public static bool IsMouseDown(MouseButton mouseButton, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read mouse input from the specified player.
                playerIndex = controllingPlayer.Value;

                switch (mouseButton)
                {
                    case MouseButton.Left:
                        return CurrentMouseState.LeftButton == ButtonState.Pressed;
                    case MouseButton.Middle:
                        return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                    case MouseButton.Right:
                        return CurrentMouseState.RightButton == ButtonState.Pressed;
                    default:
                        return false;
                }
            }
            else
            {
                // Accept input from any player.
                return (IsMouseDown(mouseButton, PlayerIndex.One, out playerIndex) ||
                        IsMouseDown(mouseButton, PlayerIndex.Two, out playerIndex) ||
                        IsMouseDown(mouseButton, PlayerIndex.Three, out playerIndex) ||
                        IsMouseDown(mouseButton, PlayerIndex.Four, out playerIndex));
            }
        }


        // Helper for checking if a button was newly pressed during this update. The controllingPlayer
        // parameter specifies which player to read input for. If this is null, it will accept input from any
        // player. When a button press is detected, the output playerIndex reports which player pressed it.
        public static bool WasButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read gamepad input from the specified player.
                playerIndex = controllingPlayer.Value;

                return CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonUp(button);
            }
            else
            {
                // Accept input from any player.
                return (WasButtonPressed(button, PlayerIndex.One, out playerIndex) ||
                        WasButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
                        WasButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
                        WasButtonPressed(button, PlayerIndex.Four, out playerIndex));
            }
        }


        // Checks for a "pause the game" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player.
        public static bool WasGamePaused(PlayerIndex? controllingPlayer)
        {
            return WasKeyPressed(Keys.Escape, controllingPlayer, out PlayerIndex playerIndex) ||
                   WasButtonPressed(Buttons.Start, controllingPlayer, out playerIndex);
        }
    }
}