
namespace GameStateManager
{
    public class UnassignedInputMessage : MessageBoxScreen
    {
        public UnassignedInputMessage(string screenName, string message, string menuTitle) :
           base(screenName, message, menuTitle, MessageBoxType.OK)
        {
            
        }
    }
}