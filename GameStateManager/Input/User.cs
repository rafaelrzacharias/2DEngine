namespace GameStateManager
{
    public class User
    {
        public int ControllerIndex;
        public bool IsPrimaryUser;
        public InputType InputType;
        public ActionMap[] ActionMaps;

        public bool IsActive { get { return ControllerIndex != -1; } }

        public User()
        {
            ControllerIndex = -1;
            IsPrimaryUser = false;
            InputType = InputType.NONE;

            ActionMaps = new ActionMap[Input.Actions.Length];
            Input.ResetActionMaps(ActionMaps);
        }
    }
}