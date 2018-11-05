using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameStateManager
{
    public class IISMessageBoxScreen : MessageBoxScreen
    {
        // Constructs an initialInteraction screen.
        public IISMessageBoxScreen(string message) :
            base(message)
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
                MainMenuScreen mainMenu = new MainMenuScreen("Main Menu");
                mainMenu.Name = nameof(mainMenu);
                OptionsMenuScreen optionsMenu = new OptionsMenuScreen("Options Menu");
                optionsMenu.Name = nameof(optionsMenu);
                MessageBoxScreen confirmQuit = new MessageBoxScreen("Are you sure you want to quit?");
                confirmQuit.Name = nameof(confirmQuit);

                Screen[] screens = new Screen[] { mainMenu, optionsMenu, confirmQuit };
                LoadingScreen.Load(screens);
                mainMenu.Setup();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Utils.PulseColor(ref TextColor);
            base.Draw(gameTime);
        }
    }
}
