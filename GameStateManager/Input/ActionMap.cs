using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace GameStateManager
{
    // A combination of gamepad and keyboard keys mapped to a particular action.
    public class ActionMap
    {
#if DESKTOP
        public List<Keys> Keys = new List<Keys>();
#endif
#if DESKTOP || MOBILE
        public List<MouseButtons> MouseButtons = new List<MouseButtons>();
#endif
#if DESKTOP || CONSOLE
        public List<Buttons> Buttons = new List<Buttons>();
#endif
#if MOBILE
        // Not implemented yet!!!
#endif
    }
}