using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameStateManager
{
    // The main menu screen is the first thing displayed when the game starts up.
    public class MainMenuScreen : MenuScreen
    {
        private OptionsMenuScreen optionsMenu;
        private MessageBoxScreen confirmQuit;

        // Constructs a main menu screen.
        public MainMenuScreen(string menuTitle)
            : base(menuTitle)
        {
            IsRootMenu = true;

            // Create our menu entries.
            MenuEntry playGameEntry = new MenuEntry("Play Game");
            MenuEntry optionsEntry = new MenuEntry("Options");
            MenuEntry exitEntry = new MenuEntry("Exit");

            // Add the menu event handlers.
            playGameEntry.Selected += PlayGameEntry_OnSelected;
            optionsEntry.Selected += OptionsEntry_OnSelected;
            exitEntry.Selected += ExitEntry_OnSelected;

            // Add the entries to the menu.
            Entries.Add(playGameEntry);
            Entries.Add(optionsEntry);
            Entries.Add(exitEntry);

            OnShow();
        }

        public void Setup()
        {
            optionsMenu = ScreenManager.GetScreen("optionsMenu") as OptionsMenuScreen;
            optionsMenu.Hide += OptionsMenu_OnHide;

            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries.Add(new MenuEntry("Yes"));
            confirmQuit.Entries.Add(new MenuEntry("No"));
            confirmQuit.Entries[0].Selected += MessageBoxScreen_Yes;
            confirmQuit.Entries[1].Selected += MessageBoxScreen_No;
        }

        // Event handler for when the "Yes" entry is selected on the Message Box.
        private void MessageBoxScreen_Yes(User user)
        {
            ScreenManager.Game.Exit();
        }


        // Event handler for when the "No" entry is selected on the Message Box.
        private void MessageBoxScreen_No(User user)
        {
            IsEnabled = true;
            confirmQuit.OnHide();
        }


        // Event handler for when the "Play Game" entry is selected.
        private void PlayGameEntry_OnSelected(User user)
        {
            Screen[] screens = new Screen[] { ScreenManager.GetScreen("mainMenuBackground"),
                ScreenManager.GetScreen("mainMenu"), ScreenManager.GetScreen("optionsMenu"),
            ScreenManager.GetScreen("pressAnyKey")};
            LoadingScreen.Unload(screens);

            GameScreen game = new GameScreen();
            game.Name = nameof(game);
            PauseMenuScreen pauseMenu = new PauseMenuScreen("Game Paused");
            pauseMenu.Name = nameof(pauseMenu);
            screens = new Screen[] { game, pauseMenu };
            LoadingScreen.Load(screens, true);

            game.Setup();
        }


        // Event handler for when the "Options" entry is selected.
        private void OptionsEntry_OnSelected(User user)
        {
            OnHide();
            optionsMenu.OnShow();
        }


        // Event handler for when the "Exit" entry is selected. A popup message is displayed.
        private void ExitEntry_OnSelected(User user)
        {
            OnDismiss();
        }


        public override void OnDismiss()
        {
            base.OnDismiss();

            IsEnabled = false;
            confirmQuit.OnShow();
        }


        // Callback for when the "Options Menu" is dismissed.
        private void OptionsMenu_OnHide()
        {
            OnShow();
        }
    }
}