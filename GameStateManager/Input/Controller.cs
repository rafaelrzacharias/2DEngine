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


    public enum MotorType
    {
        BOTH,
        LEFT,
        RIGHT
    }


    public enum VibratorMode
    {
        NONE,
        CONSTANT,
        INCREASING,
        DECREASING
    }


    // Represent a controller. It can be aassigned to a player or an AI agent.
    public class Controller
    {
        public int Slot;
        public bool IsPrimaryUser;
        public double LeftMotorTotalDuration;
        public double RightMotorTotalDuration;
        public VibratorMode LeftMotorMode;
        public VibratorMode RightMotorMode;
        public float LeftMotorInitialMagnitude;
        public float RightMotorInitialMagnitude;
        public float leftMotorCurrentMagnitude;
        public float rightMotorCurrentMagnitude;
        public double leftMotorRemainingDuration;
        public double rightMotorRemainingDuration;
        public VibratorMode LeftIntermittenceMode;
        public VibratorMode RightIntermittenceMode;
        public double leftIntermittenceTimer;
        public double rightIntermittenceTimer;
        public double leftResetTimer;
        public double rightResetTimer;
        public static readonly double MIN_RESET_TIME = 0.1;
        public bool PauseVibration;
        public ControllerType Type;
        public ActionMap[] ActionMaps;
        public ActionState[] ActionStates;

        public bool IsActive { get { return Slot != -1; } }


        // Constructs a new Controller object.
        public Controller()
        {
            Slot = -1;

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

                        if (PauseVibration == false && (RightMotorMode != VibratorMode.NONE || LeftMotorMode != VibratorMode.NONE))
                            UpdateVibrations(elapsedTime);
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


        // Sets the controller vibration, given a dration, magnitude, motor type and vibration mode.
        public void Vibrate(double duration, float magnitude, MotorType motorType, 
            VibratorMode vibrationMode, VibratorMode intermittenceMode)
        {
            switch (motorType)
            {
                case MotorType.LEFT:
                    {
                        LeftMotorTotalDuration = duration;
                        leftMotorRemainingDuration = LeftMotorTotalDuration;
                        LeftMotorInitialMagnitude = magnitude;
                        LeftMotorMode = vibrationMode;
                        LeftIntermittenceMode = intermittenceMode;
                    }
                    break;
                case MotorType.RIGHT:
                    {
                        RightMotorTotalDuration = duration;
                        rightMotorRemainingDuration = RightMotorTotalDuration;
                        RightMotorInitialMagnitude = magnitude;
                        RightMotorMode = vibrationMode;
                        RightIntermittenceMode = intermittenceMode;
                    }
                    break;
                case MotorType.BOTH:
                    {
                        LeftMotorTotalDuration = duration;
                        leftMotorRemainingDuration = LeftMotorTotalDuration;
                        LeftMotorInitialMagnitude = magnitude;
                        LeftMotorMode = vibrationMode;
                        LeftIntermittenceMode = intermittenceMode;

                        RightMotorTotalDuration = duration;
                        rightMotorRemainingDuration = RightMotorTotalDuration;
                        RightMotorInitialMagnitude = magnitude;
                        RightMotorMode = vibrationMode;
                        RightIntermittenceMode = intermittenceMode;
                    }
                    break;
            }
        }


        // Updates the vibration of the left motor at each frame.
        private void UpdateVibrations(double elapsedTime)
        {
            int a = 0;
            float b = 0;

            int li = 1;
            leftMotorRemainingDuration -= elapsedTime;
            leftMotorCurrentMagnitude = 0f;

            switch (LeftMotorMode)
            {
                case VibratorMode.INCREASING:
                    a = 1;
                    b = 1 - LeftMotorInitialMagnitude;
                    break;
                case VibratorMode.DECREASING:
                    a = -1;
                    b = LeftMotorInitialMagnitude;
                    break;
            }

            leftMotorCurrentMagnitude = LeftMotorInitialMagnitude + a * b * (float)System.Math.Sqrt((LeftMotorTotalDuration - leftMotorRemainingDuration) / LeftMotorTotalDuration);

            if (LeftIntermittenceMode != VibratorMode.NONE)
                li = ApplyIntermittence(elapsedTime, LeftIntermittenceMode, LeftMotorTotalDuration, leftMotorRemainingDuration, ref leftIntermittenceTimer, ref leftResetTimer);

            if (leftMotorRemainingDuration <= 0.0)
                StopVibration(MotorType.LEFT);

            a = 0;
            b = 0;

            int ri = 1;
            rightMotorRemainingDuration -= elapsedTime;
            rightMotorCurrentMagnitude = 0f;

            switch (RightMotorMode)
            {
                case VibratorMode.INCREASING:
                    a = 1;
                    b = 1 - RightMotorInitialMagnitude;
                    break;
                case VibratorMode.DECREASING:
                    a = -1;
                    b = RightMotorInitialMagnitude;
                    break;
            }

            rightMotorCurrentMagnitude = RightMotorInitialMagnitude + a * b * (float)System.Math.Sqrt((RightMotorTotalDuration - rightMotorRemainingDuration) / RightMotorTotalDuration);

            if (RightIntermittenceMode != VibratorMode.NONE)
                ri = ApplyIntermittence(elapsedTime, RightIntermittenceMode, RightMotorTotalDuration, rightMotorRemainingDuration, ref rightIntermittenceTimer, ref rightResetTimer);

            if (rightMotorRemainingDuration <= 0.0)
                StopVibration(MotorType.RIGHT);

            System.Console.WriteLine("Vibrating slot: " + Slot.ToString());
            GamePad.SetVibration(Slot, leftMotorCurrentMagnitude * li, rightMotorCurrentMagnitude * ri);
        }


        // Applies quick pauses to the motor during the duration of the vibration.
        private int ApplyIntermittence(double elapsedTime, VibratorMode intermittenceMode, 
            double motorTotalDuration, double motorRemainingDuration, ref double timer, ref double resetTimer)
        {
            timer += elapsedTime;

            int a = 0;
            float b = 0;

            float initialThreshold = MathHelper.Lerp((float)MIN_RESET_TIME, (float)motorTotalDuration, 0.1f);

            switch (intermittenceMode)
            {
                case VibratorMode.INCREASING:
                    a = 1;
                    b = 1 - initialThreshold;
                    break;
                case VibratorMode.DECREASING:
                    a = -1;
                    b = initialThreshold;
                    break;
            }

            double threshold = initialThreshold + a * b * System.Math.Sqrt((motorTotalDuration - motorRemainingDuration) / motorTotalDuration);

            if (threshold < MIN_RESET_TIME)
                threshold = MIN_RESET_TIME;

            if (timer > threshold)
            {
                resetTimer += elapsedTime;

                if (resetTimer > MIN_RESET_TIME)
                {
                    resetTimer = 0.0;
                    timer = 0.0;
                }
                else
                    return 0;
            }

            return 1;
        }

        public void StopVibration(MotorType motorType = MotorType.BOTH)
        {
            switch (motorType)
            {
                case MotorType.LEFT:
                    {
                        LeftMotorInitialMagnitude = 0f;
                        LeftMotorTotalDuration = 0.0;
                        leftMotorRemainingDuration = 0.0;
                        leftMotorCurrentMagnitude = 0f;
                        LeftIntermittenceMode = VibratorMode.NONE;
                        LeftMotorMode = VibratorMode.NONE;
                        leftIntermittenceTimer = 0.0;
                        leftResetTimer = 0.0;
                    }
                    break;
                case MotorType.RIGHT:
                    {
                        RightMotorInitialMagnitude = 0f;
                        RightMotorTotalDuration = 0.0;
                        rightMotorRemainingDuration = 0.0;
                        rightMotorCurrentMagnitude = 0f;
                        RightMotorMode = VibratorMode.NONE;
                        RightIntermittenceMode = VibratorMode.NONE;
                        rightIntermittenceTimer = 0.0;
                        rightResetTimer = 0.0;
                    }
                    break;
                case MotorType.BOTH:
                    {
                        LeftMotorInitialMagnitude = 0f;
                        LeftMotorTotalDuration = 0.0;
                        leftMotorRemainingDuration = 0.0;
                        leftMotorCurrentMagnitude = 0f;
                        LeftIntermittenceMode = VibratorMode.NONE;
                        LeftMotorMode = VibratorMode.NONE;
                        leftIntermittenceTimer = 0.0;
                        leftResetTimer = 0.0;

                        RightMotorInitialMagnitude = 0f;
                        RightMotorTotalDuration = 0.0;
                        rightMotorRemainingDuration = 0.0;
                        rightMotorCurrentMagnitude = 0f;
                        RightMotorMode = VibratorMode.NONE;
                        RightIntermittenceMode = VibratorMode.NONE;
                        rightIntermittenceTimer = 0.0;
                        rightResetTimer = 0.0;
                    }
                    break;
            }
        }
    }
}