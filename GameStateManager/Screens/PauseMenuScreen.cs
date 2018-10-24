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
        private MessageBoxScreen messageBox;

        // Constructs the pause menu screen.
        public PauseMenuScreen(string menuTitle)
            : base(menuTitle)
        {
            texture = Resources.GetTexture("pauseBackground");

            MenuEntry resumeGameEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameEntry = new MenuEntry("Quit Game");

            resumeGameEntry.Selected += ResumeGameEntry_OnSelected;
            quitGameEntry.Selected += QuitGameEntry_OnSelected;

            MenuEntries.Add(resumeGameEntry);
            MenuEntries.Add(quitGameEntry);

            messageBox = new MessageBoxScreen("Are you sure you want to quit?");
            messageBox.Accept += MessageBox_OnAccept;
            messageBox.Dismiss += MessageBox_OnDismiss;
        }

        private void MessageBox_OnDismiss()
        {
            IsEnabled = true;
        }


        // Event handler for when the "Quit Game" entry is selected.
        private void QuitGameEntry_OnSelected(PlayerIndex playerIndex)
        {
            IsEnabled = false;
            messageBox.OnShow();
        }


        // Event handler for when the quit popup is displayed. This uses the loading
        // screen to transition from the game back to the main menu screen.
        private void MessageBox_OnAccept(PlayerIndex playerIndex)
        {
            messageBox.OnHide();
            OnHide();

            List<string> screens = new List<string> { "MainMenuScreen" };
            LoadingScreen.Load(false, screens);
        }


        // Callback for when the "Resume Game" entry is selected.
        private void ResumeGameEntry_OnSelected(PlayerIndex playerIndex)
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
                ScreenManager.SpriteBatch.Draw(texture, ScreenManager.Viewport.Bounds, Color.White);

            base.Draw(gameTime);
        }
    }
}