using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GameStateManager
{
    // DebugSystem is a helper class that streamlines the creation of the various GameDebug
    // pieces. While games are free to add only the pieces they care about, DebugSystem allows
    // games to quickly create and add all the components by calling the Initialize method.
    public static class Debug
    {
        private static bool isInitialized;
        private static List<DebugScreen> debugSystems;

        public static Texture2D Texture { get; private set; }
        public static SpriteFont Font { get; private set; }
        public static Color AreaColor { get; private set; }

        public static Profiler Profiler { get; private set; }
        public static Console Console { get; private set; }
        public static FpsCounter FpsCounter { get; private set; }
        public static ScreenStats ScreenStats { get; private set; }
        public static AudioStats AudioStats { get; private set; }
        public static InputStats InputStats { get; private set; }
        public static GraphicsStats GraphicsStats { get; private set; }


        // Initializes the Debug and its sub-systems.
        public static void Initialize()
        {
            if (isInitialized == false)
            {
                Texture = Resources.GetTexture("whiteTexture");
                Font = Resources.GetFont("debugFont");
                AreaColor = new Color(0, 0, 0, 200);

                Profiler = new Profiler();
                Console = new Console();
                FpsCounter = new FpsCounter();
                ScreenStats = new ScreenStats();
                AudioStats = new AudioStats();
                InputStats = new InputStats();
                GraphicsStats = new GraphicsStats();

                debugSystems = new List<DebugScreen>();
                debugSystems.Add(ScreenStats);
                debugSystems.Add(AudioStats);
                debugSystems.Add(GraphicsStats);
                debugSystems.Add(InputStats);
                debugSystems.Add(Profiler);
                debugSystems.Add(FpsCounter);
                debugSystems.Add(Console);

                for (int i = debugSystems.Count - 1; i >= 0 ; i--)
                    debugSystems[i].Initialize();

                isInitialized = true;
            }
        }


        // Updates all Debug sub-systems.
        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < debugSystems.Count; i++)
            {
                if (debugSystems[i].IsActive)
                    debugSystems[i].Update(gameTime);
            }
        }

        // Draw all Debug sub-systems.
        public static void Draw(GameTime gameTime)
        {
            for (int i = 0; i < debugSystems.Count; i++)
            {
                if (debugSystems[i].IsActive)
                    debugSystems[i].Draw(gameTime);
            }
        }
    }
}