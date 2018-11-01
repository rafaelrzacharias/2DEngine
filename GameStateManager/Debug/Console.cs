using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameStateManager
{
    // Command window states.
    public enum State
    {
        CLOSED,
        OPENING,
        OPENED,
        CLOSING
    }


    // Debug command UI that runs in the Game. You can type commands using the keyboard.
    // 1) Add this component to the game.
    // 2) Register command by RegisterCommand method.
    // 3) Open/Close Debug window with '~' key.
    public class Console : DebugScreen, IConsoleHost
    {

        // CommandInfo class that contains information to run the command.
        private class CommandInfo
        {
            public CommandInfo(string command, string description, ConsoleExecute callback)
            {
                this.command = command;
                this.description = description;
                this.callback = callback;
            }

            // command name
            public string command;

            // Description of command.
            public string description;

            // delegate for execute the command.
            public ConsoleExecute callback;
        }

        // Maximum lines that shows in Debug command window.
        private const int MAX_LINE_COUNT = 16;

        // Maximum command history number.
        private const int MAX_COMMAND_HISTORY = 32;

        // Cursor character.
        private const string CURSOR = "_";

        // Current state
        public static State State { get; private set; }

        // Timer for state transition.
        private float stateTransition;

        // Registered echo listeners.
        private List<IConsoleEchoListener> listeners = new List<IConsoleEchoListener>();

        // Registered command executioner.
        private Stack<IConsoleExecutioner> executioners = new Stack<IConsoleExecutioner>();

        // Registered commands
        private Dictionary<string, CommandInfo> commandTable = new Dictionary<string, CommandInfo>();

        // Current command line string and cursor position.
        private string commandLine = string.Empty;
        private int cursorIndex = 0;

        private Queue<string> lines = new Queue<string>();

        // Command history buffer.
        private List<string> commandHistory = new List<string>();

        // Selecting command history index.
        private int commandHistoryIndex;


        // Initializes the Console.
        public override void Initialize()
        {
            base.Initialize();
            Area.Height = (MAX_LINE_COUNT + 1) * Font.LineSpacing;

            ScreenManager.Game.Services.AddService(typeof(IConsoleHost), this);

            // Display the registered commands.
            RegisterCommand("help", "Show all console commands",
            delegate (IConsoleHost host, string command, List<string> args)
            {
                int maxLength = 0;
                foreach (CommandInfo cmd in commandTable.Values)
                    maxLength = Math.Max(maxLength, cmd.command.Length);

                string format = string.Format("{{0,-{0}}}    {{1}}", maxLength);

                foreach (CommandInfo cmd in commandTable.Values)
                {
                    if (cmd.command != "help")
                        Echo(string.Format(format, cmd.command, cmd.description));
                }
            });

            // Clear screen commands.
            RegisterCommand("clear", "Clears the console",
            delegate (IConsoleHost host, string command, List<string> args)
            {
                lines.Clear();
            });

            // Quit command.
            RegisterCommand("quit", "Quits the game",
            delegate (IConsoleHost host, string command, List<string> args)
            {
                ScreenManager.Game.Exit();
            });

            IsActive = true;
        }


        public void RegisterCommand(string command, string description, ConsoleExecute callback)
        {
            string lowerCommand = command.ToLower();

            if (commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    string.Format("Command \"{0}\" is already registered.", command));
            }

            commandTable.Add(lowerCommand, new CommandInfo(command, description, callback));
        }


        public void UnregisterCommand(string command)
        {
            string lowerCommand = command.ToLower();

            if (!commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    string.Format("Command \"{0}\" is not registered.", command));
            }

            commandTable.Remove(command);
        }


        public void ExecuteCommand(string command)
        {
            // Call registered executioner.
            if (executioners.Count != 0)
            {
                executioners.Peek().ExecuteCommand(command);
                return;
            }

            // Run the command.
            char[] spaceChars = new char[] { ' ' };

            Echo(">" + command);

            command = command.TrimStart(spaceChars);

            List<string> args = new List<string>(command.Split(spaceChars));
            string cmdText = args[0];
            args.RemoveAt(0);

            CommandInfo cmd;
            if (commandTable.TryGetValue(cmdText.ToLower(), out cmd))
            {
                try
                {
                    // Call registered command delegate.
                    cmd.callback(this, command, args);
                }
                catch (Exception e)
                {
                    EchoError("Unhandled Exception occurred");

                    string[] lines = e.Message.Split(new char[] { '\n' });

                    for (int i = 0; i < lines.Length; i++)
                        EchoError(lines[i]);
                }
            }
            else
                EchoWarning("Unknown Command");

            // Add to command history.
            commandHistory.Add(command);

            while (commandHistory.Count > MAX_COMMAND_HISTORY)
                commandHistory.RemoveAt(0);

            commandHistoryIndex = commandHistory.Count;
        }


        public void RegisterEchoListener(IConsoleEchoListener listener)
        {
            listeners.Add(listener);
        }


        public void UnregisterEchoListener(IConsoleEchoListener listener)
        {
            listeners.Remove(listener);
        }


        public void Echo(ConsoleMessage messageType, string text)
        {
            lines.Enqueue(text);

            while (lines.Count >= MAX_LINE_COUNT)
                lines.Dequeue();

            // Call registered listeners.
            for(int i = 0; i < listeners.Count; i++)
                listeners[i].Echo(messageType, text);
        }


        public void Echo(string text)
        {
            Echo(ConsoleMessage.STANDARD, text);
        }


        public void EchoWarning(string text)
        {
            Echo(ConsoleMessage.WARNING, text);
        }


        public void EchoError(string text)
        {
            Echo(ConsoleMessage.ERROR, text);
        }


        public void PushExecutioner(IConsoleExecutioner executioner)
        {
            executioners.Push(executioner);
        }


        // Show the debug command window.
        public void Show()
        {
            if (State == State.CLOSED)
            {
                stateTransition = 0.0f;
                State = State.OPENING;
            }
        }


        // Hide the debug command window.
        public void Hide()
        {
            if (State == State.OPENED)
            {
                stateTransition = 1.0f;
                State = State.CLOSING;
            }
        }


        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            const float OPEN_SPEED = 8.0f;
            const float CLOSE_SPEED = 8.0f;

            switch (State)
            {
                case State.CLOSED:
                    if (Input.WasButtonPressed(Action.DEBUG, out User user))
                        Show();
                    break;
                case State.OPENING:
                    stateTransition += dt * OPEN_SPEED;
                    if (stateTransition > 1.0f)
                    {
                        stateTransition = 1.0f;
                        State = State.OPENED;
                    }
                    break;
                case State.OPENED:
                    if (Input.WasButtonPressed(Action.DEBUG, out user))
                        State = State.CLOSING;
                    else
                        ProcessKeyInputs(dt);
                    break;
                case State.CLOSING:
                    stateTransition -= dt * CLOSE_SPEED;
                    if (stateTransition < 0.0f)
                    {
                        stateTransition = 0.0f;
                        State = State.CLOSED;
                    }
                    break;
            }
        }


        // Handle keyboard input.
        public void ProcessKeyInputs(float dt)
        {
            Keys[] keys = Input.GetPressedKeys();

            if (keys == null || keys.Length == 0)
                return;

            bool shift = Input.IsKeyPressed(Keys.LeftShift) || Input.IsKeyPressed(Keys.RightShift);

            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.IsKeyPressed(keys[i], dt) == false)
                    continue;

                char ch;
                if (KeyboardUtils.KeyToString(keys[i], shift, out ch))
                {
                    // Handle typical character input.
                    commandLine = commandLine.Insert(cursorIndex, new string(ch, 1));
                    cursorIndex++;
                }
                else
                {
                    switch (keys[i])
                    {
                        case Keys.Back:
                            if (cursorIndex > 0)
                                commandLine = commandLine.Remove(--cursorIndex, 1);
                            break;
                        case Keys.Delete:
                            if (cursorIndex < commandLine.Length)
                                commandLine = commandLine.Remove(cursorIndex, 1);
                            break;
                        case Keys.Left:
                            if (cursorIndex > 0)
                                cursorIndex--;
                            break;
                        case Keys.Right:
                            if (cursorIndex < commandLine.Length)
                                cursorIndex++;
                            break;
                        case Keys.Enter:
                            // Run the command.
                            ExecuteCommand(commandLine);
                            commandLine = string.Empty;
                            cursorIndex = 0;
                            break;
                        case Keys.Up:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Max(0, commandHistoryIndex - 1);
                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Down:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Min(commandHistory.Count - 1, commandHistoryIndex + 1);
                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                    }
                }
            }
        }


        public override void Draw(GameTime gameTime)
        {
            // Do nothing when command window is completely closed.
            if (State == State.CLOSED)
                return;

            Matrix matrix = Matrix.CreateTranslation(new Vector3(
                0f, -Area.Height * (1.0f - stateTransition), 0f));

            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, matrix);

            SpriteBatch.Draw(Texture, Area, AreaColor);

            TextPosition.X = 0f;
            TextPosition.Y = 0f;
            SpriteBatch.DrawString(Font, "========== Console (type 'help' for available options) ==========", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            // Draw each lines.
            foreach (string line in lines)
            {
                SpriteBatch.DrawString(Font, line, TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }

            // Draw prompt string.
            string leftPart = ">" + commandLine.Substring(0, cursorIndex);
            Vector2 cursorPos = TextPosition + Font.MeasureString(leftPart);
            cursorPos.Y = TextPosition.Y;

            SpriteBatch.DrawString(Font, ">", TextPosition, Color.Yellow);
            SpriteBatch.DrawString(Font, " " + commandLine, TextPosition, Color.White);
            SpriteBatch.DrawString(Font, CURSOR, cursorPos, Color.White);

            SpriteBatch.End();
        }
    }
}