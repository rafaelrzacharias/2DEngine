﻿namespace GameStateManager
{
    // The main menu screen is the first thing displayed when the game starts up.
    public class MainMenuScreen : MenuScreen
    {
        private OptionsMenuScreen optionsMenu;
        private MessageBoxScreen confirmQuit;

        // Constructs a main menu screen.
        public MainMenuScreen(string screenName, string menuTitle)
            : base(menuTitle)
        {
            Name = screenName;
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

            optionsMenu = ScreenManager.GetScreen("optionsMenu") as OptionsMenuScreen;
            optionsMenu.Hide += OptionsMenu_OnHide;

            OnShow();
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
            OnHide();
            LoadingScreen.Unload(ScreenManager.GetScreen("mainMenuBackground"));
            LoadingScreen.Unload(ScreenManager.GetScreen("optionsMenu"));
            LoadingScreen.Unload(this);

            LoadingScreen.Load(new PauseMenuScreen("pauseMenu", "Paused"));
            LoadingScreen.Load(new GameScreen("gameScreen"), true);
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
            OnDismiss(user);
        }


        public override void OnShow()
        {
            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries[0].Selected += MessageBoxScreen_Yes;
            confirmQuit.Entries[1].Selected += MessageBoxScreen_No;
            confirmQuit.Dismiss += MessageBoxScreen_No;

            base.OnShow();
        }


        public override void OnHide()
        {
            confirmQuit = ScreenManager.GetScreen("confirmQuit") as MessageBoxScreen;
            confirmQuit.Entries[0].Selected -= MessageBoxScreen_Yes;
            confirmQuit.Entries[1].Selected -= MessageBoxScreen_No;
            confirmQuit.Dismiss -= MessageBoxScreen_No;

            base.OnHide();
        }


        public override void OnDismiss(User user)
        {
            base.OnDismiss(user);

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