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
        public Vector2 Origin;
        public bool IsHighlighted;
        public bool IsSelected;

        // Tracks a fading selection effect on the entry. Entries transition out when deselected.
        private float selectionFade;

        // A spriteBatch that targets the ScreenManager spriteBatch
        protected SpriteBatch SpriteBatch;

        // Gets or sets the text rendered for this entry.
        public string Text { get; set; }

        // The horizontal or vertical padding (in pixels) to the right or bellow the entry.
        public const int HorizontalPadding = 64;
        public const int VerticalPadding = 64;

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
            //Audio.PlaySound("entrySelected");

            if (Selected != null)
                Selected.Invoke(playerIndex);
        }

        // Event raised when the menu entry is highlighted.
        public delegate void BeginHighlightedEventHandler();
        public event BeginHighlightedEventHandler BeginHighlighted;

        public virtual void OnBeginHighlighted()
        {
            if (IsHighlighted == false)
            {
                IsHighlighted = true;
                //Audio.PlaySound("entryHighlighted");

                if (BeginHighlighted != null)
                    BeginHighlighted.Invoke();
            }
        }


        // Event raised when the menu entry is not highlighted anymore.
        public delegate void EndHighlightedEventHandler();
        public event EndHighlightedEventHandler EndHighlighted;

        public virtual void OnEndHighlighted()
        {
            if (IsHighlighted)
            {
                IsHighlighted = false;

                if (EndHighlighted != null)
                    EndHighlighted.Invoke();
            }
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
            Bounds = new Rectangle(0, 0, Width, Height);
        }


        // Sets up the interactable area for mouse hover and click.
        public void UpdateBounds()
        {
            Bounds.X = (int)(Position.X - Origin.X);
            Bounds.Y = (int)(Position.Y - Origin.Y);
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
                Utils.PulseColor(ref TextColor);
            }
            else
            {
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0f);
                TextureColor = TextureDefaultColor * screen.TransitionAlpha;
                TextColor = TextDefaultColor;
            }      
        }


        // Draws the menu entry. This can be overridden to customize the appearance.
        public virtual void Draw(MenuScreen screen, GameTime gameTime)
        {
            if (Texture != null)
                SpriteBatch.Draw(Texture, Position, TextureColor);

            SpriteBatch.DrawString(Font, Text, Position, TextColor,
                Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }
    }
}