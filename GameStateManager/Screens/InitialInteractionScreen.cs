using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStateManager
{
    public class InitialInteractionScreen : Screen
    {
        private readonly Texture2D texture;


        // Constructs an initialInteraction screen.
        InitialInteractionScreen()
        {
            Font = Resources.GetFont("gameFont");
            texture = Resources.GetTexture("gameBackground");
            EnabledGestures = GestureType.Tap;

            OnShow();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.WasAnyButtonPressed(out PlayerIndex playerIndex))
            {
                Input.ControllingPlayer = playerIndex;
                List<string> screens = new List<string> { "MainMenuScreen" };
                LoadingScreen.Load(false, screens);
            }
        }


        // Event hander for when a key is pressed.
         
    }
}
