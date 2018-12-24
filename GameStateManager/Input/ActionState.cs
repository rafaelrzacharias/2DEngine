namespace GameStateManager
{
    // A struct that represents an input state from a controller.
    public struct ActionState
    {
        public bool IsPressed; // If the action is being pressed.
        public bool IsTriggered; // If the action was just pressed this frame.
        public float Magnitude; // The action's intensity (for analog buttons).
        public double Duration; // How long the action has being pressed for.


        // Reset all fields to its default values.
        public void Reset()
        {
            IsPressed = false;
            IsTriggered = false;
            Magnitude = 0.0f;
            Duration = 0.0;
        }
    }
}