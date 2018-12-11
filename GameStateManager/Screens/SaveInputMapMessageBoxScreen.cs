
namespace GameStateManager
{
    public class SaveInputMapMessageBoxScreen : MessageBoxScreen
    {
        public SaveInputMapMessageBoxScreen(string screenName, string message, string menuTitle) :
            base(screenName, message, menuTitle, MessageBoxType.YESNO)
        {
            
        }
    }
}