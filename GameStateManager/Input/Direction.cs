using Microsoft.Xna.Framework.Input;

namespace GameStateManager
{
    // Helper class for working with the 8-way directions stored in a Buttons enum.
    public static class Direction
    {
        // Helper bitmasks for directions defined with the Buttons enum.
        public const Buttons None = 0;
        public const Buttons Up = Buttons.DPadUp | Buttons.LeftThumbstickUp;
        public const Buttons Down = Buttons.DPadDown | Buttons.LeftThumbstickDown;
        public const Buttons Left = Buttons.DPadLeft | Buttons.LeftThumbstickLeft;
        public const Buttons Right = Buttons.DPadRight | Buttons.LeftThumbstickRight;
        public const Buttons UpLeft = Up | Left;
        public const Buttons UpRight = Up | Right;
        public const Buttons DownLeft = Down | Left;
        public const Buttons DownRight = Down | Right;
        public const Buttons Any = Up | Down | Left | Right;


        // Gets the current direction from a gamepad and keyboard.
        public static Buttons FromInput(GamePadState gamePad, KeyboardState keyboard)
        {
            Buttons direction = None;

            // Get the vertical direction.
            if (gamePad.IsButtonDown(Buttons.DPadUp) || 
                gamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                keyboard.IsKeyDown(Keys.Up))
                direction |= Up;
            else if (gamePad.IsButtonDown(Buttons.DPadDown) || 
                gamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                keyboard.IsKeyDown(Keys.Down))
                direction |= Down;

            // Combine it with the horizontal direction.
            if (gamePad.IsButtonDown(Buttons.DPadLeft) || 
                gamePad.IsButtonDown(Buttons.LeftThumbstickLeft) ||
                keyboard.IsKeyDown(Keys.Left))
                direction |= Left;
            else if (gamePad.IsButtonDown(Buttons.DPadRight) || 
                gamePad.IsButtonDown(Buttons.LeftThumbstickRight) ||
                keyboard.IsKeyDown(Keys.Right))
                direction |= Right;

            return direction;
        }


        // Gets the direction without non-direction buttons from the Button enum
        // and extract the direction from a full set of buttons using the bitmask.
        public static Buttons FromButtons(Buttons buttons)
        {
            return buttons & Any;
        }
    }
}