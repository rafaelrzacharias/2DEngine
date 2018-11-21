using Microsoft.Xna.Framework;
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
        CONSOLE,
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

        // The last "real time" that new input was received. Slightly late button
        // presses will not update this time; they are merged with previous input.
        public static TimeSpan LastInputTime { get; private set; }

        // The current sequence of pressed buttons.
        private const int BUFFER_SIZE = 10;
        public static List<Buttons>[] Buffers;

        // This is how long to wait for input before all input data is expired.
        // This prevents the player from performing half of a move, waiting, then
        // performing the rest of the move after they forgot about the first half.
        public static readonly TimeSpan BufferTimeout = TimeSpan.FromMilliseconds(500.0);

        // The size of the "merge window" for combining button presses that occur at almost
        // the same time. If too small, players will find it difficult to perform moves which
        // require pressing several buttons simultaneously. If too large, players will find it
        // difficult to perform moves which require pressing several buttons in sequence.
        public static readonly TimeSpan MergeInputTime = TimeSpan.FromMilliseconds(100.0);

        // Provides the map of non-directional gamePad buttons to keyboard keys.
        public static Dictionary<Buttons, Keys> NonDirectionalButtons { get; private set; }
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
                { Action.DEBUG, new List<Keys> { Keys.F1 } },
                { Action.CONSOLE, new List<Keys> {Keys.OemTilde } },
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

            Buffers = new List<Buttons>[MAX_USERS];
            for (int i = 0; i < MAX_USERS; i++)
                Buffers[i] = new List<Buttons>(BUFFER_SIZE);

            NonDirectionalButtons = new Dictionary<Buttons, Keys>
            {
                { Buttons.A, Keys.K },
                { Buttons.B, Keys.L },
                { Buttons.X, Keys.J },
                { Buttons.Y, Keys.I },
                { Buttons.Start, Keys.Enter },
                { Buttons.Back, Keys.Escape },
                { Buttons.LeftShoulder, Keys.Q },
                { Buttons.LeftTrigger, Keys.D1 },
                { Buttons.RightShoulder, Keys.O },
                { Buttons.RightTrigger, Keys.D0 },
                { Buttons.LeftStick, Keys.Z },
                { Buttons.RightStick, Keys.M }
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


        // Returns the first available user slot.
        public static int GetFirstAvailableSlot()
        {
#if DESKTOP || CONSOLE
            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].IsActive == false
#if DESKTOP
                    || Users[i].InputType == InputType.KEYBOARD)
