using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GameStateManager
{
    public struct Label
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
        private Vector2[] keyLabels;
        private Vector2[] buttonLabels;
        private bool anyEntrySelected;
        private List<ActionMap[]> actionMaps;
        private MessageBoxScreen saveInputMap;
        private MessageBoxScreen unassignedInputMessage;
        private Label actionsLabel;
        private Label keyboardLabel;
        private Label gamePadLabel;
        private const int NUM_SKIPPED_KEYS = 7;

        // Constructs a new InputMapping screen.
        public InputMappingScreen(string screenName, string menuTitle)
            : base(menuTitle)
        {
            Name = screenName;
            DrawOrder = 0.1f;
            Font = Resources.GetFont("menuFont");
            gamePadTexture = Resources.GetTexture("gamePad");

            Viewport viewport = ScreenManager.Viewport;
            gamePadPosition = new Vector2(viewport.Width * 0.6f, viewport.Height * 0.45f);
            gamePadOrigin = new Vector2(gamePadTexture.Width * 0.5f, gamePadTexture.Height * 0.5f);

            actionMaps = new List<ActionMap[]>(Input.MAX_USERS);
            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                actionMaps.Add(new ActionMap[Input.Actions.Length]);
                Input.ResetActionMaps(actionMaps[i]);
            }

            actionsLabel = new Label();
            actionsLabel.Text = "Actions";
            actionsLabel.Position = new Vector2(viewport.Width * 0.1f, viewport.Height * 0.3f);

            float longestWord = 0f;
            float width = Font.MeasureString(actionsLabel.Text).X;

            if (width > longestWord)
                longestWord = width;

            // Ignore the NONE, START, DEBUG, CONSOLE, and all 4 directional actions.
            for (int i = NUM_SKIPPED_KEYS; i < Input.ActionNames.Length - 3; i++)
            {
                Entries.Add(new MenuEntry(Input.ActionNames[i]));
                Entries[i - NUM_SKIPPED_KEYS].Position.X = actionsLabel.Position.X;
                Entries[i - NUM_SKIPPED_KEYS].Position.Y += (actionsLabel.Position.Y + (i + 1 - NUM_SKIPPED_KEYS) * Font.LineSpacing * 1.2f);
                Entries[i - NUM_SKIPPED_KEYS].Origin = Vector2.Zero;
                Entries[i - NUM_SKIPPED_KEYS].UpdateBounds();

                width = Font.MeasureString(Input.ActionNames[i]).X;

                if (width > longestWord)
                    longestWord = width;
            }

            keyLabels = new Vector2[Entries.Count];
            float offset = longestWord * 1.5f;

            keyboardLabel = new Label();
            keyboardLabel.Text = "Keyboard";
            keyboardLabel.Position = new Vector2(actionsLabel.Position.X + offset, actionsLabel.Position.Y);

            int userIndex = GetUserIndex(PrimaryUser);

            longestWord = 0;
            width = Font.MeasureString(keyboardLabel.Text).X;

            if (width > longestWord)
                longestWord = width;

            for (int i = 0; i < Entries.Count; i++)
            {
                keyLabels[i] = Entries[i].Position;
                keyLabels[i].X += offset;

                width = Font.MeasureString(actionMaps[userIndex][i + NUM_SKIPPED_KEYS].Keys[0].ToString()).X;

                if (width > longestWord)
                    longestWord = width;
            }

            offset = longestWord * 1.2f;

            gamePadLabel = new Label();
            gamePadLabel.Text = "GamePad";
            gamePadLabel.Position = new Vector2(keyboardLabel.Position.X + offset, actionsLabel.Position.Y);
            offset *= 1.4f;

            buttonLabels = new Vector2[Entries.Count];

            for (int i = 0; i < Entries.Count; i++)
            {
                buttonLabels[i] = keyLabels[i];
                buttonLabels[i].X += offset;
                buttonLabels[i].Y += Font.LineSpacing * 0.5f;
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
                    Input.CanSwapControllerType = false;

                    for (int j = NUM_SKIPPED_KEYS; j < Input.Actions.Length - 3; j++)
                    {
                        ActionMap currentActionMap = actionMaps[userIndex][i + NUM_SKIPPED_KEYS];

                        if (Input.GetAction(Input.Actions[j], PrimaryUser).IsTriggered)
                        {
                            switch (PrimaryUser.Type)
                            {
                                case ControllerType.KEYBOARD:
                                    {
                                        Keys newKey = PrimaryUser.ActionMaps[j].Keys[0];
                                        Keys oldKey = currentActionMap.Keys[0];

                                        if (oldKey == newKey)
                                            break;

                                        currentActionMap.Keys[0] = newKey;

                                        for (int k = NUM_SKIPPED_KEYS; k < actionMaps[userIndex].Length - 3; k++)
                                        {
                                            if (k - NUM_SKIPPED_KEYS != i && actionMaps[userIndex][k].Keys[0] == newKey)
                                                actionMaps[userIndex][k].Keys[0] = 0;
                                        }
                                    }
                                    break;
                                case ControllerType.GAMEPAD:
                                    {
                                        Buttons newButton = PrimaryUser.ActionMaps[j].Buttons[0];
                                        Buttons oldButton = currentActionMap.Buttons[0];

                                        if (oldButton == newButton)
                                            break;

                                        currentActionMap.Buttons[0] = newButton;

                                        for (int k = NUM_SKIPPED_KEYS; k < actionMaps[userIndex].Length - 3; k++)
                                        {
                                            if (k - NUM_SKIPPED_KEYS != i && actionMaps[userIndex][k].Buttons[0] == newButton)
                                                actionMaps[userIndex][k].Buttons[0] = 0;
                                        }
                                    }
                                    break;
                            }

                            Entries[i].OnDeselected(PrimaryUser);
                            Input.CanSwapControllerType = true;
                        }

                    }

                    anyEntrySelected = true;
                }
            }

            base.Update(gameTime);
        }


        // Overrides the default implementation of UpdateMenuEntries.
        protected override void UpdateMenuEntries() { }


        // Overrides the default implementation of HandleInputs
        public override void HandleInput(GameTime gameTime)
        {
            if (anyEntrySelected == false)
                base.HandleInput(gameTime);

            anyEntrySelected = false;
        }


        // Draws the InputMapping screen.
        public override void Draw(GameTime gameTime)
        {
            if (IsVisible)
            {
                int userIndex = GetUserIndex(PrimaryUser);
                Color keyboardLabelColor = Color.White;
                Color gamePadLabelColor = Color.White;

                SpriteBatch.DrawString(Font, actionsLabel.Text, actionsLabel.Position, Color.Yellow);
                SpriteBatch.DrawString(Font, keyboardLabel.Text, keyboardLabel.Position, Color.Yellow);
                SpriteBatch.DrawString(Font, gamePadLabel.Text, gamePadLabel.Position, Color.Yellow);

                for (int i = 0; i < Entries.Count; i++)
                {
                    switch (PrimaryUser.Type)
                    {
                        case ControllerType.KEYBOARD:
                            {
                                if (Entries[i].IsSelected)
                                {
                                    keyboardLabelColor = Color.Yellow;
                                    Utils.PulseColor(ref keyboardLabelColor);
                                }
                                else
                                    keyboardLabelColor = Color.White;
                            }
                            break;
                        case ControllerType.GAMEPAD:
                            {
                                if (Entries[i].IsSelected)
                                {
                                    gamePadLabelColor = Color.Yellow;
                                    Utils.PulseColor(ref gamePadLabelColor);
                                }
                                else
                                    gamePadLabelColor = Color.White;
                            }
                            break;
                    }

                    string text = string.Empty;
                    ActionMap actions = actionMaps[userIndex][i + NUM_SKIPPED_KEYS];

                    if (actions.Keys[0] == 0)
                        text = "--";
                    else
                        text = actions.Keys[0].ToString();

                    SpriteBatch.DrawString(Font, text, keyLabels[i], keyboardLabelColor);

                    Texture2D buttonTexture = Input.GetPlatformButton(actions.Buttons[0]);
                    Vector2 origin = new Vector2(buttonTexture.Width * 0.5f, buttonTexture.Height * 0.5f);

                    SpriteBatch.Draw(buttonTexture, buttonLabels[i], buttonTexture.Bounds, gamePadLabelColor, 0f, origin, 0.3f, SpriteEffects.None, 0f);
                }

                SpriteBatch.Draw(gamePadTexture, gamePadPosition, gamePadTexture.Bounds,
                    Color.White, 0f, gamePadOrigin, 0.5f, SpriteEffects.None, DrawOrder);
            }

            base.Draw(gameTime);
        }


        // Overrides the default implementation of OnDismiss.
        public override void OnDismiss(Controller controller)
        {
            int userIndex = GetUserIndex(controller);
            bool hasInputChanged = false;
            bool unassignedInput = false;

            if (userIndex != -1)
            {
                for (int i = NUM_SKIPPED_KEYS; i < actionMaps[userIndex].Length - 3; i++)
                {
                    ActionMap actions = actionMaps[userIndex][i];
                    Keys assignedKey = actions.Keys[0];
                    Buttons assignedButton = actions.Buttons[0];

                    if (assignedKey != controller.ActionMaps[i].Keys[0] ||
                        assignedButton != controller.ActionMaps[i].Buttons[0])
                    {
                        if (assignedKey == 0 || assignedButton == 0)
                        {
                            unassignedInput = true;
                            break;
                        }

                        hasInputChanged = true;
                    }
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
                base.OnDismiss(controller);
        }


        // Event handler for when the "Yes" entry is selected in the new input message box.
        private void SaveInputMapMessageBoxScreen_Yes(Controller controller)
        {
            int userIndex = GetUserIndex(controller);

            if (userIndex != -1)
                Input.SetActionMaps(controller, actionMaps[userIndex]);

            saveInputMap.OnHide();
            OnHide();
        }


        // Event handler for when the "No" entry is selected in the new input message box.
        private void SaveInputMapMessageBoxScreen_No(Controller controller)
        {
            int userIndex = GetUserIndex(controller);

            if (userIndex != -1)
                Input.ResetActionMaps(actionMaps[userIndex]);

            saveInputMap.OnHide();
            OnHide();
        }


        // Event handler for when the "Ok" entry is selected in the unassigned input message box.
        private void UnassignedInputMessage_Ok(Controller controller)
        {
            unassignedInputMessage.OnHide();
            IsEnabled = true;
        }


        // Callback for the InputMappingScreen to know when the UnassignedInputMessage
        // or the SaveInputMapMessageBoxScreen were dismissed.
        private void OnMessageBoxDismiss(Controller controller)
        {
            IsEnabled = true;
        }


        // Helper function to find the user index in Input.Users, given a user.
        private int GetUserIndex(Controller controller)
        {
            for (int i = 0; i < Input.MAX_USERS; i++)
            {
                if (Input.Controllers[i] == controller)
                    return i;
            }

            return -1;
        }
    }
}