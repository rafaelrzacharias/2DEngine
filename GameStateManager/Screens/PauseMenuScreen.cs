using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameStateManager
{
    // The pause menu comes up over the top of the game,
    // giving the player options to resume or quit.
    public class PauseMenuScreen : MenuScreen
    {
        private readonly Texture2D texture;
        private MessageBoxScreen confirmQuit;

        // Constructs the pause menu screen.
        public PauseMenuScreen(string menuTitle)
            : base(menuTitle)
        {
            texture = Resources.GetTexture("pauseBackground");

            MenuEntry resumeGameEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameEntry = new MenuEntry("Quit Game");

            resumeGameEntry.Selected += ResumeGameEntry_OnSelected;
            quitGameEntry.Selected += QuitGameEntry_OnSelected;

            Entries.Add(resumeGameEntry);
            Entries.Add(quitGameEntry);

            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries[0].Selected += MessageBox_Yes;
            confirmQuit.Entries[1].Selected += MessageBox_No;
        }


        // Event handler for when the "No" entry is selected on the Message Box.
        private void MessageBox_No(User user)
        {
            IsEnabled = true;
            confirmQuit.OnHide();
        }


        // Event handler for when the "Quit Game" entry is selected.
        private void QuitGameEntry_OnSelected(User user)
        {
            IsEnabled = false;
            confirmQuit.OnShow();
        }


        // Event handler for when the "Yes" entry is selected on the Message Box.
        // It uses the loading screen to transition from the game back to the main menu.
        private void MessageBox_Yes(User user)
        {
            confirmQuit.OnHide();
            OnHide();

            MainMenuScreen mainMenu = new MainMenuScreen("Main Menu");
            Screen[] screens = new Screen[] { mainMenu };
            LoadingScreen.Load(screens);
        }


        // Callback for when the "Resume Game" entry is selected.
        private void ResumeGameEntry_OnSelected(User user)
        {
            OnDismiss();
        }


        // Overriden OnHide callback to resume all sounds before exiting the screen.
        public override void OnHide()
        {
            base.OnHide();
            Audio.PauseOrResumeAllSounds();
            Audio.PauseOrResumeSong();
        }


        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
                SpriteBatch.Draw(texture, ScreenManager.Viewport.Bounds, Color.White);

            base.Draw(gameTime);
        }
    }
}