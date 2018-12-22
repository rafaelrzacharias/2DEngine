using Microsoft.Xna.Framework;
using System;

namespace GameStateManager
{
    public static class Utils
    {
        // Pulse a color by a given speed.
        public static void PulseColor(ref Color color, float speed = 4f, float transparencyThreshold = 0.5f)
        {
            float pulse = (float)(Math.Sin(Input.GameTime.TotalGameTime.TotalSeconds * speed) / 2 + 0.5f);
            color.A = (byte)(255 * transparencyThreshold + (255 - 255 * transparencyThreshold) * pulse);
        }
    }
}