using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManager
{
    // The pause menu comes up over the top of the game,
    // giving the player options to resume or quit.
    public class PauseMenuScreen : MenuScreen
    {
        private readonly Texture2D texture;
        private MessageBoxScreen confirmQuit;

        // Constructs the pause menu screen.
        public PauseMenuScreen(string screenName, string menuTitle)
            : base(menuTitle)
        {
            Name = screenName;
            texture = Resources.GetTexture("pauseBackground");

            MenuEntry resumeGameEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameEntry = new MenuEntry("Quit Game");

            resumeGameEntry.Selected += ResumeGameEntry_OnSelected;
            quitGameEntry.Selected += QuitGameEntry_OnSelected;

            Entries.Add(resumeGameEntry);
            Entries.Add(quitGameEntry);
        }


        // Event handler for when the "No" entry is selected on the Message Box.
        private void MessageBox_No(Controller controller)
        {
            IsEnabled = true;
            confirmQuit.OnHide();
        }


        // Event handler for when the "Quit Game" entry is selected.
        private void QuitGameEntry_OnSelected(Controller controller)
        {
            IsEnabled = false;
            confirmQuit.OnShow();
        }


        // Event handler for when the "Yes" entry is selected on the Message Box.
        // It uses the loading screen to transition from the game back to the main menu.
        private void MessageBox_Yes(Controller controller)
        {
            confirmQuit.OnHide();
            OnHide();

            LoadingScreen.Unload(ScreenManager.GetScreen("gameScreen"));
            LoadingScreen.Unload(this);

            LoadingScreen.Load(new UnassignedInputMessage("unassignedInputMessage", "There are still unassigned inputs.", ""));
            LoadingScreen.Load(new SaveInputMapMessageBoxScreen("saveInputMap", "Would you like to save?", ""));
            LoadingScreen.Load(new BackgroundScreen("mainMenuBackground"));
            LoadingScreen.Load(new OptionsMenuScreen("optionsMenu", "Options"));
            LoadingScreen.Load(new BufferedInputScreen("bufferedInputMenu"));
            LoadingScreen.Load(new InputMappingScreen("inputMappingMenu", "Input Mapping"));
            LoadingScreen.Load(new MainMenuScreen("mainMenu", "Main Menu"));
        }


        // Callback for when the "Resume Game" entry is selected.
        private void ResumeGameEntry_OnSelected(Controller controller)
        {
            OnDismiss(controller);
        }


        // Overriden OnShow callback to resume all sounds.
        public override void OnShow()
        {
            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries[0].Selected += MessageBox_Yes;
            confirmQuit.Entries[1].Selected += MessageBox_No;
            confirmQuit.Dismiss += MessageBox_No;

            base.OnShow();
            Audio.PauseOrResumeAllSounds();
            Audio.PauseOrResumeSong();
#if DESKTOP || CONSOLE
            Input.PauseOrResumeControllersVibration();
#endif
        }


        // Overriden OnHide callback to resume all sounds before exiting the screen.
        public override void OnHide()
        {
            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries[0].Selected -= MessageBox_Yes;
            confirmQuit.Entries[1].Selected -= MessageBox_No;
            confirmQuit.Dismiss -= MessageBox_No;

            base.OnHide();
            Audio.PauseOrResumeAllSounds();
            Audio.PauseOrResumeSong();
#if DESKTOP || CONSOLE
            Input.PauseOrResumeControllersVibration();
#endif
        }


        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
                SpriteBatch.Draw(texture, ScreenManager.Viewport.Bounds, Color.White);

            base.Draw(gameTime);
        }
    }
}