#endif
                    return i;
            }

            return -1;
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
            CurrentMouseState = Mouse.GetState();

            for (int i = 0; i < MAX_USERS; i++)
            {
                if (Users[i].IsActive)
                    UpdateNonDirectionalButtons(i);
            }

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
#if !DESKTOP
            int index = -1;
            if (GetUserCount() == 0)
                index = WasAnyButtonPressed(true, false);

            if (index != -1)
            {
                for (int i = 0; i < MAX_USERS; i++)
                {
                    if (Users[i].ControllerIndex == index)
                        continue;

                    switch (Users[i].InputType)
                    {
                        case InputType.KEYBOARD:
                            {
                                if (index != MAX_USERS)
                                {
                                    Users[i].ControllerIndex = index;
                                    Users[i].InputType = InputType.GAMEPAD;

                                    OnUserControllerTypeChanged(i);
                                }
                            }
                            break;
                        case InputType.GAMEPAD:
                            {
                                if (index == MAX_USERS)
                                {
                                    Users[i].ControllerIndex = index;
                                    Users[i].InputType = InputType.KEYBOARD;

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


        private static void UpdateNonDirectionalButtons(int controllerIndex)
        {
            // Expire old input.
            TimeSpan time = TimeSpan.FromMilliseconds(0.0);
            if (ScreenManager.GameTime != null)
                time = ScreenManager.GameTime.TotalGameTime;
            TimeSpan timeSinceLast = time - LastInputTime;

            if (timeSinceLast > BufferTimeout)
                Buffers[controllerIndex].Clear();

            // Get all of the non-directional buttons pressed.
            Buttons buttons = 0;
            foreach (KeyValuePair<Buttons, Keys> pair in NonDirectionalButtons)
            {
                Buttons button = pair.Key;
                Keys key = pair.Value;

                // Check the gamePad and keyboard for presses.
                if ((LastGamePadState[controllerIndex].IsButtonUp(button) && CurrentGamePadState[controllerIndex].IsButtonDown(button)) ||
                    (LastKeyboardState.IsKeyUp(key) && CurrentKeyboardState.IsKeyDown(key)))
                {
                    // Use the bitwise OR to accumulate button presses.
                    buttons |= button;
                }
            }

            // It is very hard to press two buttons on exactly the same frame.
            // If they are close enough, consider them pressed at the same time.
            bool mergeInput = Buffers[controllerIndex].Count > 0 && timeSinceLast < MergeInputTime;

            // If there is a new direction.s
            Buttons direction = Direction.FromInput(CurrentGamePadState[controllerIndex], CurrentKeyboardState);

            if (Direction.FromInput(LastGamePadState[controllerIndex], LastKeyboardState) != direction)
            {
                // Combine the direction with the buttons.
                buttons |= direction;

                // Don't merge two opposite directions. This has the side effect that the direction needs
                // to be pressed at the same time or slightly before the buttons for merging to work.
                mergeInput = false;
            }

            // If there was any new input on this update, add it to the buffer.
            if (buttons != 0)
            {
                if (mergeInput)
                {
                    // Use the bitwise OR to merge with the previous input.
                    // LastInputTime isn't updated to prevent extending the merge window.
                    Buffers[controllerIndex][Buffers[controllerIndex].Count - 1] |= buttons;
                }
                else
                {
                    // Append this input to the buffer, expiring old input if necessary.
                    if (Buffers[controllerIndex].Count == Buffers[controllerIndex].Capacity)
                        Buffers[controllerIndex].RemoveAt(0);

                    Buffers[controllerIndex].Add(buttons);

                    // Record the time of this input to begin the merge window.
                    LastInputTime = time;
                }
            }
        }


#if DESKTOP || CONSOLE
        // Event raised when a controller is disconnected.
        public delegate void ControllerDisconnectedEventHandler(int controllerIndex);
        public static event ControllerDisconnectedEventHandler ControllerDisconnected;

        public static void OnControllerDisconnected(int controllerIndex)
        {
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
                if (CurrentGamePadState[i] != LastGamePadState[i] && 
                    CurrentGamePadState[i].IsConnected == LastGamePadState[i].IsConnected)
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


        // Checks if a key was pressed during this update. The controllingPlayer parameter specifies
        // which player to read input for. If this is null, it will accept input from any player.
        // When a keypress is detected, the output playerIndex reports which player pressed it.
        public static bool WasButtonPressed(Action action, User user = null)
        {
#if DESKTOP
            if (action == Action.CONSOLE)
            {
                for (int i = 0; i < keys[action].Count; i++)
                {
                    if (LastKeyboardState.IsKeyUp(keys[action][i]) &&
                            CurrentKeyboardState.IsKeyDown(keys[action][i]))
                        return true;
                }
            }

            if (user == null || user.InputType == InputType.KEYBOARD)
            {
                if (keys.ContainsKey(action))
                {
                    for (int i = 0; i < keys[action].Count; i++)
                    {
                        if (LastKeyboardState.IsKeyUp(keys[action][i]) &&
                            CurrentKeyboardState.IsKeyDown(keys[action][i]))
                            return true;
                    }
                }

                if (mouseButtons.ContainsKey(action))
                {
                    switch (mouseButtons[action])
                    {
                        case MouseButton.LEFT:
                            {
                                if (LastMouseState.LeftButton == ButtonState.Released &&
                                    CurrentMouseState.LeftButton == ButtonState.Pressed)
                                    return true;
                            }
                            break;
                        case MouseButton.RIGHT:
                            {
                                if (LastMouseState.RightButton == ButtonState.Released &&
                                    CurrentMouseState.RightButton == ButtonState.Pressed)
                                    return true;
                            }
                            break;
                        case MouseButton.MIDDLE:
                            {
                                if (LastMouseState.MiddleButton == ButtonState.Released &&
                                    CurrentMouseState.MiddleButton == ButtonState.Pressed)
                                    return true;
                            }
                            break;
                    }
                }
            }

#endif
#if DESKTOP || CONSOLE
            if (user == null || user.InputType == InputType.GAMEPAD)
            {
                int index = -1;
                if (user != null)
                    index = user.ControllerIndex;

                if (buttons.ContainsKey(action))
                {
                    for (int i = 0; i < buttons[action].Count; i++)
                    {
                        if (index != -1)
                        {
                            if (LastGamePadState[index].IsButtonUp(buttons[action][i]) &&
                                CurrentGamePadState[index].IsButtonDown(buttons[action][i]))
                                return true;
                        }
                        else
                        {
                            for (int j = 0; j < MAX_USERS; j++)
                            {
                                if (LastGamePadState[j].IsButtonUp(buttons[action][i]) &&
                                    CurrentGamePadState[j].IsButtonDown(buttons[action][i]))
                                    return true;
                            }
                        }
                    }
                }
            }
#endif
#if MOBILE

#endif
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
#endif

        // Determines if a move matches the current input history of a user.
        // Unless the move is a sub-move, the history is "consumed" to prevent it from matching twice.
        public static bool Matches(Move move, int i)
        {
            // If the move is longer than the buffer, if can't possibly match.
            if (move.Sequence.Length > Buffers[i].Count)
                return false;

            // Loop backwards to match against the most recent input.
            for (int j = 1; j <= move.Sequence.Length; ++j)
            {

                if (Buffers[i][Buffers[i].Count - j] != move.Sequence[move.Sequence.Length - j])
                    return false;
            }

            // Unless this move is a component of a larger sequence, consume it.
            if (!move.IsSubMove)
                Buffers[i].Clear();

            return true;
        }
    }
}