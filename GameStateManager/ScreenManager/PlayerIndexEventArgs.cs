using Microsoft.Xna.Framework;
using System;

namespace GameStateManager
{
    // Custom event argument which includes the index of the player who
    // triggered the event. This is used by the MenuEntry.OnSelected event.
    public class PlayerIndexEventArgs : EventArgs
    {
        public PlayerIndex PlayerIndex { get; private set; }

        public PlayerIndexEventArgs(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }
    }
}