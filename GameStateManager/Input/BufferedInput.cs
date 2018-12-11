using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace GameStateManager
{
    public static class BufferedInput
    {
        // The moves available to be performed by the players.
        public static readonly MoveList MoveList = new MoveList();

        // Stores each players' most recent move and when they pressed it.
        public static Move[] PlayersMove;
        public static TimeSpan[] PlayersMoveTime;

        // Time until the currently "active" move dissapears from the screen.
        public static readonly TimeSpan MOVE_TIMEOUT = TimeSpan.FromSeconds(1.0);

        // The last "real time" that new input was received. Slightly late button
        // presses will not update this time; they are merged with previous input.
        public static TimeSpan LastInputTime { get; private set; }

        // The current sequence of pressed buttons.
        private const int BUFFER_SIZE = 10;
        public static List<Action>[] Buffers;

        // This is how long to wait for input before all input data is expired.
        // This prevents the player from performing half of a move, waiting, then
        // performing the rest of the move after they forgot about the first half.
        public static readonly TimeSpan BufferTimeout = TimeSpan.FromMilliseconds(500.0);

        // The size of the "merge window" for combining button presses that occur at almost
        // the same time. If too small, players will find it difficult to perform moves which
        // require pressing several buttons simultaneously. If too large, players will find it
        // difficult to perform moves which require pressing several buttons in sequence.
        public static readonly TimeSpan MergeInputTime = TimeSpan.FromMilliseconds(100.0);


        // Initialized the BufferedInput system.
        public static void Initialize()
        {
            Buffers = new List<Action>[Input.MAX_USERS];

            for (int i = 0; i < Buffers.Length; i++)
                Buffers[i] = new List<Action>(BUFFER_SIZE);

            // Give each player a location to store their most recent move.
            PlayersMove = new Move[Buffers.Length];
            PlayersMoveTime = new TimeSpan[Buffers.Length];

            Input.ControllerConnected += BufferedInput_OnControllerStatusChanged;
            Input.ControllerDisconnected += BufferedInput_OnControllerStatusChanged;
        }


        // Callback to clear the buffered inputs upon controller connection/disconnection.
        private static void BufferedInput_OnControllerStatusChanged(int controllerIndex)
        {
            ClearInputBuffers();
        }


        // Updates the BuffereInput for a given user.
        public static void Update(int userIndex)
        {
            UpdatePlayerActions(userIndex);

            // Expire old moves.
            if (ScreenManager.GameTime.TotalGameTime - PlayersMoveTime[userIndex] > MOVE_TIMEOUT)
                PlayersMove[userIndex] = null;

            // Detection and record of current player's most recent move.
            Move newMove = MoveList.DetectMoves(userIndex);

            if (newMove != null)
            {
                PlayersMove[userIndex] = newMove;
                PlayersMoveTime[userIndex] = ScreenManager.GameTime.TotalGameTime;
            }
        }


        private static void UpdatePlayerActions(int userIndex)
        {
            // Expire old input.
            TimeSpan time = TimeSpan.FromMilliseconds(0.0);
            if (ScreenManager.GameTime != null)
                time = ScreenManager.GameTime.TotalGameTime;
            TimeSpan timeSinceLast = time - LastInputTime;

            if (timeSinceLast > BufferTimeout)
                Buffers[userIndex].Clear();

            // Get all of the non-directional buttons pressed.
            Action actions = Action.NONE;

            for (int i = 1; i < Input.Actions.Length; i++)
            {
                Action action = Input.Actions[i];

                // If the action is a direction, skip it.
                if ((action & ~(Action.UP | Action.DOWN | Action.LEFT | Action.RIGHT)) == Action.NONE)
                    continue;

                if (Input.IsActionPressed(action, Input.Users[userIndex]))
                    actions |= action;
            }

            // It is very hard to press two buttons on exactly the same frame.
            // If they are close enough, consider them pressed at the same time.
            bool mergeInput = Buffers[userIndex].Count > 0 && timeSinceLast < MergeInputTime;

            // If there is a new direction.
            Action currentAction = GetActionFromInput(Input.Users[userIndex], false);
            Action lastAction = GetActionFromInput(Input.Users[userIndex], true);

            if (lastAction != currentAction)
            {
                // Combine the direction with the buttons.
                actions |= currentAction;

                // Don't merge two opposite directions. This has the side effect that the direction needs
                // to be pressed at the same time or slightly before the buttons for merging to work.
                mergeInput = false;
            }

            // If there was any new input on this update, add it to the buffer.
            if (actions != Action.NONE)
            {
                if (mergeInput)
                {
                    // Use the bitwise OR to merge with the previous input.
                    // LastInputTime isn't updated to prevent extending the merge window.
                    Buffers[userIndex][Buffers[userIndex].Count - 1] |= actions;
                }
                else
                {
                    // Append this input to the buffer, expiring old input if necessary.
                    if (Buffers[userIndex].Count == Buffers[userIndex].Capacity)
                        Buffers[userIndex].RemoveAt(0);

                    Buffers[userIndex].Add(actions);

                    // Record the time of this input to begin the merge window.
                    LastInputTime = time;
                }
            }
        }


        // Clears the input buffer of all players.
        public static void ClearInputBuffers()
        {
            for (int i = 0; i < Buffers.Length; i++)
                Buffers[i].Clear();
        }


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
            if (move.IsSubMove == false)
                Buffers[i].Clear();

            return true;
        }


        // NOTE: This input system assumes that UP and LEFT has priority over other directions.
        private static Action GetActionFromInput(User user, bool sampleLastFrame)
        {
            Action action = Action.NONE;
            ActionMap upMap = user.ActionMaps[Input.GetActionIndex(Action.UP)];
            ActionMap downMap = user.ActionMaps[Input.GetActionIndex(Action.DOWN)];
            ActionMap leftMap = user.ActionMaps[Input.GetActionIndex(Action.LEFT)];
            ActionMap rightMap = user.ActionMaps[Input.GetActionIndex(Action.RIGHT)];

            switch (user.InputType)
            {
                case InputType.KEYBOARD:
                    {
                        KeyboardState keyboardState = Input.CurrentKeyboardState;

                        if (sampleLastFrame)
                            keyboardState = Input.LastKeyboardState;

                        List<Keys> upKeys = upMap.Keys;
                        List<Keys> downKeys = downMap.Keys;
                        List<Keys> leftKeys = leftMap.Keys;
                        List<Keys> rightKeys = rightMap.Keys;

                        for (int i = 0; i < upKeys.Count; i++)
                        {
                            if (keyboardState.IsKeyDown(upKeys[i]))
                            {
                                action |= Action.UP;
                                break;
                            }
                        }

                        // If the action already contains UP, don't add DOWN.
                        if ((action & Action.UP) == Action.NONE)
                        {
                            for (int i = 0; i < downKeys.Count; i++)
                            {
                                if (keyboardState.IsKeyDown(downKeys[i]))
                                {
                                    action |= Action.DOWN;
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < leftKeys.Count; i++)
                        {
                            if (keyboardState.IsKeyDown(leftKeys[i]))
                            {
                                action |= Action.LEFT;
                                break;
                            }
                        }

                        // If the action already contains UP, UP_LEFT or DOWN_LEFT, don't add RIGHT.
                        if ((action & (Action.UP | Action.LEFT)) == Action.NONE ||
                            (action & (Action.DOWN | Action.LEFT)) == Action.NONE)
                        {
                            for (int i = 0; i < rightKeys.Count; i++)
                            {
                                if (keyboardState.IsKeyDown(rightKeys[i]))
                                {
                                    action |= Action.RIGHT;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case InputType.GAMEPAD:
                    {
                        GamePadState gamePadState = Input.CurrentGamePadState[user.ControllerIndex];

                        if (sampleLastFrame)
                            gamePadState = Input.LastGamePadState[user.ControllerIndex];

                        List<Buttons> upButtons = upMap.Buttons;
                        List<Buttons> downButtons = downMap.Buttons;
                        List<Buttons> leftButtons = leftMap.Buttons;
                        List<Buttons> rightButtons = rightMap.Buttons;

                        for (int i = 0; i < upButtons.Count; i++)
                        {
                            if (gamePadState.IsButtonDown(upButtons[i]))
                            {
                                action |= Action.UP;
                                break;
                            }
                        }

                        // If the action already contains UP, don't add DOWN.
                        if ((action & Action.UP) == Action.NONE)
                        {
                            for (int i = 0; i < downButtons.Count; i++)
                            {
                                if (gamePadState.IsButtonDown(downButtons[i]))
                                {
                                    action |= Action.DOWN;
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < leftButtons.Count; i++)
                        {
                            if (gamePadState.IsButtonDown(leftButtons[i]))
                            {
                                action |= Action.LEFT;
                                break;
                            }
                        }

                        // If the action already contains UP, UP_LEFT or DOWN_LEFT, don't add RIGHT.
                        if ((action & (Action.UP | Action.LEFT)) == Action.NONE ||
                            (action & (Action.DOWN | Action.LEFT)) == Action.NONE)
                        {
                            for (int i = 0; i < rightButtons.Count; i++)
                            {
                                if (gamePadState.IsButtonDown(rightButtons[i]))
                                {
                                    action |= Action.RIGHT;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            return action;
        }
    }
}