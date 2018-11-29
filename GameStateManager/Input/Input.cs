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
        UP = Buttons.DPadUp | Buttons.LeftThumbstickUp,
        DOWN = Buttons.DPadDown | Buttons.LeftThumbstickDown,
        LEFT = Buttons.DPadLeft | Buttons.LeftThumbstickLeft,
        RIGHT = Buttons.DPadRight | Buttons.LeftThumbstickRight,
        LK = Buttons.A,
        HK = Buttons.B,
        LP = Buttons.X,
        HP = Buttons.Y,
        LB = Buttons.LeftShoulder,
        RB = Buttons.RightShoulder,
        LT = Buttons.LeftTrigger,
        RT = Buttons.RightTrigger,
        START = Buttons.Start,

        DEBUG = Buttons.Back,
        CONSOLE = Keys.OemTilde
    }


    // Helper for reading input from keyboard, gamepad, and touchscreen. This class tracks both the current
    // and previous state of the input devices, and implements query methods for high level input actions.
    public static class Input
    {
        public const int MAX_USERS = 4;
        public static User[] Users;

        public static readonly MoveList MoveList = new MoveList();

        // Stores each players' most recent move and when they pressed it.
        public static Move[] PlayersMove;
        public static TimeSpan[] PlayersMoveTime;

        // Time until the currently "active" move dissapears from the screen.
        public static readonly TimeSpan MOVE_TIMEOUT = TimeSpan.FromSeconds(1.0);
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

        private static Dictionary<Action, Keys[]> keys;
        private static Dictionary<Action, MouseButton> mouseButtons;
#endif
#if DESKTOP || CONSOLE
        public static GamePadState[] LastGamePadState;
        public static GamePadState[] CurrentGamePadState;

        private static Dictionary<Action, Buttons[]> buttons;

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

            keys = new Dictionary<Action, Keys[]>
            {
                { Action.UP, new Keys[] { Keys.Up, Keys.W } },
                { Action.DOWN, new Keys[] { Keys.Down, Keys.S } },
                { Action.LEFT, new Keys[] { Keys.Left, Keys.A } },
                { Action.RIGHT, new Keys[] { Keys.Right, Keys.D } },
                { Action.LK, new Keys[] { Keys.Space } },
                { Action.HK, new Keys[] { Keys.Escape } },
                { Action.LB, new Keys[] { Keys.Q, Keys.PageUp } },
                { Action.RB, new Keys[] { Keys.E, Keys.PageDown } },
                { Action.START, new Keys[] { Keys.Escape } },
                { Action.CONSOLE, new Keys[] {Keys.OemTilde } },
                { Action.DEBUG, new Keys[] { Keys.F1 } },
            };

            mouseButtons = new Dictionary<Action, MouseButton>
            {
                { Action.LK, MouseButton.LEFT },
                { Action.HK, MouseButton.RIGHT }
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

            buttons = new Dictionary<Action, Buttons[]>
            {
                { Action.UP, new Buttons[] { Buttons.DPadUp, Buttons.LeftThumbstickUp } },
                { Action.DOWN, new Buttons[] { Buttons.DPadDown, Buttons.LeftThumbstickDown } },
                { Action.LEFT, new Buttons[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft } },
                { Action.RIGHT, new Buttons[] { Buttons.DPadRight, Buttons.LeftThumbstickRight } },
                { Action.LK, new Buttons[] { Buttons.A } },
                { Action.HK, new Buttons[] { Buttons.B } },
                { Action.LB, new Buttons[] { Buttons.LeftShoulder } },
                { Action.RB, new Buttons[] { Buttons.RightShoulder } },
                { Action.DEBUG, new Buttons[] { Buttons.Back } },
                { Action.START, new Buttons[] { Buttons.Start } }
            };

            Buffers = new List<Buttons>[MAX_USERS];
            for (int i = 0; i < MAX_USERS; i++)
                Buffers[i] = new List<Buttons>(BUFFER_SIZE);

            // Give each player a location to store their most recent move.
            PlayersMove = new Move[MAX_USERS];
            PlayersMoveTime = new TimeSpan[MAX_USERS];

            NonDirectionalButtons = new Dictionary<Buttons, Keys>
            {
                { Buttons.A, Keys.K },
                { Buttons.B, Keys.L },
                { Buttons.X, Keys.J },
                { Buttons.Y, Keys.I },
                { Buttons.Start, Keys.Enter },
                { Buttons.Back, Keys.Escape },
                { Buttons.LeftShoulder, Keys.Q },
                { Buttons.LeftTrigger, Keys.U },
                { Buttons.RightShoulder, Keys.E },
                { Buttons.RightTrigger, Keys.O },
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

                if (Users[i].IsActive)
                {
                    UpdateNonDirectionalButtons(i);

                    // Expire old moves.
                    if (ScreenManager.GameTime.TotalGameTime - PlayersMoveTime[i] > MOVE_TIMEOUT)
                        PlayersMove[i] = null;

                    // Detection and record of current player's most recent move.
                    Move newMove = MoveList.DetectMoves(i);

                    if (newMove != null)
                    {
                        PlayersMove[i] = newMove;
                        PlayersMoveTime[i] = ScreenManager.GameTime.TotalGameTime;
                    }
                }
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


        // Clears the input buffer of all players.
        public static void ClearInputBuffers()
        {
            for (int i = 0; i < MAX_USERS; i++)
                Buffers[i].Clear();
        }


        private static void UpdateNonDirectionalButtons(int userIndex)
        {
            // Expire old input.
            TimeSpan time = TimeSpan.FromMilliseconds(0.0);
            if (ScreenManager.GameTime != null)
                time = ScreenManager.GameTime.TotalGameTime;
            TimeSpan timeSinceLast = time - LastInputTime;

            if (timeSinceLast > BufferTimeout)
                Buffers[userIndex].Clear();

            // Get all of the non-directional buttons pressed.
            Buttons buttons = 0;

            switch (Users[userIndex].InputType)
            {
                case InputType.KEYBOARD:
                    {
                        foreach (KeyValuePair<Buttons, Keys> pair in NonDirectionalButtons)
                        {
                            Keys key = pair.Value;

                            // Check the keyboard for presses and OR them to accumulate buttons.
                            if (LastKeyboardState.IsKeyUp(key) && CurrentKeyboardState.IsKeyDown(key))
                                buttons |= pair.Key;
                        }
                    }
                    break;
                case InputType.GAMEPAD:
                    {
                        foreach (KeyValuePair<Buttons, Keys> pair in NonDirectionalButtons)
                        {
                            Buttons button = pair.Key;

                            // Check the keyboard for presses and OR them to accumulate buttons.
                            if (LastGamePadState[Users[userIndex].ControllerIndex].IsButtonUp(button) &&
                                CurrentGamePadState[Users[userIndex].ControllerIndex].IsButtonDown(button))
                                buttons |= button;
                        }
                    }
                    break;
            }

            // It is very hard to press two buttons on exactly the same frame.
            // If they are close enough, consider them pressed at the same time.
            bool mergeInput = Buffers[userIndex].Count > 0 && timeSinceLast < MergeInputTime;

            // If there is a new direction.
            Buttons currentDirection = 0;
            Buttons lastDirection = 0;
            switch (Users[userIndex].InputType)
            {
                case InputType.KEYBOARD:
                    currentDirection = GetDirectionFromInput(Users[userIndex], InputState.CURRENT);
                    lastDirection = GetDirectionFromInput(Users[userIndex], InputState.LAST);
                    break;
                case InputType.GAMEPAD:
                    currentDirection = GetDirectionFromInput(Users[userIndex], InputState.CURRENT);
                    lastDirection = GetDirectionFromInput(Users[userIndex], InputState.LAST);
                    break;
            }

            if (lastDirection != currentDirection)
            {
                // Combine the direction with the buttons.
                buttons |= currentDirection;

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
                    Buffers[userIndex][Buffers[userIndex].Count - 1] |= buttons;
                }
                else
                {
                    // Append this input to the buffer, expiring old input if necessary.
                    if (Buffers[userIndex].Count == Buffers[userIndex].Capacity)
                        Buffers[userIndex].RemoveAt(0);

                    Buffers[userIndex].Add(buttons);

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
                for (int i = 0; i < keys[action].Length; i++)
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
                    for (int i = 0; i < keys[action].Length; i++)
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
                    for (int i = 0; i < buttons[action].Length; i++)
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
                if (Buffers[i][Buffers[i].Count - j] != (Buttons)move.Sequence[move.Sequence.Length - j])
                    return false;
            }

            // Unless this move is a component of a larger sequence, consume it.
            if (!move.IsSubMove)
                Buffers[i].Clear();

            return true;
        }


        private enum InputState { CURRENT, LAST }

        private static Buttons GetDirectionFromInput(User user, InputState state)
        {
            Buttons direction = 0;

            switch (user.InputType)
            {
                case InputType.KEYBOARD:
                    {
                        KeyboardState keyboard;
                        if (state == InputState.CURRENT)
                            keyboard = CurrentKeyboardState;
                        else
                            keyboard = LastKeyboardState;

                        // Get the vertical direction.
                        if (keyboard.IsKeyDown(Keys.Up))
                            direction |= (Buttons)Action.UP;
                        else if (keyboard.IsKeyDown(Keys.Down))
                            direction |= (Buttons)Action.DOWN;

                        // Combine it with the horizontal direction.
                        if (keyboard.IsKeyDown(Keys.Left))
                            direction |= (Buttons)Action.LEFT;
                        else if (keyboard.IsKeyDown(Keys.Right))
                            direction |= (Buttons)Action.RIGHT;
                    }
                    break;
                case InputType.GAMEPAD:
                    {
                        GamePadState gamePad;
                        if (state == InputState.CURRENT)
                            gamePad = CurrentGamePadState[user.ControllerIndex];
                        else
                            gamePad = LastGamePadState[user.ControllerIndex];

                        // Get the vertical direction.
                        if (gamePad.IsButtonDown(Buttons.DPadUp) ||
                            gamePad.IsButtonDown(Buttons.LeftThumbstickUp))
                            direction |= (Buttons)Action.UP;
                        else if (gamePad.IsButtonDown(Buttons.DPadDown) ||
                            gamePad.IsButtonDown(Buttons.LeftThumbstickDown))
                            direction |= (Buttons)Action.DOWN;

                        // Combine it with the horizontal direction.
                        if (gamePad.IsButtonDown(Buttons.DPadLeft) ||
                            gamePad.IsButtonDown(Buttons.LeftThumbstickLeft))
                            direction |= (Buttons)Action.LEFT;
                        else if (gamePad.IsButtonDown(Buttons.DPadRight) ||
                            gamePad.IsButtonDown(Buttons.LeftThumbstickRight))
                            direction |= (Buttons)Action.RIGHT;
                    }
                    break;
            }
            

            return direction;
        }
        

        // Gets the direction without non-direction buttons from the Button enum
        // and extract the direction from a full set of buttons using the bitmask.
        public static Buttons GetDirectionFromButtons(Buttons buttons)
        {
            return buttons & (Buttons)(Action.UP | Action.DOWN | Action.LEFT | Action.RIGHT);
        }
    }
}