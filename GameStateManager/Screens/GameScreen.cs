using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace GameStateManager
{
    // This screen implements the actual game logic. It is just a placeholder to get the
    // idea across. You'll probably want to put some more interesting gameplay in here!
    public class GameScreen : Screen
    {
        private readonly Texture2D texture;
        private PauseMenuScreen pauseMenu;

        // Constructs a gameplay screen.
        public GameScreen()
        {
            Font = Resources.GetFont("gameFont");
            texture = Resources.GetTexture("gameBackground");
            EnabledGestures = GestureType.Tap;

            pauseMenu = new PauseMenuScreen("Pause Menu");
            pauseMenu.Dismiss += PauseMenu_OnDismiss;

            OnShow();
        }


        // Event handler for when the "Pause Menu" is dismissed.
        private void PauseMenu_OnDismiss()
        {
            OnShow();
        }


        // Lets the game respond to player input. Unlike the Update method,
        // this will only be called when the gameplay screen is sactive.
        public override void HandleInput()
        {
            // The game pauses either if the user presses the pause button, or if they unplug the active gamepad.
            // This requires us to keep track of whether a gamepad was ever plugged in, because we don't want
            // to pause on PC if they are playing with a keyboard and have no gamepad at all!
            // Specify further, how controller disconnection should display message and how pausing the game is handled!!!
            if (Input.WasGamePaused(out PlayerIndex playerIndex, PrimaryUser) /*|| Input.IsGamePadConnected[(int)ControllingPlayer.Value] == false*/)
            {
                OnDismiss();
            }

            if (Input.WasKeyPressed(Keys.S, out playerIndex, PrimaryUser))
                Audio.PlaySong("song", true, 0.1f);

            if (Input.WasKeyPressed(Keys.D, out playerIndex, PrimaryUser))
                Audio.PauseOrResumeSong();

            if (Input.WasKeyPressed(Keys.F, out playerIndex, PrimaryUser))
                Audio.StopSong();

            if (Input.WasKeyPressed(Keys.Q, out playerIndex, PrimaryUser))
            {
                foreach (KeyValuePair<string, SoundEffect> pair in Resources.SoundEffects)
                    Audio.PlaySound(pair.Key, new AudioEmitter(), false);
            }
        }


        // Draw the gameplay screen.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
                SpriteBatch.Draw(texture, ScreenManager.Viewport.Bounds, Color.White);

            base.Draw(gameTime);
        }

        public override void OnDismiss()
        {
            base.OnDismiss();
            pauseMenu.OnShow();
        }
    }
}