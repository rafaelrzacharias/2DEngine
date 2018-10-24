using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Threading;

namespace GameStateManager
{
    public class InstructionsScreen : GameScreen
    {
        Texture2D background;
        SpriteFont font;
        bool isLoading;
        GameplayScreen gameplayScreen;
        Thread thread;
        const string LOADING_TEXT = "Loading...";
        Vector2 loadingTextSize;

        public InstructionsScreen()
        {
            EnabledGestures = GestureType.Tap;

            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            background = content.Load<Texture2D>(@"Textures\Backgrounds\instructions");
            font = content.Load<SpriteFont>(@"Fonts\MenuFont");

            loadingTextSize = font.MeasureString(LOADING_TEXT);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If additional thread is running, skip
            if (thread != null)
            {
                // If additional thread finished loading and the screen is not exiting
                if (thread.ThreadState == ThreadState.Stopped && !IsExiting)
                {
                    isLoading = false;

                    // Exit the screen and show the gameplay screen with pre-loaded assets
                    ExitScreen();
                    ScreenManager.AddScreen(gameplayScreen, null);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(Input input)
        {
            if (isLoading)
            {
                base.HandleInput(input);
                return;
            }

            PlayerIndex player;

            if (input.WasKeyPressed(Keys.Space, ControllingPlayer, out player) ||
                input.WasKeyPressed(Keys.Enter, ControllingPlayer, out player) ||
                input.MouseGesture.HasFlag(MouseGestureType.LeftClick) ||
                input.WasButtonPressed(Buttons.Start, ControllingPlayer, out player))
            {
                // Create a new instance of the gameplay screen
                gameplayScreen = new GameplayScreen();
                gameplayScreen.ScreenManager = ScreenManager;

                // Start loading the resources in additional thread
                thread = new Thread(new ThreadStart(gameplayScreen.LoadAssets));

                isLoading = true;
                thread.Start();
            }

            for (int i = 0; i < input.Gestures.Count; i++)
            {
                if (input.Gestures[i].GestureType == GestureType.Tap)
                {
                    // Create a new instance of the gameplay screen
                    gameplayScreen = new GameplayScreen();
                    gameplayScreen.ScreenManager = ScreenManager;

                    // Start loading the resources in additional thread
                    thread = new Thread(new ThreadStart(gameplayScreen.LoadAssets));
                    isLoading = true;
                    thread.Start();
                }
            }

            base.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw the background
            spriteBatch.Draw(background, new Vector2(0f, 0f), 
                new Color(255, 255, 255, TransitionAlpha));

            // If loading the gameplay screen, display "Loading..." text
            if (isLoading)
            {
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 position = new Vector2(
                    (viewport.Width - loadingTextSize.X) / 2f, (viewport.Height - loadingTextSize.Y) / 2f);
                spriteBatch.DrawString(font, LOADING_TEXT, position, Color.Black);
                spriteBatch.DrawString(font, LOADING_TEXT, position - new Vector2(-4, 4), new Color(255f, 150f, 0f));
            }

            spriteBatch.End();
        }
    }
}