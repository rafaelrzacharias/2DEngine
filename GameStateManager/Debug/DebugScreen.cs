using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameStateManager
{
    public abstract class DebugScreen
    {
        protected SpriteFont Font;
        protected SpriteBatch SpriteBatch;
        protected Texture2D Texture;
        protected Rectangle Area;
        protected Color AreaColor;
        protected Vector2 TextPosition;

        public virtual bool IsActive { get; set; }

        public virtual void Initialize()
        {
            SpriteBatch = ScreenManager.SpriteBatch;
            Font = Debug.Font;
            Texture = Debug.Texture;
            Area = new Rectangle(0, 0, ScreenManager.Viewport.Width, ScreenManager.Viewport.Height);
            AreaColor = Debug.AreaColor;
            TextPosition = Vector2.Zero;
            IsActive = false;
        }

        protected virtual void CommandExecute(IConsoleHost host, string command, List<string> arguments)
        {
            if (arguments.Count == 0)
                IsActive = !IsActive;
        }

        public virtual void Update(GameTime gametime) { }

        public abstract void Draw(GameTime gameTime);
    }
}