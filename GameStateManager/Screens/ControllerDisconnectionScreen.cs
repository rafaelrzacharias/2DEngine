using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace GameStateManager
{
    public class ControllerDisconnectionScreen : MenuScreen
    {
        public string Text;
        protected Color TextColor;
        private Vector2 TextPosition;
        private Texture2D Texture;
        private Rectangle BackgroundArea;
        protected bool ShouldDarkenBackground;
        private Vector2 Padding;
        private Vector2 TextSize;
        private Vector2 PanelPosition;
        private Texture2D controllerTexture;
        private Texture2D keyboardTexture;
        private Texture2D inputTexture;
        private Color inputTextureColor;
        private Vector2 controllerTexturePosition;

        // Constructs a new controller disconnection screen.
        public ControllerDisconnectionScreen(string screenName, string menuTitle = "")
            : base(menuTitle)
        {
            Name = screenName;
            BackgroundColor = new Color(0, 0, 0, 128);
            Font = Resources.GetFont("menuFont");
            Texture = Resources.GetTexture("whiteTexture");
            controllerTexture = Resources.GetTexture("controllerTexture");
            keyboardTexture = Resources.GetTexture("keyboardTexture");
            inputTextureColor = Color.Red;
            EnabledGestures = GestureType.Tap;
            DrawOrder = 0.2f;
            ShouldDarkenBackground = true;
            Padding = new Vector2(16, 16);

            PanelPosition = new Vector2(ScreenManager.Viewport.Width * 0.5f, ScreenManager.Viewport.Height * 0.5f);

            BackgroundArea = Rectangle.Empty;
            BackgroundArea.Width = (int)((controllerTexture.Width * 0.2f + Padding.X) * Input.MAX_USERS);
            BackgroundArea.Height = (int)(controllerTexture.Height * 0.2f + 2f * Font.LineSpacing + 3f * Padding.Y);
            BackgroundArea.X = (int)(PanelPosition.X - BackgroundArea.Width * 0.5f);
            BackgroundArea.Y = (int)(PanelPosition.Y - BackgroundArea.Height * 0.5f);


            Text = "Controller Disconnected!";
            TextSize = Font.MeasureString(Text);
            TextColor = Color.Yellow;
            TextPosition = new Vector2(PanelPosition.X - TextSize.X * 0.5f, BackgroundArea.Top + Padding.Y);

            controllerTexturePosition.X = PanelPosition.X - (Input.MAX_USERS - 1) * Padding.X * 0.5f - 
                (Input.MAX_USERS - 1) * 0.5f * controllerTexture.Width * 0.2f - controllerTexture.Width * 0.2f * 0.5f;
            controllerTexturePosition.Y = TextPosition.Y + TextSize.Y + Padding.Y;

            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                Entries.Add(new MenuEntry("Player " + (i + 1).ToString()));
                Entries[i].Position.X += controllerTexturePosition.X + controllerTexture.Width * 0.2f * 0.5f + i * (controllerTexture.Width * 0.2f + Padding.X);
                Entries[i].Position.Y = controllerTexturePosition.Y + controllerTexture.Height * 0.2f + Padding.Y;

                Entries[i].UpdateBounds();
            }
        }


        // Override for the menu entries, so that they will not slide in/out of the screen.
        protected override void UpdateMenuEntryLocations() { }


        // Draws the message box.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                // Darken all other screens that were drawn underneath the popup.
                if (ShouldDarkenBackground)
                    FadeScreen(TransitionAlpha * 0.66f);

                SpriteBatch.Draw(Texture, PanelPosition, BackgroundArea, BackgroundColor * TransitionAlpha, 0f, 
                    new Vector2(BackgroundArea.Width * 0.5f, BackgroundArea.Height * 0.5f), 1f, SpriteEffects.None, 0f);

                Vector2 inputTexPos = controllerTexturePosition;
                for (int i = 0; i < Input.MAX_USERS; i++)
                {
                    if (Input.Users[i].InputType == InputType.KEYBOARD)
                        inputTexture = keyboardTexture;
                    else
                        inputTexture = controllerTexture;

                    if (Input.CurrentGamePadState[i].IsConnected)
                        inputTextureColor = Color.Green;
                    else
                        inputTextureColor = Color.Red;

                    SpriteBatch.Draw(inputTexture, inputTexPos, controllerTexture.Bounds, inputTextureColor * TransitionAlpha,
                        0f, Vector2.Zero, 0.2f, SpriteEffects.None, 0f);
                    inputTexPos.X += (int)(controllerTexture.Width * 0.2f + Padding.X);
                }

                Utils.PulseColor(ref TextColor);
                SpriteBatch.DrawString(Font, Text, TextPosition, TextColor * TransitionAlpha,
                    0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            base.Draw(gameTime);
        }
    }
}