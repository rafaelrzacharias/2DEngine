using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

            SpriteBatch.DrawString(Font, "Mouse state: ", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            PlayerIndex controllingPlayer = Input.ControllingPlayer;

            string mouseLeft = Input.IsMouseDown(MouseButton.Left, controllingPlayer, out PlayerIndex playerIndex) ? "Pressed" : "Released";
            string mouseMiddle = Input.IsMouseDown(MouseButton.Middle, controllingPlayer, out playerIndex) ? "Pressed" : "Released";
            string mouseRight = Input.IsMouseDown(MouseButton.Right, controllingPlayer, out playerIndex) ? "Pressed" : "Released";
            string wheelValue = Input.CurrentMouseState.ScrollWheelValue.ToString();

            SpriteBatch.DrawString(Font, "Left button: " + mouseLeft + ", Middle button: " + mouseMiddle + ", Right Button: " + mouseRight +
                ", ScrollWheelValue: " + wheelValue, TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Is dragging: " + Input.isDragging.ToString() + ", Is drag complete: " + 
                Input.isDragComplete.ToString(), TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Current position: " + Input.CurrentMousePosition.ToString() + ", Drag start position: " +
                Input.MouseDragStartPosition.ToString() + ", Drag end position: " + Input.MouseDragEndPosition.ToString() , TextPosition, Color.White);
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Drag delta: " + Input.MouseDragDelta.ToString() +
                ", Drag Distance: " + Input.MouseDragDistance.ToString(), TextPosition, Color.White);
            TextPosition.Y += 2 * Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Gamepad states:", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;
            
            for (int i = 0; i < Input.CurrentGamePadStates.Length; i++)
            {
                TextPosition.X = 0f;
                SpriteBatch.DrawString(Font, "No." + (i + 1).ToString() + ", Is connected: " + 
                    Input.IsGamePadConnected[i].ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;

                SpriteBatch.DrawString(Font, "Buttons: ", TextPosition, Color.White);
                Color buttonColor;

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.A))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                SpriteBatch.DrawString(Font, "A, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.B))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("A, ").X;
                SpriteBatch.DrawString(Font, "B, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.X))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("B, ").X;
                SpriteBatch.DrawString(Font, "X, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.Y))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("X, ").X;
                SpriteBatch.DrawString(Font, "Y, ", TextPosition, buttonColor);
                
                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.Back))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Y, ").X;
                SpriteBatch.DrawString(Font, "Back, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.Start))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Back, ").X;
                SpriteBatch.DrawString(Font, "Start, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftShoulder))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("Start, ").X;
                SpriteBatch.DrawString(Font, "LeftShoulder, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightShoulder))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("LeftShoulder, ").X;
                SpriteBatch.DrawString(Font, "RightShoulder, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftTrigger))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("RightShoulder, ").X;
                string leftTrigger = "LeftTrigger (" + Input.CurrentGamePadStates[i].Triggers.Left.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftTrigger, TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightTrigger))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftTrigger).X;
                string rightTrigger = "RightTrigger (" + Input.CurrentGamePadStates[i].Triggers.Right.ToString() + "), ";
                SpriteBatch.DrawString(Font, rightTrigger, TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftStick))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                TextPosition.Y += Font.LineSpacing;
                SpriteBatch.DrawString(Font, "LeftStick, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightStick))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("LeftStick, ").X;
                SpriteBatch.DrawString(Font, "RightStick, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.DPadDown))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("RightStick, ").X;
                SpriteBatch.DrawString(Font, "DPadDown, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.DPadUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadDown, ").X;
                SpriteBatch.DrawString(Font, "DPadUp, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.DPadLeft))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadUp, ").X;
                SpriteBatch.DrawString(Font, "DPadLeft, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.DPadRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString("DPadLeft, ").X;
                SpriteBatch.DrawString(Font, "DPadRight, ", TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftThumbstickLeft) ||
                    Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftThumbstickRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X = Font.MeasureString("Buttons: ").X;
                TextPosition.Y += Font.LineSpacing;

                string leftThumbstickX = "LeftStickX (" + Input.CurrentGamePadStates[i].ThumbSticks.Left.X.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftThumbstickX, TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftThumbstickDown) ||
                    Input.CurrentGamePadStates[i].IsButtonDown(Buttons.LeftThumbstickUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftThumbstickX).X;
                string leftThumbstickY = "LeftStickY (" + Input.CurrentGamePadStates[i].ThumbSticks.Left.Y.ToString() + "), ";
                SpriteBatch.DrawString(Font, leftThumbstickY, TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightThumbstickLeft) ||
                    Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightThumbstickRight))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(leftThumbstickY).X;
                string rightThumbstickX = "RightStickX (" + Input.CurrentGamePadStates[i].ThumbSticks.Right.X.ToString() + "), ";
                SpriteBatch.DrawString(Font, rightThumbstickX, TextPosition, buttonColor);

                if (Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightThumbstickDown) ||
                    Input.CurrentGamePadStates[i].IsButtonDown(Buttons.RightThumbstickUp))
                    buttonColor = Color.Yellow;
                else
                    buttonColor = Color.White;

                TextPosition.X += Font.MeasureString(rightThumbstickX).X;
                string rightThumbstickY = "RightStickY (" + Input.CurrentGamePadStates[i].ThumbSticks.Right.Y.ToString() + ")";
                SpriteBatch.DrawString(Font, rightThumbstickY, TextPosition, buttonColor);

                TextPosition.Y += Font.LineSpacing;
            }

            TextPosition.X = 0;
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Touch state: ", TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            for (int i = 0; i < Input.TouchState.Count; i++)
            {
                SpriteBatch.DrawString(Font, "No." + (i + 1).ToString() + ", Is connected: " + 
                    Input.TouchState.IsConnected.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;

                SpriteBatch.DrawString(Font, Input.TouchState[i].ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }

            SpriteBatch.End();
        }
    }
}