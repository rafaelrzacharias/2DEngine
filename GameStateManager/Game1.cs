using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameStateManager
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;

        // This is the master list of moves in logical order. This array is kept
        // around in order to draw the move list on the screen in this order.
        Move[] moves;

        // The move list used for move detection at runtime.
        MoveList moveList;
        public static int LongestMoveLenght;

        // Stores each players' most recent move and when they pressed it.
        Move[] playerMoves;
        TimeSpan[] playerMoveTimes;

        // Time until the currently "active" move dissapears from the screen.
        readonly TimeSpan MoveTimeout = TimeSpan.FromSeconds(1.0);

        // GamePad button textures.
        Texture2D upTexture;
        Texture2D downTexture;
        Texture2D leftTexture;
        Texture2D rightTexture;
        Texture2D upLeftTexture;
        Texture2D upRightTexture;
        Texture2D downLeftTexture;
        Texture2D downRightTexture;
        Texture2D aButtonTexture;
        Texture2D bButtonTexture;
        Texture2D xButtonTexture;
        Texture2D yButtonTexture;
        Texture2D plusTexture;
        Texture2D padFaceTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false,
                PreferredBackBufferWidth = 1000,
                PreferredBackBufferHeight = 700
            };

            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            IsMouseVisible = true;

            // Construct the master list of moves.
            moves = new Move[]
            {
                new Move("Jump", Buttons.A) { IsSubMove = true },
                new Move("Punch", Buttons.X) { IsSubMove = true },
                new Move("Double Jump", Buttons.A, Buttons.A),
                new Move("Jump Kick", Buttons.A | Buttons.X),
                new Move("Quad Punch", Buttons.X, Buttons.Y, Buttons.X, Buttons.Y),
                new Move("Fireball", Direction.Down, Direction.DownRight, Direction.Right | Buttons.X),
                new Move("Long Jump", Direction.Up, Direction.Up, Buttons.A),
                new Move("Back Flip", Direction.Down, Direction.Down | Buttons.A),
                new Move("30 Lives", Direction.Up, Direction.Up, Direction.Down, Direction.Down, Direction.Left, Direction.Right, Direction.Left, Direction.Right, Buttons.B, Buttons.A),
            };

            // Construct a move list which will store its own copy of the moves.
            moveList = new MoveList(moves);

            // Give each player a location to store their most recent move.
            playerMoves = new Move[Input.MAX_USERS];
            playerMoveTimes = new TimeSpan[Input.MAX_USERS];
        }


        protected override void Initialize()
        {
            Resources.Initialize(Content);
            Input.Initialize();
            Audio.Initialize();
            ScreenManager.Initialize(this);
            Debug.Initialize();

            base.Initialize();

            LoadingScreen.Load(new ControllerDisconnectionScreen("controllerDisconnection"));
            LoadingScreen.Load(new IISMessageBoxScreen("pressAnyKey", "Press any key to start"));
            LoadingScreen.Load(new BackgroundScreen("mainMenuBackground"));

            upTexture = Resources.GetTexture("up");
            downTexture = Resources.GetTexture("down");
            leftTexture = Resources.GetTexture("left");
            rightTexture = Resources.GetTexture("right");
            upLeftTexture = Resources.GetTexture("upLeft");
            upRightTexture = Resources.GetTexture("upRight");
            downLeftTexture = Resources.GetTexture("downLeft");
            downRightTexture = Resources.GetTexture("downRight");
            aButtonTexture = Resources.GetTexture("aButton");
            bButtonTexture = Resources.GetTexture("bButton");
            xButtonTexture = Resources.GetTexture("xButton");
            yButtonTexture = Resources.GetTexture("yButton");
            plusTexture = Resources.GetTexture("plus");
            padFaceTexture = Resources.GetTexture("padFace");
        }


        protected override void Update(GameTime gameTime)
        {
            //Debug.Update(gameTime);

            //Debug.Profiler.StartFrame();

            //Debug.Profiler.BeginMark("InputUpdate", Color.Yellow);
            Input.Update();
            //Debug.Profiler.EndMark("InputUpdate");

            //Debug.Profiler.BeginMark("AudioUpdate", Color.Red);
            //Audio.Update(gameTime);
            //Audio.UpdateListener();
            //Debug.Profiler.EndMark("AudioUpdate");

            //Debug.Profiler.BeginMark("ScreenManagerUpdate", Color.Violet);
            ScreenManager.Update(gameTime);
            //Debug.Profiler.EndMark("ScreenManagerUpdate");

            for (int i = 0; i < Input.MAX_USERS; ++i)
            {
                // Expire old moves.
                if (gameTime.TotalGameTime - playerMoveTimes[i] > MoveTimeout)
                    playerMoves[i] = null;

                // Detection and record of current player's most recent move.
                Move newMove = moveList.DetectMoves(i);

                if (newMove != null)
                {
                    playerMoves[i] = newMove;
                    playerMoveTimes[i] = gameTime.TotalGameTime;
                }
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            //Debug.Profiler.BeginMark("ScreenManagerDraw", Color.Green);
            //ScreenManager.Draw(gameTime);
            //Debug.Profiler.EndMark("ScreenManagerDraw");

            //Debug.Profiler.BeginMark("DebugDraw", Color.Green);
            //Debug.Draw(gameTime);
            //Debug.Profiler.EndMark("DebugDraw");

            //Debug.Profiler.BeginMark("baseDraw", Color.Green);
            //base.Draw(gameTime);
            //Debug.Profiler.EndMark("baseDraw");

            GraphicsDevice.Clear(Color.CornflowerBlue);

            ScreenManager.SpriteBatch.Begin();

            // Calculate some reasonable boundaries within the safe area.
            Vector2 topLeft = new Vector2(50f, 50f);
            Vector2 bottomRight = new Vector2(
                GraphicsDevice.Viewport.Width - topLeft.X,
                GraphicsDevice.Viewport.Height - topLeft.Y);

            // Keeps track of where to draw next.
            Vector2 position = topLeft;

            // Draw the list of all moves, backwards, since they were reversed inside the moveList.
            for (int i = moves.Length - 1; i >= 0; i--)
            {
                Vector2 size = MeasureMove(moves[i]);

                // If this move would fall off the right edge of the screen.
                if (position.X + size.X > bottomRight.X)
                {
                    // start again on the next line.
                    position.X = topLeft.X;
                    position.Y += size.Y;
                }

                DrawMove(moves[i], position);
                position.X += size.X + 30f;
            }

            position.Y += 90f;

            // Draw the input from each player.
            for (int i = 0; i < Input.MAX_USERS; ++i)
            {
                position.X = topLeft.X;
                DrawInput(i, position);
                position.Y += 80f;
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        // Calculates the size of what would be drawn by a call to DrawMove.
        Vector2 MeasureMove(Move move)
        {
            Vector2 textSize = Debug.Font.MeasureString(move.Name);
            Vector2 sequenceSize = MeasureSequence(move.Sequence);
            return new Vector2(Math.Max(textSize.X, sequenceSize.X), textSize.Y + sequenceSize.Y);
        }


        // Draws graphical instructions on how to perform a move.
        void DrawMove(Move move, Vector2 position)
        {
            DrawString(move.Name, position, Color.White);
            position.Y += Debug.Font.MeasureString(move.Name).Y;
            DrawSequence(move.Sequence, position);
        }


        // Draws the input buffer and the most recently fired action for a given player.
        void DrawInput(int i, Vector2 position)
        {
            // Draw the player's name and currently active move, if any.
            string text = "Player: " + (i + 1).ToString() + " input: ";
            Vector2 textSize = Debug.Font.MeasureString(text);
            DrawString(text, position, Color.White);

            if (playerMoves[i] != null)
            {
                DrawString(playerMoves[i].Name, new Vector2(
                    position.X + textSize.X, position.Y), Color.Red);
            }

            // Draw the player's input buffer.
            position.Y += textSize.Y;
            DrawSequence(Input.Buffers[i], position);
        }


        // Draws a string with a subtle drop shadow.
        void DrawString(string text, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(Debug.Font, text, new Vector2(position.X, position.Y + 1), Color.Black);
            ScreenManager.SpriteBatch.DrawString(Debug.Font, text, new Vector2(position.X, position.Y), color);
        }


        // Calculates the size of what would be drawn by a call to DrawSequence.
        Vector2 MeasureSequence(System.Collections.Generic.IEnumerable<Buttons> sequence)
        {
            float width = 0f;

            foreach (Buttons button in sequence)
                width += MeasureButtons(button).X;

            return new Vector2(width, padFaceTexture.Height);
        }


        // Draws a horizontal series of input steps in a sequence.
        void DrawSequence(System.Collections.Generic.IEnumerable<Buttons> sequence, Vector2 position)
        {
            foreach (Buttons button in sequence)
            {
                DrawButtons(button, position);
                position.X += MeasureButtons(button).X;
            }
        }


        // Calculates the size of what would be drawn by a call to DrawButtons.
        Vector2 MeasureButtons(Buttons buttons)
        {
            Buttons direction = Direction.FromButtons(buttons);
            float width = 0f;

            // If the buttons have a direction.
            if (direction > 0)
            {
                width = GetDirectionTexture(direction).Width;

                // If the buttons have at least one non-directional button.
                if ((buttons & ~direction) > 0)
                    width += plusTexture.Width + padFaceTexture.Width;
            }
            else
                width = padFaceTexture.Width;

            return new Vector2(width, padFaceTexture.Height);
        }


        // Draws the combined state of a set of buttons flags. The rendered output looks like a
        // directional arrow, a group of buttons, or both concatenated with a plus sign operator.
        void DrawButtons(Buttons buttons, Vector2 position)
        {
            // Get the texture to draw for the direction.
            Buttons direction = Direction.FromButtons(buttons);
            Texture2D directionTexture = GetDirectionTexture(direction);

            // If there is a direction, draw it.
            if (directionTexture != null)
            {
                ScreenManager.SpriteBatch.Draw(directionTexture, position, Color.White);
                position.X += directionTexture.Width;
            }

            // If any non-directional button is pressed
            if ((buttons & ~direction) > 0)
            {
                // Draw a plus if both a direction and one more button is pressed.
                if (directionTexture != null)
                {
                    ScreenManager.SpriteBatch.Draw(plusTexture, position, Color.White);
                    position.X += plusTexture.Width;
                }

                // Draw a gamePad with all inactive buttons in the background.
                ScreenManager.SpriteBatch.Draw(padFaceTexture, position, Color.White);

                // Draw each active button over the inactive gamePad face.
                if ((buttons & Buttons.A) > 0)
                    ScreenManager.SpriteBatch.Draw(aButtonTexture, position, Color.White);
                if ((buttons & Buttons.B) > 0)
                    ScreenManager.SpriteBatch.Draw(bButtonTexture, position, Color.White);
                if ((buttons & Buttons.X) > 0)
                    ScreenManager.SpriteBatch.Draw(xButtonTexture, position, Color.White);
                if ((buttons & Buttons.Y) > 0)
                    ScreenManager.SpriteBatch.Draw(yButtonTexture, position, Color.White);
            }
        }


        // Gets the texture for a given direction.
        Texture2D GetDirectionTexture(Buttons direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return upTexture;
                case Direction.Down:
                    return downTexture;
                case Direction.Left:
                    return leftTexture;
                case Direction.Right:
                    return rightTexture;
                case Direction.UpLeft:
                    return upLeftTexture;
                case Direction.UpRight:
                    return upRightTexture;
                case Direction.DownLeft:
                    return downLeftTexture;
                case Direction.DownRight:
                    return downRightTexture;
                default:
                    return null;
            }
        }
    }
}