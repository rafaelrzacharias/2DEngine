using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManager
{
    // Component for FPS measure and draw.
    public class FpsCounter : DebugScreen
    {
        private Stopwatch stopwatch; // Stopwatch for fps measuring.
        private int sampleFrames;
        private Color fontColor;
        private StringBuilder stringBuilder; // StringBuilder for Fps counter draw.

        // Gets/Sets Fps sample duration.
        public TimeSpan SampleSpan { get; set; }


        // Initializes the FpsCounter.
        public override void Initialize()
        {
            base.Initialize();

            stopwatch = Stopwatch.StartNew();
            SampleSpan = TimeSpan.FromSeconds(1.0);
            stringBuilder = new StringBuilder(16);
            stringBuilder.Length = 0;

            Area.X = ScreenManager.Viewport.Width - 100;
            Area.Width = 400;
            Area.Height = 2 * Font.LineSpacing;
            TextPosition = new Vector2(Area.X + 10f, Area.Y + 10f);

            // Register 'fps' command if debug command is registered as a service.
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("fps", "Toggle FPS counter on/off", CommandExecute);
        }


        public override void Update(GameTime gameTime)
        {
            if (stopwatch.Elapsed > SampleSpan)
            {
                // Update Fps value and start next sampling period.
                float fps = sampleFrames / (float)stopwatch.Elapsed.TotalSeconds;

                if (fps < 20f)
                    fontColor = Color.Red;
                else if (fps < 40f)
                    fontColor = Color.Yellow;
                else
                    fontColor = Color.Green;

                stopwatch.Reset();
                stopwatch.Start();
                sampleFrames = 0;

                // Update draw string.
                stringBuilder.Length = 0;
                stringBuilder.Append("FPS: ");
                stringBuilder.AppendNumber(fps);
            }
        }


        public override void Draw(GameTime gameTime)
        {
            sampleFrames++;

            SpriteBatch.Begin();
            SpriteBatch.Draw(Texture, Area, AreaColor);
            SpriteBatch.DrawString(Font, stringBuilder, TextPosition, fontColor);
            SpriteBatch.End();
        }
    }
}