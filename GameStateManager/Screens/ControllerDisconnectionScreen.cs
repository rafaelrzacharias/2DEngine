using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

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
        private Texture2D noInputTexture;
        private Color inputTextureColor;
        private Vector2 controllerTexturePosition;
        private User temporaryUser;

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
            noInputTexture = Resources.GetTexture("noInputTexture");
            inputTextureColor = Color.White;
            temporaryUser = new User();
            EnabledGestures = GestureType.Tap;
            DrawOrder = 0.3f;
            ShouldDarkenBackground = true;
            Padding = new Vector2(16, 16);

            PanelPosition = new Vector2(ScreenManager.Viewport.Width * 0.5f, ScreenManager.Viewport.Height * 0.5f);

            BackgroundArea = Rectangle.Empty;
            BackgroundArea.Width = (int)((controllerTexture.Width * 0.25f + Padding.X) * Input.MAX_USERS);
            BackgroundArea.Height = (int)(controllerTexture.Height * 0.2f + 2f * Font.LineSpacing + 3f * Padding.Y);
            BackgroundArea.X = (int)(PanelPosition.X - BackgroundArea.Width * 0.5f);
            BackgroundArea.Y = (int)(PanelPosition.Y - BackgroundArea.Height * 0.5f);
            
            SetText("Controller disconnected: Connect a controller.", Color.Yellow);

            controllerTexturePosition.X = PanelPosition.X - (Input.MAX_USERS - 1) * Padding.X * 0.5f - 
                (Input.MAX_USERS - 1) * 0.5f * controllerTexture.Width * 0.2f - controllerTexture.Width * 0.2f * 0.5f;
            controllerTexturePosition.Y = TextPosition.Y + Font.LineSpacing + Padding.Y;

            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                Entries.Add(new MenuEntry("Player " + (i + 1).ToString()));
                Entries[i].Position.X += controllerTexturePosition.X + controllerTexture.Width * 0.2f * 0.5f + i * (controllerTexture.Width * 0.2f + Padding.X);
                Entries[i].Position.Y = controllerTexturePosition.Y + controllerTexture.Height * 0.2f + Padding.Y;

                Entries[i].UpdateBounds();
                Entries[i].Selected += ControllerDisconnectionScreen_Selected;
            }

            Input.ControllerConnected += Input_ControllerConnected;
            Input.ControllerDisconnected += Input_ControllerDisconnected;
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
                    switch (Input.Users[i].InputType)
                    {
                        case InputType.NONE:
                            inputTexture = noInputTexture;
                            break;
                        case InputType.KEYBOARD:
                            inputTexture = keyboardTexture;
                            break;
                        case InputType.GAMEPAD:
                            inputTexture = controllerTexture;
                            break;
                        case InputType.TOUCH:
                            break;
                    }

                    if (Input.Users[i].IsActive)
                        inputTextureColor = Color.White;
                    else
                        inputTextureColor = BackgroundColor;

                    SpriteBatch.Draw(inputTexture, inputTexPos, controllerTexture.Bounds, inputTextureColor * TransitionAlpha,
                        0f, Vector2.Zero, 0.2f, SpriteEffects.None, 0f);
                    inputTexPos.X += (int)(controllerTexture.Width * 0.2f + Padding.X);
                }

                SpriteBatch.DrawString(Font, Text, TextPosition, TextColor * TransitionAlpha,
                    0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            base.Draw(gameTime);
        }


        // Event handler for when a controller is disconnected.
        private void Input_ControllerDisconnected(int controllerIndex)
        {
            SetText("Controller disconnected: Connect a controller.", Color.Yellow);

            if (IsVisible == false && Input.GetPrimaryUser() != null)
                OnShow();

            if (IsVisible)
                ControllingUser = Input.GetPrimaryUser();
        }

        // Event handler for when a controller is connected.
        private void Input_ControllerConnected(int controllerIndex)
        {
            if (IsVisible || Input.GetPrimaryUser() != null)
            {
                SetText("Controller connected: Choose a slot.", Color.Green);

                ControllingUser = temporaryUser;
                ControllingUser.ControllerIndex = controllerIndex;
                ControllingUser.InputType = InputType.GAMEPAD;

                if (IsVisible == false)
                    OnShow();
            }
        }


        private void ControllerDisconnectionScreen_Selected(User user)
        {
            Input.SetUserControllerType(Input.Users[highlightedEntry], user.ControllerIndex);

            if (Input.GetUserCount() == 1)
                Input.SetPrimaryUser(Input.Users[highlightedEntry]);

            OnHide();
        }

        private void SetText(string text, Color color)
        {
            Text = text;
            TextSize = Font.MeasureString(Text);
            TextPosition = new Vector2(PanelPosition.X - TextSize.X * 0.5f, BackgroundArea.Top + Padding.Y);
            TextColor = color;
        }


        public override void OnHide()
        {
            ControllingUser = null;
            Audio.PauseOrResumeAllSounds();
            Audio.PauseOrResumeSong();
            base.OnHide();
        }


        public override void OnShow()
        {
            Input.CanSwapControllerType = false;
            Audio.PauseOrResumeAllSounds();
            Audio.PauseOrResumeSong();
            base.OnShow();
        }


        protected override void OnFinishTransitionOff()
        {
            Input.CanSwapControllerType = true;
        }
    }
}