﻿using Microsoft.Xna.Framework;

namespace GameStateManager
{
    // The loading screen coordinates transitions between the menu system and the game itself.
    // Normally one screen will transition off at the same time as the next screen is transitioning on,
    // but for larger transitions that can take a longer time to load their data, we want the menu
    // system to be entirely gone before we start loading the game.
    public class LoadingScreen : Screen
    {
        private readonly bool isLoadingSlow;
        private readonly string loadingText;
        private Vector2 loadingTextSize;
        private Vector2 textPosition;
        public static int PreviousScreensCount;


        // The constructor is private. Loading screens are activated via the static Load method instead.
        private LoadingScreen(Screen screen, bool isLoadingSlow)
        {
            this.isLoadingSlow = isLoadingSlow;
            ScreenManager.AddScreen(screen);

            Font = Resources.GetFont("gameFont");
            loadingText = "Loading...";
            loadingTextSize = Font.MeasureString(loadingText);
            textPosition = (new Vector2(ScreenManager.Viewport.Width,
                ScreenManager.Viewport.Height) - loadingTextSize) / 2f;

            OnShow();
        }


        // Activates the loading screen.
        public static void Load(Screen screenToLoad, bool isLoadingSlow = false)
        {
            LoadingScreen loadingScreen = new LoadingScreen(screenToLoad, isLoadingSlow);
        }


        // Activates the loading screen and transition off a screen.
        public static void Unload(Screen screenToUnload)
        {
            screenToUnload.ExitScreen();
        }


        // Update the loading screen.
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If all previous screens have finished transitioning off, perform the load.
            if (PreviousScreensCount == 0)
            {
                ScreenManager.RemoveScreen(this);

                // Once the load has finished, we use ResetElapsedTime to tell the game timing mechanism
                // that we have just finished a very long frame, and that it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        // Draws the loading screen.
        public override void Draw(GameTime gameTime)
        {
            // The gameplay screen takes a while to load, but menu screens load quickly. Display a loading message
            // Itt doesn't look good if we flash this up for just a fraction of a second, if it's not really needed.
            // This parameter tells us how long the loading will take, so we know whether to bother drawing the message.
            if (isLoadingSlow)
                SpriteBatch.DrawString(Font, loadingText, textPosition, BackgroundColor * TransitionAlpha);

            base.Draw(gameTime);
        }
    }
}