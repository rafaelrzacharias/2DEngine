using Microsoft.Xna.Framework.Input;
using System;

namespace GameStateManager
{
    // Describes a sequences of buttons which must be pressed to activate the move.
    public class Move : IComparable
    {
        public string Name;

        // The sequence of button presses required to activate this move.
        public Action[] Sequence;

        // Set this to true if the input used to activate this move
        // may be reused as a component of longer moves.
        public bool IsSubMove;


        public Move(string name, params Action[] sequence)
        {
            Name = name;
            Sequence = sequence;
        }


        // Default implementation of IComparable.
        public int CompareTo(object obj)
        {
            return Sequence.Length;
        }


        // A real game would implement this method in a derived class.
        public void PerformMove(string name) { }
    }
}