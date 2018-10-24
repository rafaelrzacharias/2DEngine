using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace GameStateManager
{
    public enum MouseButton
    {
        Left,
        Middle,
        Right
    }


    // Helper for reading input from keyboard, gamepad, and touch input. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions
    // such as "move up through the menu" or "pause the game".
    public static class Input
    {
        private static MouseState LastMouseState;
        private static KeyboardState LastKeyboardState;
        private static bool isLeftMouseDown = false;
        private static int dragThreshold = 3;
        private const int MAX_INPUTS = 4;
        private static GamePadState[] LastGamePadStates;
        private static bool isInitialized;

        // Key that pressed last frame.
        private static Keys pressedKey;

        // Timer for key repeating.
        private static float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private static float keyRepeatStartDuration = 0.3f;

        // Key repeat duration in seconds after the first key press.
        private static float keyRepeatDuration = 0.05f;

        // The controlling player (primary user)
        public static PlayerIndex ControllingPlayer;

        public static MouseState CurrentMouseState { get; private set; }
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static bool isDragging = false;
        public static bool isDragComplete = false;

        private static Vector2 currentMousePosition = Vector2.Zero;
        public static Vector2 CurrentMousePosition { get { return currentMousePosition; } }

        private static Vector2 prevMousePosition = Vector2.Zero;
        public static Vector2 PrevMousePosition { get { return prevMousePosition; } }

        private static Vector2 dragMouseStart = Vector2.Zero;
        public static Vector2 MouseDragStartPosition { get { return dragMouseStart; } }

        private static Vector2 dragMouseEnd = Vector2.Zero;
        public static Vector2 MouseDragEndPosition { get { return dragMouseEnd; } }

        public static Vector2 MouseDragDelta { get; private set; }
        public static float MouseDragDistance { get; private set; }
        public static GamePadState[] CurrentGamePadStates { get; private set; }
        public static bool[] IsGamePadConnected { get; private set; }
        public static TouchCollection TouchState { get; private set; }
        public static List<GestureSample> Gestures { get; private set; }


        public static void Initialize()
        {
            if (isInitialized == false)
            {
                CurrentMouseState = new MouseState();
                LastMouseState = new MouseState();
                CurrentKeyboardState = new KeyboardState();
                LastKeyboardState = new KeyboardState();
                CurrentGamePadStates = new GamePadState[MAX_INPUTS];
                LastGamePadStates = new GamePadState[MAX_INPUTS];
                IsGamePadConnected = new bool[MAX_INPUTS];
                Gestures = new List<GestureSample>();

                isInitialized = true;
            }
        }


        // Reads the latest state of the keyboard and gamepad.
        public static void Update()
        {
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            UpdateMouseStates();

            for (int i = 0; i < MAX_INPUTS; i++)
            {
                LastGamePadStates[i] = CurrentGamePadStates[i];
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                if (CurrentGamePadStates[i].IsConnected)
                    IsGamePadConnected[i] = true;
                else
                    IsGamePadConnected[i] = false;
            }

            // Check of any button was pressed in a controller that is not the current controller scheme
            //for (int i = 0; i < MAX_INPUTS; i++)
            //{
            //    if (CurrentGamePadStates[i].IsConnected)
            //    {
            //        ControllingPlayer = (PlayerIndex)i;
            //        break;
            //    }
            //}

            TouchState = TouchPanel.GetState();
            Gestures.Clear();

            while (TouchPanel.IsGestureAvailable)
                Gestures.Add(TouchPanel.ReadGesture());
        }


        // I NEED TO MAKE THIS METHOD USE THE PLAYERINDEX JUST LIKE ALL OTHER METHODS.
        private static void UpdateMouseStates()
        {
            currentMousePosition.X = CurrentMouseState.X;
            currentMousePosition.Y = CurrentMouseState.Y;
            prevMousePosition.X = LastMouseState.X;
            prevMousePosition.Y = LastMouseState.Y;

            // If we were dragging and the left mouse button was released
            if (CurrentMouseState.LeftButton == ButtonState.Released && isDragging)
            {
                isLeftMouseDown = false;
                isDragging = false;
                isDragComplete = true;
                dragMouseEnd = currentMousePosition;

                MouseDragDistance = Vector2.Distance(dragMouseStart, dragMouseEnd);
                MouseDragDelta = dragMouseEnd - dragMouseStart;
            }

            // Let's set the left mouse down and the mouse origin
            if (isLeftMouseDown == false && CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    CurrentMouseState.Equals(LastMouseState) == false)
            {
                isLeftMouseDown = true;
                isDragComplete = false;
                dragMouseStart = currentMousePosition;
            }

            if (isLeftMouseDown && CurrentMouseState.LeftButton == ButtonState.Released &&
                    CurrentMouseState.Equals(LastMouseState) == false)
                isLeftMouseDown = false;

            // If dragging distance was over the current threshold (5 pixels),
            // set the dragging to true.
            if (isLeftMouseDown && isDragging == false)
            {
                Vector2 delta = dragMouseStart - currentMousePosition;

                if (delta.Length() > dragThreshold)
                {
                    isDragging = true;
                    dragMouseStart = currentMousePosition;
                }
            }
        }


        // Checks if any key was pressed on each connected gamepad, keyboard and mouse.
        public static bool WasAnyButtonPressed()
        {
            if (WasKeyPressed(Keys.OemTilde, ControllingPlayer, out PlayerIndex playerIndex))
                return false;

            for (int i = 0; i < MAX_INPUTS; i++)
            {
                if (CurrentGamePadStates[i].IsConnected && LastGamePadStates[i].IsConnected &&
                    CurrentGamePadStates[i] != LastGamePadStates[i])
                {
                    if (CurrentGamePadStates[i].ThumbSticks.Left.Length() != LastGamePadStates[i].ThumbSticks.Left.Length() ||
                        CurrentGamePadStates[i].ThumbSticks.Right.Length() != LastGamePadStates[i].ThumbSticks.Right.Length())
                        continue;

                    ControllingPlayer = (PlayerIndex)i;
                    return true;
                }
            }

            if (CurrentKeyboardState.GetPressedKeys().Length != 0)
            {
                ControllingPlayer = PlayerIndex.One;
                return true;
            }

            if (WasMouseClicked(MouseButton.Left, null, out playerIndex) || 
                WasMouseClicked(MouseButton.Middle, null, out playerIndex) ||
                WasMouseClicked(MouseButton.Right, null, out playerIndex))
            {
                ControllingPlayer = PlayerIndex.One;
                return true;
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
            return CurrentKeyboardState.IsKeyDown(key);
        }


        // Checks whether or not the scroll wheel changed.
        public static bool HasScrollWheelChanged()
        {
            return CurrentMouseState.ScrollWheelValue != LastMouseState.ScrollWheelValue;
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


        // Checks for a "menu select" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player. When the action
        // is detected, the output playerIndex reports which player pressed it.
        public static bool WasMenuSelected(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return WasMouseClicked(MouseButton.Left, controllingPlayer, out playerIndex) ||
                   WasKeyPressed(Keys.Space, controllingPlayer, out playerIndex) ||
                   WasButtonPressed(Buttons.A, controllingPlayer, out playerIndex);
        }


        // Checks for a "menu cancel" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player. When the action
        // is detected, the output playerIndex reports which player pressed it.
        public static bool WasMenuCancelled(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return WasMouseClicked(MouseButton.Right, controllingPlayer, out playerIndex) ||
                   WasKeyPressed(Keys.Escape, controllingPlayer, out playerIndex) ||
                   WasButtonPressed(Buttons.B, controllingPlayer, out playerIndex);
        }


        // Checks for a "menu up" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player.
        public static bool WasMenuUp(PlayerIndex? controllingPlayer)
        {
            return WasKeyPressed(Keys.Up, controllingPlayer, out PlayerIndex playerIndex) ||
                   WasButtonPressed(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   WasButtonPressed(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        // Checks for a "menu down" input action. The controllingPlayer parameter specifies which player
        // to read input for. If this is null, it will accept input from any player.
        public static bool WasMenuDown(PlayerIndex? controllingPlayer)
        {
            return WasKeyPressed(Keys.Down, controllingPlayer, out PlayerIndex playerIndex) ||
                   WasButtonPressed(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   WasButtonPressed(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
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