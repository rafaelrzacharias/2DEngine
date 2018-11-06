using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManager
{
    public enum MessageBoxType
    {
        NONE,
        YESNO,
        OK
    }

    // A popup message box screen, used to display confirmation messages.
    public class MessageBoxScreen : MenuScreen
    {
        public string Text;
        protected Color TextColor;
        private Vector2 TextPosition;
        private Texture2D Texture;
        private Rectangle BackgroundArea;
        protected bool ShouldDarkenBackground;
        private Vector2 Padding;
        private Vector2 TextSize;
        private Vector2 Origin;
        private MessageBoxType MessageBoxType;

        // Constructs a message box in which the caller specifies the prompt.
        public MessageBoxScreen(string screenName, string message, string menuTitle = "", MessageBoxType type = MessageBoxType.NONE)
            : base(menuTitle)
        {
            MessageBoxType = type;
            Name = screenName;
            BackgroundColor = new Color(0, 0, 0, 128);
            Font = Resources.GetFont("menuFont");
            Texture = Resources.GetTexture("whiteTexture");
            EnabledGestures = GestureType.Tap;
            DrawOrder = 0.2f;
            ShouldDarkenBackground = true;
            Text = message;

            TextColor = Color.Yellow;
            MenuTitle = menuTitle;
            TextPosition = new Vector2(ScreenManager.Viewport.Width, ScreenManager.Viewport.Height) * 0.5f;
            TextSize = Font.MeasureString(Text);
            Origin = TextSize * 0.5f;
            Padding = new Vector2(32, 32);
            BackgroundArea = Rectangle.Empty;

            switch (MessageBoxType)
            {
                case MessageBoxType.YESNO:
                        Entries.Add(new MenuEntry("Yes"));
                        Entries.Add(new MenuEntry("No"));
                    break;
                case MessageBoxType.OK:
                        Entries.Add(new MenuEntry("OK"));
                    break;
            }
        }


        // Override for the menu entries, so that they will not slide in/out of the screen.
        protected override void UpdateMenuEntryLocations()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].Position.X = TextPosition.X - Origin.X + 
                    (i + 1) * TextSize.X / Entries.Count - TextSize.X / (Entries.Count + 1);
                Entries[i].Position.Y = TextPosition.Y - Origin.Y + 2 * Font.LineSpacing;

                Entries[i].UpdateBounds();
            }

            BackgroundArea.Width = (int)TextSize.X + (int)Padding.X;

            if (Entries.Count > 0)
            {
                BackgroundArea.Height = (int)Entries[Entries.Count - 1].Position.Y - (int)TextPosition.Y +
                    + (int)TextSize.Y + (int)Padding.Y;
            }
            else
                BackgroundArea.Height = (int)TextSize.Y + (int)Padding.Y;
        }


        // Draws the message box.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                // Darken all other screens that were drawn underneath the popup.
                if (ShouldDarkenBackground)
                    FadeScreen(TransitionAlpha * 0.66f);

                SpriteBatch.Draw(Texture, TextPosition, BackgroundArea, BackgroundColor * TransitionAlpha,
                    0f, Origin + Padding * 0.5f, 1f, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(Font, Text, TextPosition, TextColor * TransitionAlpha,
                    0f, Origin, 1f, SpriteEffects.None, 0f);
            }

            base.Draw(gameTime);
        }
    }
}