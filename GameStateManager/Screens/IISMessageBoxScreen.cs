using System.Collections.Generic;

namespace GameStateManager
{
    public class IISMessageBoxScreen : MessageBoxScreen
    {
        // Constructs an initialInteraction screen.
        public IISMessageBoxScreen() :
            base("Press any key to start", false)
        {
            ShouldDarkenBackground = false;
            OnShow();
        }


        public override void HandleInput()
        {
            if (Input.WasAnyButtonPressed())
            {
                List<string> screens = new List<string> { "MainMenuScreen" };
                LoadingScreen.Load(false, screens);
            }
        }
    }
}
