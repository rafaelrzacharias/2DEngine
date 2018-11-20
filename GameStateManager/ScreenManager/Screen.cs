using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace GameStateManager
{
    // Describes the screen transition state.
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden
    }


    // A screen is a single layer that has update and draw logic, and which can be combined
    // with other layers to build up a complex menu system. The Main menu, the options menu,
    // the message box, and the main game itself are all implemented as screens.
    public abstract class Screen
    {
        private float pauseAlpha;

        // A spriteBatch that targets the ScreenManager spriteBatch.
        protected SpriteBatch SpriteBatch;

        // Indicates how long the screen takes to transition on when it is activated.
        public TimeSpan TransitionOnTime { get; protected set; }

        // Indicates how long the screen takes to transition off when it is deactivated.
        public TimeSpan TransitionOffTime { get; protected set; }

        // Gets the current position of the screen transition, ranging from zero
        // (fully active, no transition) to one (transitioned fully off to nothing).
        public float TransitionPosition { get; protected set; }

        // Gets the current alpha of the screen transition, ranging from 1
        // (fully active, no transition) to 0 (transitioned fully off to nothing).
        public float TransitionAlpha { get { return 1f - TransitionPosition; } }

        // Gets the current screen transition state.
        public ScreenState TransitionState { get; protected set; }

        // The sprite font that the screen uses.
        public SpriteFont Font { get; protected set; }

        // Get the current screen color for drawing.
        public Color BackgroundColor { get; protected set; }

        // There are two reasons why a screen might be transitioning off. It could be temporarily
        // going away to make room for another screen that is on top of it, or it could be going
        // away for good. This property indicates whether the screen is exiting for real: if set,
        // the screen will automatically remove itself as soon as the transition finishes.
        public bool IsExiting { get; set; }

        // Checks whether or not the screen should be drawn.
        public bool IsVisible { get; set; }

        // Checks whether or no the screen should be updated.
        public bool IsEnabled { get; set; }

        // The name of the screen.
        public string Name { get; set; }

        // Gets the index of the player who is currently controlling this screen, or null if it is
        // accepting input from any player. Used to lock the game to a specific player profile.
        // The main menu responds to input from any connected gamepad, but whichever player makes a
        // selection from this menu is given control over all subsequent screens, so other gamepads
        // are inactive until the controlling player returns to the main menu.
        protected User ControllingUser;
        public User PrimaryUser
        {
            get { return ControllingUser != null ? ControllingUser : Input.GetPrimaryUser(); }
            set { ControllingUser = value; }
        }

        // Determines which order the screen is going to get drawn.
        public float DrawOrder;

        // Determines if a screen is a root menu.
        public bool IsRootMenu;

        // Gets the gestures the screen is interested in. Screens should be as specific as possible with gestures
        // to increase the accuracy of the gesture engine. For example, most menus only need Tap or perhaps Tap
        // and VerticalDrag to operate. These gestures are handled by the ScreenManager when screens change
        // and all gestures are placed in the InputState passed to the HandleInput method.
        private GestureType enabledGestures = GestureType.None;
        public GestureType EnabledGestures
        {
            get { return enabledGestures; }

            protected set
            {
                enabledGestures = value;

                // The screen manager handles this during screen changes, but if this screen is active
                // and the gesture types are changing, we have to update the TouchPanel ourself.
                if (TransitionState == ScreenState.Active)
                    TouchPanel.EnabledGestures = value;
            }
        }

        private bool isTransitioningOn;
        private bool isTransitioningOff;

        public bool IsTransitioning { get { return isTransitioningOn || isTransitioningOff; } }


        // Callback for when the screen needs to show itself.
        public delegate void ShowCallback();
        public ShowCallback Show;

        public virtual void OnShow()
        {
            isTransitioningOn = true;

            if (Show != null)
                Show.Invoke();
        }


        // Callback for when the screen needs to hide itself.
        public delegate void HideCallback();
        public HideCallback Hide;

        public virtual void OnHide()
        {
            isTransitioningOff = true;

            if (Hide != null)
                Hide.Invoke();
        }


        // Callback for when the screen is dismissed.
        public delegate void DismissCallback(User user);
        public DismissCallback Dismiss;

        public virtual void OnDismiss(User user)
        {
            if (IsRootMenu == false)
                OnHide();

            if (Dismiss != null)
                Dismiss.Invoke(user);
        }


        // Constructs a new GameScreen and assign the default values.
        public Screen()
        {
            SpriteBatch = ScreenManager.SpriteBatch;
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            TransitionPosition = 1f;
            TransitionState = ScreenState.TransitionOn;
            Font = Resources.GetFont("gameFont");
            BackgroundColor = Color.White;

            isTransitioningOff = true;
        }


        // Allows the screen to run logic, such as updating the transition position.
        // Unlike HandleInput, this method is called regardless of whether the screen
        // is active, hidden, or in the middle of a transition.
        public virtual void Update(GameTime gameTime)
        {
            if (IsExiting)
            {
                // If the screen is going to die, it should transition off.
                TransitionState = ScreenState.TransitionOff;

                // When the transition finishes, remove this screen.
                if (UpdateTransition(gameTime, TransitionOffTime, 1) == false)
                    ScreenManager.RemoveScreen(this);
            }
            else if (isTransitioningOff)
            {
                // If the screen is covered by another, it should transition off.
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                    TransitionState = ScreenState.TransitionOff;
                else
                {
                    IsEnabled = false;
                    IsVisible = false;
                    isTransitioningOff = false;
                    TransitionState = ScreenState.Hidden;
                }
            }
            else if (isTransitioningOn)
            {
                // Otherwise the screen should transition on and become active.
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                {
                    IsEnabled = true;
                    IsVisible = true;
                    TransitionState = ScreenState.TransitionOn;
                }
                else
                {
                    isTransitioningOn = false;
                    TransitionState = ScreenState.Active;
                }
            }

            // Gradually fade in or out when covered by another screen.
            pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);
        }


        // Helper for updating the screen transition position.
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1f;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

            // Update the transition position.
            TransitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if ((direction < 0 && TransitionPosition <= 0) || (direction > 0 && TransitionPosition >= 1))
            {
                TransitionPosition = MathHelper.Clamp(TransitionPosition, 0f, 1f);
                return false;
            }

            // Otherwise, we are still busy transitioning.
            return true;
        }


        // // Allows the screen to handle user input. Unlike Update, this method is only called
        // when the screen is active, and not when some other screen has taken the focus.
        public virtual void HandleInput() { }


        // Screen-specific updte to player rich presence.
        public virtual void UpdatePresence() { }


        // This is called when the screen should draw itself.
        public virtual void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                // If the game is transitioning on or off, fade it out to black.
                if (TransitionPosition > 0 || pauseAlpha > 0)
                {
                    float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2f);
                    FadeScreen(alpha);
                }
            }
        }


        // Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which instantly kills the screen,
        // this method respects the transition timings and gives the screen a chance to slowly transition off.
        public void ExitScreen()
        {
            // If the screen has 0 transition time, remove it immediately.
            if (TransitionOffTime == TimeSpan.Zero)
                ScreenManager.RemoveScreen(this);
            else
                IsExiting = true;
        }


        // Helper draws a translucent black fullscreen sprite, used for fading
        // screens in and out, and for darkening the background behind popups.
        public void FadeScreen(float alpha)
        {
            ScreenManager.SpriteBatch.Draw(Resources.GetTexture("whiteTexture"),
                ScreenManager.Viewport.Bounds, Color.Black * alpha);
        }
    }
}