using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            aButtonTexture = Resources.GetTexture("aButtonPadFace");
            bButtonTexture = Resources.GetTexture("bButtonPadFace");
            xButtonTexture = Resources.GetTexture("xButtonPadFace");
            yButtonTexture = Resources.GetTexture("yButtonPadFace");
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
            if (Input.IsActionPressed(Action.UI_BACK, PrimaryUser))
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
                for (int i = BufferedInput.MoveList.Length - 1; i >= 0; i--)
                {
                    Vector2 size = MeasureMove(BufferedInput.MoveList[i]);

                    // If this move would fall off the right edge of the screen.
                    if (position.X + size.X > BottomRight.X)
                    {
                        // start again on the next line.
                        position.X = TopLeft.X;
                        position.Y += size.Y;
                    }

                    DrawMove(BufferedInput.MoveList[i], position);
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
            BufferedInput.ClearInputBuffers();
            base.OnHide();
        }


        // Calculates the size of what would be drawn by a call to DrawMove.
        Vector2 MeasureMove(Move move)
        {
            Vector2 textSize = Font.MeasureString(move.Name);

            Action[] actions = new Action[move.Sequence.Length];
            for (int i = 0; i < move.Sequence.Length; i++)
                actions[i] = move.Sequence[i];

            Vector2 sequenceSize = MeasureSequence(actions);
            return new Vector2(Math.Max(textSize.X, sequenceSize.X), textSize.Y + sequenceSize.Y);
        }


        // Draws graphical instructions on how to perform a move.
        void DrawMove(Move move, Vector2 position)
        {
            DrawString(move.Name, position, Color.White);
            position.Y += Font.MeasureString(move.Name).Y;

            Action[] actions = new Action[move.Sequence.Length];
            for (int i = 0; i < move.Sequence.Length; i++)
                actions[i] = move.Sequence[i];

            DrawSequence(actions, position);
        }


        // Draws the input buffer and the most recently fired action for a given player.
        void DrawInput(int i, Vector2 position)
        {
            // Draw the player's name and currently active move, if any.
            string text = "Player: " + (i + 1).ToString() + " input: ";
            Vector2 textSize = Font.MeasureString(text);
            DrawString(text, position, Color.White);

            if (BufferedInput.PlayersMove[i] != null)
            {
                DrawString(BufferedInput.PlayersMove[i].Name, new Vector2(
                    position.X + textSize.X, position.Y), Color.Red);
            }

            // Draw the player's input buffer.
            position.Y += textSize.Y;
            DrawSequence(BufferedInput.Buffers[i], position);
        }


        // Draws a string with a subtle drop shadow.
        void DrawString(string text, Vector2 position, Color color)
        {
            SpriteBatch.DrawString(Font, text, new Vector2(position.X, position.Y + 1), Color.Black);
            SpriteBatch.DrawString(Font, text, new Vector2(position.X, position.Y), color);
        }


        // Calculates the size of what would be drawn by a call to DrawSequence.
        Vector2 MeasureSequence(IEnumerable<Action> sequence)
        {
            float width = 0f;

            foreach (Action action in sequence)
                width += MeasureButtons(action).X;

            return new Vector2(width, padFaceTexture.Height);
        }


        // Draws a horizontal series of input steps in a sequence.
        void DrawSequence(IEnumerable<Action> sequence, Vector2 position)
        {
            foreach (Action action in sequence)
            {
                DrawButtons(action, position);
                position.X += MeasureButtons(action).X;
            }
        }


        // Calculates the size of what would be drawn by a call to DrawButtons.
        Vector2 MeasureButtons(Action action)
        {
            //Action direction = GetDirectionFromAction(action);
            Action direction = action & (Action.UP | Action.DOWN | Action.LEFT | Action.RIGHT);
            float width = 0f;

            // If the buttons have a direction.
            if (direction != Action.NONE)
            {
                width = GetDirectionTexture(direction).Width;

                // If the buttons have at least one non-directional button.
                if ((action & ~direction) != Action.NONE)
                    width += plusTexture.Width + padFaceTexture.Width;
            }
            else
                width = padFaceTexture.Width;

            return new Vector2(width, padFaceTexture.Height);
        }


        // Draws the combined state of a set of buttons flags. The rendered output looks like a
        // directional arrow, a group of buttons, or both concatenated with a plus sign operator.
        void DrawButtons(Action action, Vector2 position)
        {
            // Get the texture to draw for the direction.
            Action direction = action & (Action.UP | Action.DOWN | Action.LEFT | Action.RIGHT);
            //Action direction = GetDirectionFromAction(action);
            Texture2D directionTexture = GetDirectionTexture(direction);

            // If there is a direction, draw it.
            if (directionTexture != null)
            {
                SpriteBatch.Draw(directionTexture, position, Color.White);
                position.X += directionTexture.Width;
            }

            // If any non-directional button is pressed
            if ((action & ~direction) != Action.NONE)
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
                if ((action & Action.LK) != Action.NONE)
                    SpriteBatch.Draw(aButtonTexture, position, Color.White);
                if ((action & Action.HK) != Action.NONE)
                    SpriteBatch.Draw(bButtonTexture, position, Color.White);
                if ((action & Action.LP) != Action.NONE)
                    SpriteBatch.Draw(xButtonTexture, position, Color.White);
                if ((action & Action.HP) != Action.NONE)
                    SpriteBatch.Draw(yButtonTexture, position, Color.White);
            }
        }


        // Gets the direction without non-direction buttons from the Button enum
        // and extract the direction from a full set of buttons using the bitmask.
        private Action GetDirectionFromAction(Action action)
        {
            switch (action)
            {
                case Action.UP:
                    return Action.UP;
                case Action.DOWN:
                    return Action.DOWN;
                case Action.LEFT:
                    return Action.LEFT;
                case Action.RIGHT:
                    return Action.RIGHT;
                case Action.UP | Action.LEFT:
                    return Action.UP | Action.LEFT;
                case Action.UP | Action.RIGHT:
                    return Action.UP | Action.RIGHT;
                case Action.DOWN | Action.LEFT:
                    return Action.DOWN | Action.LEFT;
                case Action.DOWN | Action.RIGHT:
                    return Action.DOWN | Action.RIGHT;
                default:
                    return action & (Action.UP | Action.DOWN | Action.LEFT | Action.RIGHT);
            }
        }


        // Gets the texture for a given direction.
        private Texture2D GetDirectionTexture(Action direction)
        {
            switch (direction)
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