using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManager
{
    // A popup message box screen, used to display confirmation messages.
    public class MessageBoxScreen : Screen
    {
        private readonly string message;
        private readonly Texture2D texture;

        public delegate void AcceptedEventHandler(PlayerIndex playerIndex);
        public AcceptedEventHandler Accept;

        public virtual void OnAccept(PlayerIndex playerIndex)
        {
            OnHide();

            if (Accept != null)
                Accept.Invoke(playerIndex);
        }


        public delegate void RejectedEventHandler(PlayerIndex playerIndex);
        public RejectedEventHandler Reject;

        public virtual void OnReject(PlayerIndex playerIndex)
        {
            OnDismiss();

            if (Reject != null)
                Reject.Invoke(playerIndex);
        }


        // Constructs a message box in which the caller specifies the prompt.
        public MessageBoxScreen(string message = "", bool includeUsageText = true)
            : base()
        {
            Color = new Color(0, 0, 0, 128);
            Font = Resources.GetFont("menuFont");
            texture = Resources.GetTexture("whiteTexture");
            EnabledGestures = GestureType.Tap;
            DrawOrder = 0.2f;

            if (includeUsageText)
                this.message = message + "\nA button, Space = Ok\nB button, Esc = Cancel";
            else
                this.message = message;
        }


        // Responds to user input, accepting or cancelling the message box.
        public override void HandleInput()
        {

            // We pass in our ControllingPlayer, which may be null (to accept input from any player) or a
            // specific index. If null, the InputState helper returns which player provided the input. We pass
            // that through to our Accepted and Cancelled events, so they can tell which player triggered them.
            if (Input.WasMenuSelected(ControllingPlayer, out PlayerIndex playerIndex))
                OnAccept(playerIndex);
            else if (Input.WasMenuCancelled(ControllingPlayer, out playerIndex))
                OnReject(playerIndex);
        }


        // Draws the message box.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                // Darken all other screens that were drawn underneath the popup.
                FadeScreen(TransitionAlpha * 0.66f);

                // Center the message text in the viewport.
                Vector2 viewportSize = new Vector2(ScreenManager.Viewport.Width, ScreenManager.Viewport.Height);
                Vector2 textSize = Font.MeasureString(message);
                Vector2 textPosition = (viewportSize - textSize) / 2f;

                // The background includes a border larger than the text itself.
                const int horizontalPad = 32;
                const int verticalPad = 16;

                Rectangle backgroundRectangle = new Rectangle(
                    (int)textPosition.X - horizontalPad, (int)textPosition.Y - verticalPad,
                    (int)textSize.X + horizontalPad * 2, (int)textSize.Y + verticalPad * 2);

                ScreenManager.SpriteBatch.Draw(texture, backgroundRectangle, Color * TransitionAlpha);
                ScreenManager.SpriteBatch.DrawString(Font, message, textPosition, Color.White * TransitionAlpha);
            }
        }
    }
}