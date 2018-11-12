using Microsoft.Xna.Framework;

namespace GameStateManager
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false,
                PreferredBackBufferWidth = 1000,
                PreferredBackBufferHeight = 700
            };

            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }
       

        protected override void Initialize()
        {
            Resources.Initialize(Content);
            Input.Initialize();
            Audio.Initialize();
            ScreenManager.Initialize(this);
            Debug.Initialize();

            base.Initialize();

            LoadingScreen.Load(new ControllerDisconnectionScreen("controllerDisconnection"));
            ScreenManager.GetScreen("controllerDisconnection").OnShow();
            LoadingScreen.Load(new IISMessageBoxScreen("pressAnyKey", "Press any key to start"));
            LoadingScreen.Load(new BackgroundScreen("mainMenuBackground"));
        }


        protected override void Update(GameTime gameTime)
        {
            Debug.Update(gameTime);

            Debug.Profiler.StartFrame();

            Debug.Profiler.BeginMark("InputUpdate", Color.Yellow);
            Input.Update();
            Debug.Profiler.EndMark("InputUpdate");

            Debug.Profiler.BeginMark("AudioUpdate", Color.Red);
            Audio.Update(gameTime);
            Audio.UpdateListener();
            Debug.Profiler.EndMark("AudioUpdate");

            Debug.Profiler.BeginMark("ScreenManagerUpdate", Color.Violet);
            ScreenManager.Update(gameTime);
            Debug.Profiler.EndMark("ScreenManagerUpdate");

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            Debug.Profiler.BeginMark("ScreenManagerDraw", Color.Green);
            ScreenManager.Draw(gameTime);
            Debug.Profiler.EndMark("ScreenManagerDraw");

            Debug.Profiler.BeginMark("DebugDraw", Color.Green);
            Debug.Draw(gameTime);
            Debug.Profiler.EndMark("DebugDraw");

            Debug.Profiler.BeginMark("baseDraw", Color.Green);
            base.Draw(gameTime);
            Debug.Profiler.EndMark("baseDraw");
        }
    }
}