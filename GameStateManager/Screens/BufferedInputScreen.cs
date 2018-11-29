using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace GameStateManager
{
    public class BufferedInputScreen : Screen
    {
        private Texture2D upTexture;
        private Texture2D downTexture;
        private Texture2D leftTexture;
        private Texture2D rightTexture;
        private Texture2D upLeftTexture;
        private Texture2D upRightTexture;
        private Texture2D downLeftTexture;
        private Texture2D downRightTexture;
        private Texture2D aButtonTexture;
        private Texture2D bButtonTexture;
        private Texture2D xButtonTexture;
        private Texture2D yButtonTexture;
        private Texture2D plusTexture;
        private Texture2D padFaceTexture;
        private Vector2 TopLeft;
        private Vector2 BottomRight;


        // Constructs a BufferedInput screen.
        public BufferedInputScreen(string screenName)
        {
            Name = screenName;
            DrawOrder = 0.1f;
            Font = Resources.GetFont("menuFont");

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

            // Calculate some reasonable boundaries within the safe area.
            TopLeft = new Vector2(50f, 50f);
            BottomRight = new Vector2(ScreenManager.Viewport.Width - TopLeft.X,
                ScreenManager.Viewport.Height - TopLeft.Y);
        }


        // Handles the input to control the screen transitions.
        public override void HandleInput()
        {
            if (Input.WasButtonPressed(Action.HK, PrimaryUser))
                OnDismiss(PrimaryUser);
        }


        // Draws the BufferedInput screen.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                // Resets the position of where to draw next.
                Vector2 position = TopLeft;

                // Draw the list of all moves, backwards, since they were reversed inside the moveList.
                for (int i = Input.MoveList.Length - 1; i >= 0; i--)
                {
                    Vector2 size = MeasureMove(Input.MoveList[i]);

                    // If this move would fall off the right edge of the screen.
                    if (position.X + size.X > BottomRight.X)
                    {
                        // start again on the next line.
                        position.X = TopLeft.X;
                        position.Y += size.Y;
                    }

                    DrawMove(Input.MoveList[i], position);
                    position.X += size.X + 30f;
                }

                position.Y += 90f;

                // Draw the input from each player.
                for (int i = 0; i < Input.MAX_USERS; ++i)
                {
                    position.X = TopLeft.X;
                    DrawInput(i, position);
                    position.Y += 80f;
                }
            }

            base.Draw(gameTime);
        }


        // Overrides the OnHide default implementation to clear the buffer.
        public override void OnHide()
        {
            Input.ClearInputBuffers();
            base.OnHide();
        }


        // Calculates the size of what would be drawn by a call to DrawMove.
        Vector2 MeasureMove(Move move)
        {
            Vector2 textSize = Font.MeasureString(move.Name);

            Buttons[] buttons = new Buttons[move.Sequence.Length];
            for (int i = 0; i < move.Sequence.Length; i++)
                buttons[i] = (Buttons)move.Sequence[i];

            Vector2 sequenceSize = MeasureSequence(buttons);
            return new Vector2(Math.Max(textSize.X, sequenceSize.X), textSize.Y + sequenceSize.Y);
        }


        // Draws graphical instructions on how to perform a move.
        void DrawMove(Move move, Vector2 position)
        {
            DrawString(move.Name, position, Color.White);
            position.Y += Font.MeasureString(move.Name).Y;

            Buttons[] buttons = new Buttons[move.Sequence.Length];
            for (int i = 0; i < move.Sequence.Length; i++)
                buttons[i] = (Buttons)move.Sequence[i];

            DrawSequence(buttons, position);
        }


        // Draws the input buffer and the most recently fired action for a given player.
        void DrawInput(int i, Vector2 position)
        {
            // Draw the player's name and currently active move, if any.
            string text = "Player: " + (i + 1).ToString() + " input: ";
            Vector2 textSize = Font.MeasureString(text);
            DrawString(text, position, Color.White);

            if (Input.PlayersMove[i] != null)
            {
                DrawString(Input.PlayersMove[i].Name, new Vector2(
                    position.X + textSize.X, position.Y), Color.Red);
            }

            // Draw the player's input buffer.
            position.Y += textSize.Y;
            DrawSequence(Input.Buffers[i], position);
        }


        // Draws a string with a subtle drop shadow.
        void DrawString(string text, Vector2 position, Color color)
        {
            SpriteBatch.DrawString(Font, text, new Vector2(position.X, position.Y + 1), Color.Black);
            SpriteBatch.DrawString(Font, text, new Vector2(position.X, position.Y), color);
        }


        // Calculates the size of what would be drawn by a call to DrawSequence.
        Vector2 MeasureSequence(IEnumerable<Buttons> sequence)
        {
            float width = 0f;

            foreach (Buttons button in sequence)
                width += MeasureButtons(button).X;

            return new Vector2(width, padFaceTexture.Height);
        }


        // Draws a horizontal series of input steps in a sequence.
        void DrawSequence(IEnumerable<Buttons> sequence, Vector2 position)
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
            Buttons direction = Input.GetDirectionFromButtons(buttons);
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
            Buttons direction = Input.GetDirectionFromButtons(buttons);
            Texture2D directionTexture = GetDirectionTexture(direction);

            // If there is a direction, draw it.
            if (directionTexture != null)
            {
                SpriteBatch.Draw(directionTexture, position, Color.White);
                position.X += directionTexture.Width;
            }

            // If any non-directional button is pressed
            if ((buttons & ~direction) > 0)
            {
                // Draw a plus if both a direction and one more button is pressed.
                if (directionTexture != null)
                {
                    SpriteBatch.Draw(plusTexture, position, Color.White);
                    position.X += plusTexture.Width;
                }

                // Draw a gamePad with all inactive buttons in the background.
                SpriteBatch.Draw(padFaceTexture, position, Color.White);

                // Draw each active button over the inactive gamePad face.
                if ((buttons & Buttons.A) > 0)
                    SpriteBatch.Draw(aButtonTexture, position, Color.White);
                if ((buttons & Buttons.B) > 0)
                    SpriteBatch.Draw(bButtonTexture, position, Color.White);
                if ((buttons & Buttons.X) > 0)
                    SpriteBatch.Draw(xButtonTexture, position, Color.White);
                if ((buttons & Buttons.Y) > 0)
                    SpriteBatch.Draw(yButtonTexture, position, Color.White);
            }
        }


        // Gets the texture for a given direction.
        Texture2D GetDirectionTexture(Buttons direction)
        {
            switch ((Action)direction)
            {
                case Action.UP:
                    return upTexture;
                case Action.DOWN:
                    return downTexture;
                case Action.LEFT:
                    return leftTexture;
                case Action.RIGHT:
                    return rightTexture;
                case Action.UP | Action.LEFT:
                    return upLeftTexture;
                case Action.UP | Action.RIGHT:
                    return upRightTexture;
                case Action.DOWN | Action.LEFT:
                    return downLeftTexture;
                case Action.DOWN | Action.RIGHT:
                    return downRightTexture;
                default:
                    return null;
            }
        }
    }
}