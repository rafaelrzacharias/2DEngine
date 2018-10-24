using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameStateManager
{
    public class GraphicsStats : DebugScreen
    {
        // Initializes the GraphicsStats
        public override void Initialize()
        {
            base.Initialize();

            Area.Width = (int)(ScreenManager.Viewport.Width * 0.5f);

            // Register 'graphics' command if debug command is registered as a service.
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("graphics", "Graphics Stats", CommandExecute);
        }


        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();

            Area.Height = Font.LineSpacing * (Resources.Textures.Count + Resources.Fonts.Count + 5);
            SpriteBatch.Draw(Texture, Area, AreaColor);

            TextPosition.Y = 0;
            SpriteBatch.DrawString(Font, "========== GraphicsStats ==========", TextPosition, Color.Yellow);
            TextPosition.Y += 2 * Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Textures loaded: " + Resources.Textures.Count.ToString(), TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            foreach (KeyValuePair<string, Texture2D> pair in Resources.Textures)
            {
                SpriteBatch.DrawString(Font, "Name: " + pair.Key + ", Is disposed: " + 
                    pair.Value.IsDisposed.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }

            TextPosition.Y += Font.LineSpacing;
            SpriteBatch.DrawString(Font, "Fonts loaded: " + Resources.Fonts.Count.ToString(), TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            foreach (KeyValuePair<string, SpriteFont> pair in Resources.Fonts)
            {
                SpriteBatch.DrawString(Font, "Name: " + pair.Key +
                    ", Is disposed: " + pair.Value.Texture.IsDisposed.ToString(), TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }

            SpriteBatch.End();
        }
    }
}