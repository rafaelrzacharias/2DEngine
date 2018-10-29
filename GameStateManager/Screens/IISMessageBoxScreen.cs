﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameStateManager
{
    public class IISMessageBoxScreen : MessageBoxScreen
    {
        // Constructs an initialInteraction screen.
        public IISMessageBoxScreen() :
            base("Press any key to start")
        {
            ShouldDarkenBackground = false;
            OnShow();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (Input.WasAnyButtonPressed())
            {
                List<string> screens = new List<string> { "MainMenuScreen" };
                LoadingScreen.Load(false, screens);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Utils.PulseColor(ref TextColor);
            base.Draw(gameTime);
        }
    }
}
