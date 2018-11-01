using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameStateManager
{
    // The main menu screen is the first thing displayed when the game starts up.
    public class MainMenuScreen : MenuScreen
    {
        private readonly BackgroundScreen backgroundScreen;
        private OptionsMenuScreen optionsMenu;
        private MessageBoxScreen messageBox;

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

            backgroundScreen = new BackgroundScreen();

            optionsMenu = new OptionsMenuScreen("Options");
            optionsMenu.Hide += OptionsMenu_OnHide;

            messageBox = new MessageBoxScreen("Are you sure you want to exit?");
            messageBox.Entries.Add(new MenuEntry("Yes"));
            messageBox.Entries.Add(new MenuEntry("No"));
            messageBox.Entries[0].Selected += MessageBoxScreen_Yes;
            messageBox.Entries[1].Selected += MessageBoxScreen_No;

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
            messageBox.OnHide();
        }


        // Event handler for when the "Play Game" entry is selected.
        private void PlayGameEntry_OnSelected(User user)
        {
            List<string> screens = new List<string> { "GameScreen", "PauseScreen", "MessageScreen" };
            LoadingScreen.Load(true, screens);
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
            messageBox.OnShow();
        }


        // Callback for when the "Options Menu" is dismissed.
        private void OptionsMenu_OnHide()
        {
            OnShow();
        }
    }
}