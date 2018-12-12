using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GameStateManager
{
    public struct ButtonLabel
    {
        public string Text;
        public Vector2 Position;
    }


    // A screen that displays a controller and all buttons for remapping.
    public class InputMappingScreen : MenuScreen
    {
        private Texture2D gamePadTexture;
        private Vector2 gamePadPosition;
        private Vector2 gamePadOrigin;
        private ButtonLabel[] buttonLabels;
        private bool anyEntrySelected;
        private List<ActionMap[]> actionMaps;
        private MessageBoxScreen saveInputMap;
        private MessageBoxScreen unassignedInputMessage;

        // Constructs a new InputMapping screen.
        public InputMappingScreen(string screenName, string menuTitle)
            : base(menuTitle)
        {
            Name = screenName;
            DrawOrder = 0.1f;
            Font = Resources.GetFont("menuFont");
            gamePadTexture = Resources.GetTexture("gamePad");

            Viewport viewport = ScreenManager.Viewport;
            gamePadPosition = new Vector2(viewport.Width * 0.6f, viewport.Height * 0.5f);
            gamePadOrigin = new Vector2(gamePadTexture.Width * 0.5f, gamePadTexture.Height * 0.5f);

            actionMaps = new List<ActionMap[]>(Input.MAX_USERS);
            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                actionMaps.Add(new ActionMap[Input.Actions.Length]);
                Input.ResetActionMaps(actionMaps[i]);
            }

            float longestWord = 0f;

            // Ignore the NONE, START, DEBUG, CONSOLE, and all 4 directional actions.
            for (int i = 5; i < Input.ActionNames.Length - 3; i++)
            {
                Entries.Add(new MenuEntry(Input.ActionNames[i]));
                Entries[i - 5].Position.X = viewport.Width * 0.1f;
                Entries[i - 5].Position.Y += (viewport.Height * 0.3f + (i - 5) * Font.LineSpacing * 1.2f);
                Entries[i - 5].Origin = Vector2.Zero;
                Entries[i - 5].UpdateBounds();
                //Entries[i - 5].Selected += InputMappingScreen_Selected;

                float width = Font.MeasureString(Input.ActionNames[i]).X;

                if (width > longestWord)
                    longestWord = width;
            }

            buttonLabels = new ButtonLabel[Entries.Count];
            float offset = longestWord * 1.5f;

            UpdateMenuEntries();

            for (int i = 0; i < Entries.Count; i++)
            {
                buttonLabels[i].Position = Entries[i].Position;
                buttonLabels[i].Position.X += offset;
            }

            saveInputMap = ScreenManager.GetScreen("saveInputMap") as MessageBoxScreen;
            saveInputMap.Entries[0].Selected += SaveInputMapMessageBoxScreen_Yes;
            saveInputMap.Entries[1].Selected += SaveInputMapMessageBoxScreen_No;
            saveInputMap.Dismiss += OnMessageBoxDismiss;

            unassignedInputMessage = ScreenManager.GetScreen("unassignedInputMessage") as MessageBoxScreen;
            unassignedInputMessage.Entries[0].Selected += UnassignedInputMessage_Ok;
            unassignedInputMessage.Dismiss += OnMessageBoxDismiss;
        }


        // Updates the InputMapping screen.
        public override void Update(GameTime gameTime)
        {
            int userIndex = GetUserIndex(PrimaryUser);

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].IsSelected)
                {
                    for (int j = 5; j < Input.Actions.Length - 3; j++)
                    {
                        ActionMap currentActionMap = actionMaps[userIndex][i + 5];

                        if (Input.IsActionPressed(Input.Actions[j], PrimaryUser, true))
                        {
                            switch (PrimaryUser.InputType)
                            {
                                case InputType.KEYBOARD:
                                    {
                                        Keys newKey = PrimaryUser.ActionMaps[j].Keys[0];
                                        Keys oldKey = currentActionMap.Keys[0];

                                        if (oldKey == newKey)
                                            break;

                                        string keyName = newKey.ToString();

                                        buttonLabels[i].Text = keyName;
                                        currentActionMap.Keys[0] = newKey;

                                        for (int k = 0; k < buttonLabels.Length; k++)
                                        {
                                            if (k != i && buttonLabels[k].Text == keyName)
                                            {
                                                buttonLabels[k].Text = "--";
                                                actionMaps[userIndex][k + 5].Keys[0] = 0;
                                            }
                                        }
                                    }
                                    break;
                                case InputType.GAMEPAD:
                                    {
                                        Buttons newButton = PrimaryUser.ActionMaps[j].Buttons[0];
                                        Buttons oldButton = currentActionMap.Buttons[0];

                                        if (oldButton == newButton)
                                            break;

                                        string buttonName = newButton.ToString();

                                        buttonLabels[i].Text = buttonName;
                                        currentActionMap.Buttons[0] = newButton;

                                        for (int k = 0; k < buttonLabels.Length; k++)
                                        {
                                            if (k != i && buttonLabels[k].Text == buttonName)
                                            {
                                                buttonLabels[k].Text = "--";
                                                actionMaps[userIndex][k + 5].Buttons[0] = 0;
                                            }
                                        }
                                    }
                                    break;
                            }

                            Entries[i].OnDeselected(PrimaryUser);
                        }

                    }

                    anyEntrySelected = true;
                }
            }

            base.Update(gameTime);
        }


        // Overrides the default implementation of HandleInputs
        public override void HandleInput()
        {
            if (anyEntrySelected == false)
                base.HandleInput();

            anyEntrySelected = false;
        }


        // Overrides the default positions of the menu entries.
        protected override void UpdateMenuEntries()
        {
            int userIndex = GetUserIndex(PrimaryUser);

            if (userIndex != -1)
            {
                string text = string.Empty;

                switch (PrimaryUser.InputType)
                {
                    case InputType.KEYBOARD:
                        {
                            for (int i = 0; i < Entries.Count; i++)
                            {
                                Keys key = actionMaps[userIndex][i + 5].Keys[0];

                                if (key != 0)
                                    text = key.ToString();
                                else
                                    text = "--";

                                // Allow only one key mapping for non-directional buttons.
                                buttonLabels[i].Text = text;
                            }
                        }
                        break;
                    case InputType.GAMEPAD:
                        {
                            for (int i = 0; i < Entries.Count; i++)
                            {
                                Buttons button = actionMaps[userIndex][i + 5].Buttons[0];

                                if (button != 0)
                                    text = button.ToString();
                                else
                                    text = "--";

                                // Allow only one button mapping for non-directional buttons.
                                buttonLabels[i].Text = text;
                            }
                        }
                        break;
                }
            }
        }


        // Draws the InputMapping screen.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                Color labelColor = Color.DarkSlateGray;
                
                for (int i = 0; i < buttonLabels.Length; i++)
                {
                    if (Entries[i].IsSelected)
                        Utils.PulseColor(ref labelColor);
                    else
                        labelColor = Color.DarkSlateGray;

                    SpriteBatch.DrawString(Font, buttonLabels[i].Text, buttonLabels[i].Position, labelColor);
                }

                SpriteBatch.Draw(gamePadTexture, gamePadPosition, gamePadTexture.Bounds,
                    Color.White, 0f, gamePadOrigin, 1f, SpriteEffects.None, DrawOrder);
            }

            base.Draw(gameTime);
        }


        // Overrides the default implementation of OnDismiss.
        public override void OnDismiss(User user)
        {
            int userIndex = GetUserIndex(user);
            bool hasInputChanged = false;
            bool unassignedInput = false;

            if (userIndex != -1)
            {
                switch (user.InputType)
                {
                    case InputType.KEYBOARD:
                        {
                            for (int i = 5; i < actionMaps[userIndex].Length - 3; i++)
                            {
                                Keys assignedKey = actionMaps[userIndex][i].Keys[0];

                                if (assignedKey != user.ActionMaps[i].Keys[0])
                                {
                                    if (assignedKey == 0)
                                    {
                                        unassignedInput = true;
                                        break;
                                    }

                                    hasInputChanged = true;
                                }
                            }
                        }
                        break;
                    case InputType.GAMEPAD:
                        {
                            for (int i = 5; i < actionMaps[userIndex].Length - 3; i++)
                            {
                                Buttons assignedButton = actionMaps[userIndex][i].Buttons[0];

                                if (assignedButton != user.ActionMaps[i].Buttons[0])
                                {
                                    if (assignedButton == 0)
                                    {
                                        unassignedInput = true;
                                        break;
                                    }

                                    hasInputChanged = true;
                                }
                            }
                        }
                        break;
                }
            }

            if (unassignedInput)
            {
                IsEnabled = false;
                unassignedInputMessage.OnShow();
            }
            else if (hasInputChanged)
            {
                IsEnabled = false;
                saveInputMap.OnShow();
            }
            else
                base.OnDismiss(user);
        }


        // Overrides the default implementation of OnHide enable controller hot-swap.
        public override void OnHide()
        {
            Input.CanSwapControllerType = true;
            base.OnHide();
        }


        // Overrides the default implementation of OnShow disable controller hot-swap.
        public override void OnShow()
        {
            Input.CanSwapControllerType = false;
            base.OnShow();
        }


        // Event handler for when the "Yes" entry is selected in the new input message box.
        private void SaveInputMapMessageBoxScreen_Yes(User user)
        {
            int userIndex = GetUserIndex(user);

            if (userIndex != -1)
                Input.SetActionMaps(user, actionMaps[userIndex]);

            saveInputMap.OnHide();
            OnHide();
        }


        // Event handler for when the "No" entry is selected in the new input message box.
        private void SaveInputMapMessageBoxScreen_No(User user)
        {
            int userIndex = GetUserIndex(user);

            if (userIndex != -1)
                Input.ResetActionMaps(actionMaps[userIndex]);

            saveInputMap.OnHide();
            OnHide();
        }


        // Event handler for when the "Ok" entry is selected in the unassigned input message box.
        private void UnassignedInputMessage_Ok(User user)
        {
            unassignedInputMessage.OnHide();
            IsEnabled = true;
        }


        // Callback for the InputMappingScreen to know when the UnassignedInputMessage
        // or the SaveInputMapMessageBoxScreen were dismissed.
        private void OnMessageBoxDismiss(User user)
        {
            IsEnabled = true;
        }


        // Helper function to find the user index in Input.Users, given a user.
        private int GetUserIndex(User user)
        {
            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                if (Input.Users[i] == user)
                    return i;
            }

            return -1;
        }
    }
}