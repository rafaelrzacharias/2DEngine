using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace GameStateManager
{
    public enum ControllerType
    {
        NONE,
        KEYBOARD,
        GAMEPAD,
        TOUCH,
    }


    // Represent a controller. It can be aassigned to a player or an AI agent.
    public class Controller
    {
        public int Slot;
        public bool IsPrimaryUser;
        public ControllerType Type;
        public ActionMap[] ActionMaps;
        public ActionState[] ActionStates;

        public bool IsActive { get { return Slot != -1; } }


        // Constructs a new Controller object.
        public Controller()
        {
            Slot = -1;
            IsPrimaryUser = false;
            Type = ControllerType.NONE;
            ActionMaps = new ActionMap[Input.Actions.Length];
            Input.ResetActionMaps(ActionMaps);

            ActionStates = new ActionState[Input.Actions.Length];
            for (int i = 0; i < ActionStates.Length; i++)
                ActionStates[i] = new ActionState();
        }


        // Updates the state of each assigned, connected controller.
        // If the controller is unassigned, but connected, also checks it for input.
        public void Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;

            switch (Type)
            {
                case ControllerType.KEYBOARD:
                    {
                        List<Keys> keys;
                        List<MouseButtons> mouseButtons;

                        for (int i = 0; i < ActionMaps.Length; i++)
                        {
                            keys = ActionMaps[i].Keys;
                            if (CheckKeys(keys, ref ActionStates[i], elapsedTime))
                                continue;

                            mouseButtons = ActionMaps[i].MouseButtons;
                            if (CheckMouseButtons(mouseButtons, ref ActionStates[i], elapsedTime))
                                continue;

                            ActionStates[i].Reset();
                        }
                    }
                    break;
                case ControllerType.GAMEPAD:
                    {
                        List<Buttons> buttons;

                        for (int i = 0; i < ActionMaps.Length; i++)
                        {
                            buttons = ActionMaps[i].Buttons;
                            if (CheckButtons(buttons, ref ActionStates[i], elapsedTime))
                                continue;

                            ActionStates[i].Reset();
                        }
                    }
                    break;
                case ControllerType.TOUCH:
                    {
                        // Not implemented yet!!!
                    }
                    break;
            }
        }


        private bool CheckKeys(List<Keys> keys, ref ActionState actionState, double elapsedTime)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Keys key = keys[i];

                if (Input.CurrentKeyboardState.IsKeyDown(key))
                {
                    actionState.IsPressed = true;
                    actionState.IsTriggered = Input.LastKeyboardState.IsKeyUp(key);
                    actionState.Magnitude = 1.0f;
                    actionState.Duration += elapsedTime;
                    return true;
                }
            }

            return false;
        }


        private bool CheckMouseButtons(List<MouseButtons> mouseButtons, ref ActionState actionState, double elapsedTime)
        {
            for (int i = 0; i < mouseButtons.Count; i++)
            {
                switch (mouseButtons[i])
                {
                    case MouseButtons.LEFT:
                        {
                            if (Input.CurrentMouseState.LeftButton == ButtonState.Pressed)
                            {
                                actionState.IsPressed = true;
                                actionState.IsTriggered = Input.LastMouseState.LeftButton == ButtonState.Released;
                                actionState.Magnitude = 1.0f;
                                actionState.Duration += elapsedTime;
                                return true;
                            }
                        }
                        break;
                    case MouseButtons.RIGHT:
                        {
                            if (Input.CurrentMouseState.RightButton == ButtonState.Pressed)
                            {
                                actionState.IsPressed = true;
                                actionState.IsTriggered = Input.LastMouseState.RightButton == ButtonState.Released;
                                actionState.Magnitude = 1.0f;
                                actionState.Duration += elapsedTime;
                                return true;
                            }
                        }
                        break;
                    case MouseButtons.MIDDLE:
                        {
                            if (Input.CurrentMouseState.MiddleButton == ButtonState.Pressed)
                            {
                                actionState.IsPressed = true;
                                actionState.IsTriggered = Input.LastMouseState.MiddleButton == ButtonState.Released;
                                actionState.Magnitude = 1.0f;
                                actionState.Duration += elapsedTime;
                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }


        private bool CheckButtons(List<Buttons> buttons, ref ActionState actionState, double elapsedTime)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                Buttons button = buttons[i];

                if (Input.CurrentGamePadState[Slot].IsButtonDown(button))
                {
                    actionState.IsPressed = true;
                    actionState.IsTriggered = Input.LastGamePadState[Slot].IsButtonUp(button);
                    actionState.Magnitude = GetMagnitude(button);
                    actionState.Duration += elapsedTime;
                    return true;
                }
            }

            return false;
        }


        // Returns the magnitude of an analog button.
        private float GetMagnitude(Buttons button)
        {
            switch (button)
            {
                case Buttons.LeftTrigger:
                    return Input.CurrentGamePadState[Slot].Triggers.Left;
                case Buttons.RightTrigger:
                    return Input.CurrentGamePadState[Slot].Triggers.Right;
                case Buttons.LeftThumbstickUp:
                    return Input.CurrentGamePadState[Slot].ThumbSticks.Left.Y;
                case Buttons.LeftThumbstickDown:
                    return -1 * Input.CurrentGamePadState[Slot].ThumbSticks.Left.Y;
                case Buttons.LeftThumbstickLeft:
                    return -1 * Input.CurrentGamePadState[Slot].ThumbSticks.Left.X;
                case Buttons.LeftThumbstickRight:
                    return Input.CurrentGamePadState[Slot].ThumbSticks.Left.X;
                case Buttons.RightThumbstickUp:
                    return Input.CurrentGamePadState[Slot].ThumbSticks.Right.Y;
                case Buttons.RightThumbstickDown:
                    return -1 * Input.CurrentGamePadState[Slot].ThumbSticks.Right.Y;
                case Buttons.RightThumbstickLeft:
                    return -1 * Input.CurrentGamePadState[Slot].ThumbSticks.Right.X;
                case Buttons.RightThumbstickRight:
                    return Input.CurrentGamePadState[Slot].ThumbSticks.Right.X;
                default:
                    return 1.0f;
            }
        }
    }
}