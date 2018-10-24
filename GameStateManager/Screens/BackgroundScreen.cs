using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManager
{
    // The background screen sits behind all the other menu screens.
    // It draws a background image that remains fixed in place regardless
    // of whatever transitions the screens on top of it may be doing.
    public class BackgroundScreen : Screen
    {
        private Texture2D texture;


        // Constructs a background screen.
        public BackgroundScreen()
        {
            texture = Resources.GetTexture("background");

            OnShow();
        }


        // Draws the background screen.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                ScreenManager.SpriteBatch.Draw(texture, ScreenManager.Viewport.Bounds,
                    new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            }
        }
    }
}