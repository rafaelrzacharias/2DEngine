using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace GameStateManager
{
    // The screen manager is a component which manages one or more GameScreen instances. It maintains
    // a stack of screens, calls their Update and Draw methods at the appropriate times, and automatically
    // routes input to the topmost active screen.
    public static class ScreenManager
    {
        private static List<Screen> screensToUpdate;
        private static List<Screen> screensToRemove;

        //public static Gamer SignedInPlayer { get; private set; };
        public static SpriteBatch SpriteBatch { get; private set; }
        public static List<Screen> Screens { get; private set; }
        public static Viewport Viewport { get; private set; }
        public static Rectangle TitleSafeArea { get; private set; }
        public static Game Game { get; private set; }


        // Initializes the screen manager component.
        public static void Initialize(Game game)
        {
            Game = game;
            Screens = new List<Screen>();
            screensToUpdate = new List<Screen>();
            screensToRemove = new List<Screen>();

            // We must set EnabledGestures before we can query for them,
            // but we don't assume the game wants to read them.
            TouchPanel.EnabledGestures = GestureType.None;

            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Game.Services.AddService(SpriteBatch);

            Viewport = Game.GraphicsDevice.Viewport;
            TitleSafeArea = Viewport.TitleSafeArea;
        }


        private static int CompareDrawOrder(Screen a, Screen b)
        {
            return a.DrawOrder.CompareTo(b.DrawOrder);
        }


        // Allows each screen to run logic.
        public static void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            Screens.Sort(CompareDrawOrder);

            for (int i = 0; i < Screens.Count; i++)
                screensToUpdate.Add(Screens[i]);

            bool otherScreenHasFocus = (Game.IsActive == false);

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                Screen screen = screensToUpdate[screensToUpdate.Count - 1];
                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                screen.Update(gameTime);

                if (screen.TransitionState == ScreenState.TransitionOn || 
                    screen.TransitionState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (otherScreenHasFocus == false)
                    {
                        if (screen.IsTransitioning == false && Console.State == State.CLOSED)
                            screen.HandleInput();

                        otherScreenHasFocus = true;
                    }
                }
            }
        }

        public static void TransitionOffPreviousScreens()
        {
            // Tell all the current screens to transition off.
            for (int i = 0; i < Screens.Count; i++)
                Screens[i].ExitScreen();
        }


        // Tells each screen to draw itself.
        public static void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin();

            for (int i = 0; i < Screens.Count; i++)
            {
                if (Screens[i].TransitionState == ScreenState.Hidden)
                    continue;

                Screens[i].Draw(gameTime);
            }

            SpriteBatch.End();
        }


        // Adds a new screen to the screen manager.
        public static void AddScreen(Screen screen)
        {
            screen.IsExiting = false;
            Screens.Add(screen);

            // Update the TouchPanel to respond to gestures this screen is interested in.
            TouchPanel.EnabledGestures = screen.EnabledGestures;
        }


        // Removes a screen from the screen manager. Use Screen.ExitScreen instead of calling this directly,
        // so the screen can gradually transition off rather than just being instantly removed.
        public static void RemoveScreen(Screen screen)
        {
            Screens.Remove(screen);
            screensToUpdate.Remove(screen);
            LoadingScreen.PreviousScreensCount--;

            // If there is a screen still in the manager, update TouchPanel
            // to respond to gestures that screen is interested in.
            if (Screens.Count > 0)
                TouchPanel.EnabledGestures = Screens[Screens.Count - 1].EnabledGestures;
        }
    }
}