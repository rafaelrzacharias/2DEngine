using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
        public GameScreen(string screenName)
        {
            Name = screenName;
            Font = Resources.GetFont("gameFont");
            texture = Resources.GetTexture("gameBackground");
            EnabledGestures = GestureType.Tap;

            pauseMenu = ScreenManager.GetScreen("pauseMenu") as PauseMenuScreen;
            pauseMenu.Dismiss += PauseMenu_OnDismiss;

            OnShow();
        }


        // Event handler for when the "Pause Menu" is dismissed.
        private void PauseMenu_OnDismiss(User user)
        {
            OnShow();
        }


        // Lets the game respond to player input. Unlike the Update method,
        // this will only be called when the gameplay screen is active.
        public override void HandleInput()
        {
            if (Input.IsActionPressed(Action.START, PrimaryUser))
                OnDismiss(PrimaryUser);

            if (Input.IsActionPressed(Action.LK, PrimaryUser))
                Audio.PlaySong("song", true, 0.1f);

            if (Input.IsActionPressed(Action.HK, PrimaryUser))
                Audio.PauseOrResumeSong();

            if (Input.IsActionPressed(Action.LB, PrimaryUser))
                Audio.StopSong();

            if (Input.IsActionPressed(Action.RB, PrimaryUser))
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

        public override void OnDismiss(User user)
        {
            base.OnDismiss(user);
            pauseMenu.OnShow();
        }
    }
}