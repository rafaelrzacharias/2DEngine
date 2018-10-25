using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameStateManager
{
    // Helper class represents a single entry in a MenuScreen. By default this just draws the entry
    // text string, but it can be customized to display menu entries in different ways. This also
    // provides an event that will be raised when the menu entry is selected.
    public class MenuEntry
    {
        private SpriteFont Font;
        private Color TextDefaultColor;
        private Color TextSelectedColor;
        private Color TextColor;
        private Color TextureDefaultColor;
        private Color TextureSelectedColor;
        private Color TextureColor;
        private Vector2 Origin;
        public bool IsHighlighted;

        // Tracks a fading selection effect on the entry. Entries transition out when deselected.
        private float selectionFade;

        // A spriteBatch that targets the ScreenManager spriteBatch
        protected SpriteBatch SpriteBatch;

        // Gets or sets the text rendered for this entry.
        public string Text { get; set; }

        // Queries how much space this menu entry requires.
        public int Height { get; private set; }

        // Queries ho wide this menu entry is. Used for centering on the screen.
        public int Width { get; private set; }

        // The position at which the entry is drawn. This is set by the MenuScreen each frame in Update.
        public Vector2 Position;

        // The texture for the entry background.
        public Texture2D Texture { get; private set; }

        // The bounds of the menu entry.
        public Rectangle Bounds;

        public float Scale { get; set; }

        public float Rotation { get; protected set; }

        // Event raised when the menu entry is selected.
        public delegate void SelectedEventHandler(PlayerIndex playerIndex);
        public event SelectedEventHandler Selected;

        public virtual void OnSelected(PlayerIndex playerIndex)
        {
            if (Selected != null)
                Selected.Invoke(playerIndex);
        }


        // Constructs a new menu entry with the specified text.
        public MenuEntry(string text)
        {
            SpriteBatch = ScreenManager.SpriteBatch;
            Text = text;
            TextDefaultColor = Color.White;
            TextSelectedColor = Color.Yellow;
            TextureDefaultColor = Color.Black;
            TextureSelectedColor = Color.Gray;
            Scale = 1f;
            Rotation = 0f;
            Font = Resources.GetFont("menuFont");

            Vector2 textSize = Font.MeasureString(Text);
            Origin = textSize / 2f;
            Width = (int)textSize.X;
            Height = (int)textSize.Y;
            Bounds = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }


        // Update the menu entry.
        public virtual void Update(MenuScreen screen, GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between their selected
            // and deselected appearance, rather than instantly popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4f;

            if (IsHighlighted)
            {
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1f);
                TextureColor = TextureSelectedColor * screen.TransitionAlpha;
                TextColor = TextSelectedColor;
            }
            else
            {
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0f);
                TextureColor = TextureDefaultColor * screen.TransitionAlpha;
                TextColor = TextDefaultColor;
            }

            // Pulsate the size of the selected menu entry.
            float pulsate = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 6) + 1;
            Scale = 1f + pulsate * 0.05f * selectionFade;

            UpdateBounds();
        }


        // Updates the Bounds of the entry.
        private void UpdateBounds()
        {
            Bounds.X = (int)(Position.X * Scale);
            Bounds.Y = (int)(Position.Y * Scale);
            Bounds.Width = (int)(Position.X + Bounds.Width * Scale);
            Bounds.Height = (int)(Position.Y + Bounds.Height * Scale);
        }


        // Draws the menu entry. This can be overridden to customize the appearance.
        public virtual void Draw(MenuScreen screen, GameTime gameTime)
        {
            if (Texture != null)
                SpriteBatch.Draw(Texture, Position, TextureColor);

            SpriteBatch.Draw(Resources.GetTexture("whiteTexture"), Bounds, new Color(255, 0, 0, 50));

            SpriteBatch.DrawString(Font, Text, Position, TextColor, 
                Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }
    }
}