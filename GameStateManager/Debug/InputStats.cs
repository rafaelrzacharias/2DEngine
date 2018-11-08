using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace GameStateManager
{
    public class InputStats : DebugScreen
    {
        // Initializes the InputStats
        public override void Initialize()
        {
            base.Initialize();

            // Register 'input' command if debug command is registered as a service
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("input", "Toggle input stats on/off", CommandExecute);
        }


        public override void Draw(GameTime gameTime)
        {
            TextPosition.X = 0f;
            TextPosition.Y = 0f;
            Area.Height = Font.LineSpacing * 40;

            SpriteBatch.Begin();

            SpriteBatch.Draw(Texture, Area, AreaColor);

            SpriteBatch.DrawString(Font, "========== InputStats ==========", TextPosition, Color.Yellow);
            TextPosition.Y += 2 * Font.LineSpacing;
#if DESKTOP
            SpriteBatch.DrawString(Font, "Mouse state: ", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            string mouseLeft = Input.IsMouseDown(MouseButton.LEFT) ? "Pressed" : "Released";
            string mouseMiddle = Input.IsMouseDown(MouseButton.MIDDLE) ? "Pressed" : "Released";
            string mouseRight = Input.IsMouseDown(MouseButton.RIGHT) ? "Pressed" : "Released";
            string wheelValue = Input.CurrentMouseState.ScrollWheelValue.ToString();
            string isDragging = Input.isDragging.ToString();
            string isDragComplete = Input.isDragComplete.ToString();
            string currentMousePosition = Input.CurrentMouseState.Position.ToString();
            string mouseDragStart = Input.MouseDragStartPosition.ToString();
            string mouseDragEnd = Input.MouseDragEndPosition.ToString();
            string mouseDragDelta = Input.MouseDragDelta.ToString();
            string mouseDragDistance = Input.MouseDragDistance.ToString();

            SpriteBatch.DrawString(Font, "Left button: " + mouseLeft + ", Middle button: " + mouseMiddle + ", Right Button: " + mouseRight +
                ", ScrollWheelValue: " + wheelValue, TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Is dragging: " + isDragging + ", Is drag complete: " +
                isDragComplete, TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Current position: " + currentMousePosition + ", Drag start position: " +
                mouseDragStart + ", Drag end position: " + mouseDragEnd, TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Drag delta: " + mouseDragDelta +
                ", Drag Distance: " + mouseDragDistance, TextPosition, Color.White);
            TextPosition.Y += 2 * Font.LineSpacing;
#endif
#if DESKTOP || CONSOLE
            SpriteBatch.DrawString(Font, "Gamepad states:", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                GamePadState gamePad = Input.CurrentGamePadState[i];

                TextPosition.X = 0f;
                SpriteBatch.DrawString(Font, "No." + (i + 1).ToString() + ", Is connected: " +
                    gamePad.IsConnected.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;

                SpriteBatch.DrawString(Font, "Buttons: ", TextPosition, Color.White);
                Color buttonColor;

                if (gamePad.IsButtonDown(Buttons.A))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                SpriteBatch.DrawString(Font, "A, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.B))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("A, ").X;
                SpriteBatch.DrawString(Font, "B, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.X))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("B, ").X;
                SpriteBatch.DrawString(Font, "X, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.Y))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("X, ").X;
                SpriteBatch.DrawString(Font, "Y, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.Back))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Y, ").X;
                SpriteBatch.DrawString(Font, "Back, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.Start))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Back, ").X;
                SpriteBatch.DrawString(Font, "Start, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.LeftShoulder))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Start, ").X;
                SpriteBatch.DrawString(Font, "LeftShoulder, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.RightShoulder))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("LeftShoulder, ").X;
                SpriteBatch.DrawString(Font, "RightShoulder, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.LeftTrigger))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("RightShoulder, ").X;
                string leftTrigger = "LeftTrigger (" + gamePad.Triggers.Left.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftTrigger, TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.RightTrigger))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftTrigger).X;
                string rightTrigger = "RightTrigger (" + gamePad.Triggers.Right.ToString() + "), ";
                SpriteBatch.DrawString(Font, rightTrigger, TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.LeftStick))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                TextPosition.Y += Font.LineSpacing;
                SpriteBatch.DrawString(Font, "LeftStick, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.RightStick))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("LeftStick, ").X;
                SpriteBatch.DrawString(Font, "RightStick, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.DPadDown))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("RightStick, ").X;
                SpriteBatch.DrawString(Font, "DPadDown, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.DPadUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadDown, ").X;
                SpriteBatch.DrawString(Font, "DPadUp, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.DPadLeft))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadUp, ").X;
                SpriteBatch.DrawString(Font, "DPadLeft, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.DPadRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadLeft, ").X;
                SpriteBatch.DrawString(Font, "DPadRight, ", TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.LeftThumbstickLeft) ||
                    gamePad.IsButtonDown(Buttons.LeftThumbstickRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                TextPosition.Y += Font.LineSpacing;

                string leftThumbstickX = "LeftStickX (" + gamePad.ThumbSticks.Left.X.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftThumbstickX, TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                    gamePad.IsButtonDown(Buttons.LeftThumbstickUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftThumbstickX).X;
                string leftThumbstickY = "LeftStickY (" + gamePad.ThumbSticks.Left.Y.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftThumbstickY, TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.RightThumbstickLeft) ||
                    gamePad.IsButtonDown(Buttons.RightThumbstickRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftThumbstickY).X;
                string rightThumbstickX = "RightStickX (" + gamePad.ThumbSticks.Right.X.ToString() + "), ";
                SpriteBatch.DrawString(Font, rightThumbstickX, TextPosition, buttonColor);

                if (gamePad.IsButtonDown(Buttons.RightThumbstickDown) ||
                    gamePad.IsButtonDown(Buttons.RightThumbstickUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(rightThumbstickX).X;
                string rightThumbstickY = "RightStickY (" + gamePad.ThumbSticks.Right.Y.ToString() + ")";
                SpriteBatch.DrawString(Font, rightThumbstickY, TextPosition, buttonColor);
                TextPosition.Y += Font.LineSpacing;

                TextPosition.X = 0;
                TextPosition.Y += Font.LineSpacing;
            }
#endif
#if MOBILE
            SpriteBatch.DrawString(Font, "Touch state: ", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, Input.TouchState.ToString(), TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;
#endif
            SpriteBatch.End();
        }
    }
}