using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace GameStateManager
{
    public class User
    {
        public PlayerIndex Index;
        public bool IsPrimaryUser;
        public InputType InputType;
        public MouseState LastMouseState;
        public MouseState CurrentMouseState;
        public KeyboardState LastKeyboardState;
        public KeyboardState CurrentKeyboardState;
        public GamePadState LastGamePadState;
        public GamePadState CurrentGamePadState;
        public TouchCollection TouchState;
        public List<GestureSample> Gestures;

        private bool isLeftMouseDown;
        private const int dragThreshold = 3;
        public bool isDragging;
        public bool isDragComplete;

        private Vector2 dragMouseStart;
        public Vector2 MouseDragStartPosition { get { return dragMouseStart; } }

        private Vector2 dragMouseEnd;
        public Vector2 MouseDragEndPosition { get { return dragMouseEnd; } }

        public Vector2 MouseDragDelta { get; private set; }
        public float MouseDragDistance { get; private set; }

        public User()
        {
            LastMouseState = new MouseState();
            CurrentMouseState = new MouseState();
            LastKeyboardState = new KeyboardState();
            CurrentKeyboardState = new KeyboardState();
            LastGamePadState = new GamePadState();
            CurrentGamePadState = new GamePadState();
            Gestures = new List<GestureSample>();
        }

        public void UpdateInput()
        {
            switch (InputType)
            {
                case InputType.KEYBOARD:
                    {
                        LastMouseState = CurrentMouseState;
                        CurrentMouseState = Mouse.GetState();
                        LastKeyboardState = CurrentKeyboardState;
                        CurrentKeyboardState = Keyboard.GetState();

                        // If we were dragging and the left mouse button was released
                        if (CurrentMouseState.LeftButton == ButtonState.Released && isDragging)
                        {
                            isLeftMouseDown = false;
                            isDragging = false;
                            isDragComplete = true;
                            dragMouseEnd = CurrentMouseState.Position.ToVector2();

                            MouseDragDistance = Vector2.Distance(dragMouseStart, dragMouseEnd);
                            MouseDragDelta = dragMouseEnd - dragMouseStart;
                        }

                        // Let's set the left mouse down and the mouse origin
                        if (isLeftMouseDown == false && CurrentMouseState.LeftButton == ButtonState.Pressed &&
                                CurrentMouseState.Equals(LastMouseState) == false)
                        {
                            isLeftMouseDown = true;
                            isDragComplete = false;
                            dragMouseStart = CurrentMouseState.Position.ToVector2();
                        }

                        if (isLeftMouseDown && CurrentMouseState.LeftButton == ButtonState.Released &&
                                CurrentMouseState.Equals(LastMouseState) == false)
                            isLeftMouseDown = false;

                        // If dragging distance was over the current threshold (5 pixels),
                        // set the dragging to true.
                        if (isLeftMouseDown && isDragging == false)
                        {
                            Vector2 delta = dragMouseStart - CurrentMouseState.Position.ToVector2();

                            if (delta.Length() > dragThreshold)
                            {
                                isDragging = true;
                                dragMouseStart = CurrentMouseState.Position.ToVector2();
                            }
                        }
                    }
                    break;
                case InputType.GAMEPAD:
                    {
                        LastGamePadState = CurrentGamePadState;
                        CurrentGamePadState = GamePad.GetState(Index);

                        if (CurrentGamePadState.IsConnected == false && LastGamePadState.IsConnected)
                            Input.OnControllerDisconnected(Index);
                    }
                    break;
                case InputType.TOUCH:
                    {
                        TouchState = TouchPanel.GetState();
                        Gestures.Clear();

                        while (TouchPanel.IsGestureAvailable)
                            Gestures.Add(TouchPanel.ReadGesture());
                    }
                    break;
            }
        }
    }
}