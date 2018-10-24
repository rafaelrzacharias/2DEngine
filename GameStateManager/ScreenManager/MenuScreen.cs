using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManager
{
    // Base class for screens that contain a menu of options. The user can
    // move up and down to select an entry, or cancel to back out of the screen.
    public abstract class MenuScreen : Screen
    {
        // The number of pixels to pad above and below menu entries (for touch input).
        private const int ENTRY_PADDING = 70; // This should be specified by the UI.Button, and not here!!!
        private int selectedEntry;
        private Vector2 titlePosition;
        private Vector2 titleOrigin;
        private string MenuTitle;
        private bool isInitialized;

        // Gets the list of menu entries, so derived classes can add or change the menu contents.
        protected List<MenuEntry> MenuEntries { get; private set; }

        // Constructs a menu screen.
        public MenuScreen(string menuTitle)
        {
            MenuTitle = menuTitle;
            MenuEntries = new List<MenuEntry>();
            EnabledGestures = GestureType.Tap;
            Font = Resources.GetFont("menuFont");
            titlePosition = new Vector2(ScreenManager.Viewport.Width / 2f, 80f);
            titleOrigin = Font.MeasureString(MenuTitle) / 2f;
            DrawOrder = 0.1f;
        }

        private void UpdateEntriesHighlight()
        {
            for (int i = 0; i < MenuEntries.Count; i++)
                MenuEntries[i].IsHighlighted = (i == selectedEntry);
        }

        // Responds to user input, changing the selected entry and accepting or cancelling the menu.
        public override void HandleInput()
        {
            if (isInitialized == false)
            {
                MenuEntries[0].IsHighlighted = true;
                isInitialized = true;
            }

            // Move to previous entry, or wrap around.
            if (Input.WasMenuUp(ControllingPlayer))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = MenuEntries.Count - 1;

                UpdateEntriesHighlight();
                //Audio.PlaySound("menuScroll");
            }

            // Move to next entry, or wrap around.
            if (Input.WasMenuDown(ControllingPlayer))
            {
                selectedEntry++;

                if (selectedEntry >= MenuEntries.Count)
                    selectedEntry = 0;

                UpdateEntriesHighlight();
                //Audio.PlaySound("menuScroll");
            }

            if (Input.WasMenuSelected(ControllingPlayer, out PlayerIndex playerIndex))
            {
                MenuEntries[selectedEntry].OnSelected(playerIndex);
                //Audio.PlaySound("menuSelect");
            }
            else if (Input.WasMenuCancelled(ControllingPlayer, out playerIndex))
            {
                OnDismiss();
                //Audio.PlaySound("menuCancel");
            }

            // Look for any taps that occurred and select any entries that were tapped.
            for (int i = 0; i < Input.Gestures.Count; i++)
            {
                if (Input.Gestures[i].GestureType == GestureType.Tap)
                {
                    for (int j = 0; j < MenuEntries.Count; j++)
                    {
                        // Since gestures are only available on Mobile, we can safely pass PlayerIndex.One
                        // to all entries since there will be only one player on Mobile.
                        if (MenuEntries[j].Bounds.Contains(Input.Gestures[i].Position))
                        {
                            MenuEntries[j].OnSelected(PlayerIndex.One);
                            //Audio.PlaySound("menuSelect");
                        }
                    }
                }
            }
        }


        // Allows the screen the chance to position the menu entries. By default,
        // all menu entries are lined up in a vertical list, centered on the screen.
        protected virtual void UpdateMenuEntryLocations()
        {
            // Slides the menu into place during transitions, using a power curve to make
            // things look more interesting (the movement slows down near the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2.0);

            // Start at Y = 175. Each X value is generated per entry.
            Vector2 position = new Vector2(0f, 175f);

            for (int i = 0; i < MenuEntries.Count; i++)
            {
                // Center each entry horizontally.
                position.X = ScreenManager.Viewport.Width / 2f - MenuEntries[i].Width / 2f;

                if (TransitionState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256f;
                else
                    position.X += transitionOffset * 512f;

                // Set the entry's position and move down to the next entry.
                MenuEntries[i].Position = position;
                position.Y += MenuEntries[i].Height + ENTRY_PADDING;
            }
        }


        // Updates the menu screen.
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsEnabled)
            {
                // Make sure our entries are in the right place before we draw them.
                UpdateMenuEntryLocations();

                // Update each nested MenuEntry object.
                for (int i = 0; i < MenuEntries.Count; i++)
                    MenuEntries[i].Update(this, gameTime);
            }
        }


        // Draws the menu entry.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                for (int i = 0; i < MenuEntries.Count; i++)
                    MenuEntries[i].Draw(this, gameTime);

                // Slides the menu into place during transitions, using a power curve to make
                // things look more interesting (the movement slows down near the end).
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2.0);

                // Draw the menu title centered on the screen.  
                Color titleColor = new Color(192, 192, 192) * TransitionAlpha;

                SpriteBatch.DrawString(Font, MenuTitle, titlePosition, titleColor, 0f,
                    titleOrigin, 1.25f, SpriteEffects.None, DrawOrder);
            }

            base.Draw(gameTime);
        }
    }
}