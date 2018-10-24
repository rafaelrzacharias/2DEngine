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
            MenuEntries.Add(playGameEntry);
            MenuEntries.Add(optionsEntry);
            MenuEntries.Add(exitEntry);

            backgroundScreen = new BackgroundScreen();

            optionsMenu = new OptionsMenuScreen("Options");
            optionsMenu.Hide += OptionsMenu_OnHide;

            messageBox = new MessageBoxScreen("Are you sure you want to exit?");
            messageBox.Accept += MessageBox_OnAccept;
            messageBox.Reject += MessageBox_OnReject;

            OnShow();
        }


        // Event handler for when the "Play Game" entry is selected.
        private void PlayGameEntry_OnSelected(PlayerIndex playerIndex)
        {
            List<string> screens = new List<string> { "GameScreen", "PauseScreen", "MessageScreen" };
            LoadingScreen.Load(true, screens);
        }


        // Event handler for when the "Options" entry is selected.
        private void OptionsEntry_OnSelected(PlayerIndex playerIndex)
        {
            OnHide();
            optionsMenu.OnShow();
        }


        // Event handler for when the "Exit" entry is selected. A popup message is displayed.
        private void ExitEntry_OnSelected(PlayerIndex playerIndex)
        {
            OnDismiss();
        }


        public override void OnDismiss()
        {
            base.OnDismiss();

            IsEnabled = false;
            messageBox.OnShow();
        }


        // Event handler for when the message box is accepted.
        private void MessageBox_OnAccept(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }


        // Event handler for when the message box gets dismissed.
        private void MessageBox_OnReject(PlayerIndex playerIndex)
        {
            IsEnabled = true;
        }


        // Callback for when the "Options Menu" is dismissed.
        private void OptionsMenu_OnHide()
        {
            OnShow();
        }
    }
}