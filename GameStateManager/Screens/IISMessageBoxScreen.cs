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
        

        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
;
            int controllerIndex = Input.WasAnyButtonPressed(true, true);

            if (controllerIndex != -1)
            {
                Input.SetPrimaryUser(Input.Controllers[0]);
                Input.SetUserControllerType(Input.Controllers[0], controllerIndex);

                OnHide();

#if DESKTOP || CONSOLE
                LoadingScreen.Load(new ControllerDisconnectionScreen("controllerDisconnection"));
#endif
                LoadingScreen.Load(new BackgroundScreen("mainMenuBackground"));
                LoadingScreen.Load(new MessageBoxScreen("confirmQuit", "Are you sure you want to quit?", "", MessageBoxType.YESNO));
                LoadingScreen.Load(new UnassignedInputMessage("unassignedInputMessage", "There are still unassigned inputs.", ""));
                LoadingScreen.Load(new SaveInputMapMessageBoxScreen("saveInputMap", "Would you like to save?", ""));
                LoadingScreen.Load(new OptionsMenuScreen("optionsMenu", "Options"));
                LoadingScreen.Load(new BufferedInputScreen("bufferedInputMenu"));
                LoadingScreen.Load(new InputMappingScreen("inputMappingMenu", "Input Mapping"));
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
