using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameStateManager
{
    public class ScreenStats : DebugScreen
    {
        private List<Screen> screens;


        // Initializes the ScreenTracing.
        public override void Initialize()
        {
            base.Initialize();
            Area.Width = (int)(ScreenManager.Viewport.Width * 0.5f);

            screens = new List<Screen>();

            // Register 'screen' command if debug command is registered as a service.
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("screen", "Toggle screen stats on/off", CommandExecute);
        }


        // Prints a list of all the screens, for debugging.
        public override void Draw(GameTime gameTime)
        {
            screens = ScreenManager.Screens;
            Area.Height = 2 * Font.LineSpacing + Font.LineSpacing * 5 * screens.Count;

            SpriteBatch.Begin();
            SpriteBatch.Draw(Texture, Area, AreaColor);

            TextPosition.X = 0f;
            TextPosition.Y = 0f;
            SpriteBatch.DrawString(Font, "========== ScreenStats ==========", TextPosition, Color.Yellow);
            TextPosition.Y += 2 * Font.LineSpacing;

            for (int i = 0; i < screens.Count; i++)
            {
                TextPosition.X = 0f;
                SpriteBatch.DrawString(Font, screens[i].Name + " (" + screens[i].GetType().Name + ")", TextPosition, Color.Yellow);
                TextPosition.Y += Font.LineSpacing;
                SpriteBatch.DrawString(Font, "Screen state: " + screens[i].TransitionState.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
                SpriteBatch.DrawString(Font, "Supported gestures: " + screens[i].EnabledGestures.ToString(), TextPosition, Color.White);
                TextPosition.X = ScreenManager.Viewport.Width * 0.25f;
                TextPosition.Y = Font.LineSpacing * (i * 4 + 3);
                SpriteBatch.DrawString(Font, "Is enabled: " + screens[i].IsEnabled.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
                SpriteBatch.DrawString(Font, "Is visible: " + screens[i].IsVisible.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing * 2f;
            }

            SpriteBatch.End();
        }
    }
}