﻿using System;
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
        protected int highlightedEntry;
        private int previousHighlightedEntry;
        private Vector2 titlePosition;
        private Vector2 titleOrigin;
        private bool isMenuUp;
        private bool isMenuDown;
        protected string MenuTitle;

        // Gets the list of menu entries, so derived classes can add or change the menu contents.
        public List<MenuEntry> Entries { get; private set; }

        // Constructs a menu screen.
        public MenuScreen(string menuTitle)
        {
            MenuTitle = menuTitle;
            Entries = new List<MenuEntry>();
            EnabledGestures = GestureType.Tap;
            Font = Resources.GetFont("menuFont");
            titlePosition = new Vector2(ScreenManager.Viewport.Width * 0.5f, 
                ScreenManager.Viewport.Height * 0.1f);
            titleOrigin = Font.MeasureString(MenuTitle) / 2f;
            DrawOrder = 0.1f;
#if MOBILE
            highlightedEntry = -1;
#endif
        }


        private void UpdateEntriesHighlight()
        {
            isMenuUp = Input.GetTimedAction(Action.UP, PrimaryUser);
            isMenuDown = Input.GetTimedAction(Action.DOWN, PrimaryUser);

#if DESKTOP || MOBILE
            if (PrimaryUser != null /*&& PrimaryUser.Type == ControllerType.KEYBOARD*/ && Input.HasMouseMoved())
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Input.IsMouseOver(Entries[i].Bounds))
                    {
                        previousHighlightedEntry = i;
                        highlightedEntry = i;
                        break;
                    }

                    highlightedEntry = -1;
                }
            }
#endif
            if (isMenuUp)
            {
                if (highlightedEntry == -1)
                    highlightedEntry = previousHighlightedEntry;
                else
                    highlightedEntry--;

                if (highlightedEntry < 0)
                    highlightedEntry = Entries.Count - 1;

                previousHighlightedEntry = highlightedEntry;
            }

            if (isMenuDown)
            {
                if (highlightedEntry == -1)
                    highlightedEntry = previousHighlightedEntry;
                else
                    highlightedEntry++;

                if (highlightedEntry >= Entries.Count)
                    highlightedEntry = 0;

                previousHighlightedEntry = highlightedEntry;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (i == highlightedEntry)
                    Entries[i].OnBeginHighlighted();
                else
                    Entries[i].OnEndHighlighted();
            }
        }


        private void UpdateEntriesSelected()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].IsSelected = false;

                if (Entries[i].IsHighlighted && Input.GetAction(Action.UI_SELECT, PrimaryUser).IsTriggered)
                {
                    Entries[i].OnSelected(PrimaryUser);
                    //Audio.PlaySound("entrySelected");
                    break;
                }
            }

            if (Input.GetAction(Action.UI_BACK, PrimaryUser).IsTriggered)
            {
                OnDismiss(PrimaryUser);
                //Audio.PlaySound("menuDismissed");
            }
#if MOBILE
            for (int i = 0; i < Input.Gestures.Length; i++)
            {
                if (Input.Gestures[i].GestureType == GestureType.Tap)
                {
                    for (int j = 0; j < Entries.Count; j++)
                    {
                        // Since gestures are only available on Mobile, we can safely pass Index 0
                        // to all entries since there will be only one player on Mobile.
                        if (Entries[j].Bounds.Contains(Input.Gestures[i].Position))
                        {
                            Entries[j].OnSelected(PrimaryUser);
                            //Audio.PlaySound("menuSelect");
                        }
                    }
                }
            }
#endif
        }

        // Responds to user input, changing the selected entry and accepting or cancelling the menu.
        public override void HandleInput(GameTime gameTime)
        {
            if (PrimaryUser != null)
                PrimaryUser.Update(gameTime);

            UpdateEntriesHighlight();
            UpdateEntriesSelected();
        }


        // Allows the screen the chance to position the menu entries. By default,
        // all menu entries are lined up in a vertical list, centered on the screen.
        protected virtual void UpdateMenuEntries()
        {
            // Slides the menu into place during transitions, using a power curve to make
            // things look more interesting (the movement slows down near the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2.0);

            for (int i = 0; i < Entries.Count; i++)
            {
                // Center each entry horizontally and move down to the next entry.
                Entries[i].Position.X = ScreenManager.Viewport.Width * 0.5f;
                Entries[i].Position.Y = ScreenManager.Viewport.Height * 0.2f + (i * MenuEntry.VerticalPadding);
                
                if (TransitionState == ScreenState.TransitionOn)
                    Entries[i].Position.X -= transitionOffset * 256f;
                else
                    Entries[i].Position.X += transitionOffset * 512f;

                Entries[i].UpdateBounds();
            }
        }


        // Updates the menu screen.
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsEnabled)
            {
                // Make sure our entries are in the right place before we draw them.
                UpdateMenuEntries();

                // Update each nested MenuEntry object.
                for (int i = 0; i < Entries.Count; i++)
                    Entries[i].Update(this, gameTime);
            }
        }


        // Draws the menu entry.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                for (int i = 0; i < Entries.Count; i++)
                    Entries[i].Draw(this, gameTime);

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