using Microsoft.Xna.Framework;

namespace GameStateManager
{
    public class IISMessageBoxScreen : MessageBoxScreen
    {
        // Constructs an initialInteraction screen.
        public IISMessageBoxScreen(string screenName, string message) :
            base(screenName, message)
        {
            ShouldDarkenBackground = false;
            OnShow();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (Input.WasAnyButtonPressed())
            {
                OnHide();

                LoadingScreen.Unload(this);

                LoadingScreen.Load(new MessageBoxScreen("confirmQuit", "Are you sure you want to quit?", "", MessageBoxType.YESNO));
                LoadingScreen.Load(new OptionsMenuScreen("optionsMenu", "Options"));
                LoadingScreen.Load(new MainMenuScreen("mainMenu", "Main Menu"));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Utils.PulseColor(ref TextColor);
            base.Draw(gameTime);
        }
    }
}
