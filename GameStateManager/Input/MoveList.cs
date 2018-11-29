using System;

namespace GameStateManager
{
    // Represents a set of available moves for matching. This internal storage
    // of this class is optimized for efficient match searches.
    public class MoveList
    {
        public string Name { get; private set; }
        private Move[] Moves;

        public MoveList(string name = "")
        {
            if (string.IsNullOrEmpty(name) == false)
                Name = name;

            Moves = new Move[]
                {
                new Move("Jump", Action.LK) { IsSubMove = true },
                new Move("Punch", Action.LP) { IsSubMove = true },
                new Move("Double Jump", Action.LK, Action.LK),
                new Move("Jump Kick", Action.LK | Action.LK),
                new Move("Quad Punch", Action.LP, Action.HP, Action.LP, Action.HP),
                new Move("Fireball", Action.DOWN, Action.DOWN_RIGHT, Action.RIGHT | Action.LP),
                new Move("Long Jump", Action.UP, Action.UP, Action.LK),
                new Move("Back Flip", Action.DOWN, Action.DOWN | Action.LK),
                new Move("30 Lives", Action.UP, Action.UP, Action.DOWN, Action.DOWN, Action.LEFT, Action.RIGHT, Action.LEFT, Action.RIGHT, Action.HK, Action.LK),
                };

            // Store the list of moves in order of decreasing sequence length.
            // This greatly simplifies the logic of the DetectMove method.
            Array.Sort(Moves);
        }


        // Finds the longest Move which matches the given input, if any.
        public Move DetectMoves(int controllerIndex)
        {
            // Perform a linear search for a move which matches the input. This relies
            // on the moves array being in order of decreasing sequence length.
            for (int i = 0; i < Moves.Length; i++)
            {
                if (Input.Matches(Moves[i], controllerIndex))
                    return Moves[i];
            }

            return null;
        }


        public int LongestMoveLength
        {
            // Since they are in decreasing order, the first move is the longest.
            get { return Moves[0].Sequence.Length; }
        }


        public int Length { get { return Moves.Length; } }

        public Move this[int i] { get { return Moves[i]; } }
    }
}