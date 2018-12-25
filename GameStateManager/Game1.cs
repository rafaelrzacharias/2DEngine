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
            ResourcesList resources = Content.Load<ResourcesList>("resources");
            Resources.Initialize(Content, resources);
            Input.Initialize(this);
            Audio.Initialize();
            ScreenManager.Initialize(this);
            Debug.Initialize();

            base.Initialize();

            LoadingScreen.Load(new IISMessageBoxScreen("pressAnyKey", "Press any key to start"));
        }


        protected override void Update(GameTime gameTime)
        {
            Debug.Profiler.StartFrame();

            Debug.Update(gameTime);

            //Debug.Profiler.BeginMark("Update: Input", Color.Black);
            Input.Update(gameTime);
            //Debug.Profiler.EndMark("Update: Input");

            //Debug.Profiler.BeginMark("Update: Audio", Color.Red);
            Audio.Update(gameTime);
            Audio.UpdateListener();
            //Debug.Profiler.EndMark("Update: Audio");

            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.Draw(gameTime);
            Debug.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}