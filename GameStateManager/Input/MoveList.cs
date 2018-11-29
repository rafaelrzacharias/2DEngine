using System;

namespace GameStateManager
{
    // Represents a set of available moves for matching. This internal storage
    // of this class is optimized for efficient match searches.
    public class MoveList
    {
        private Move[] Moves;

        public MoveList(Move[] moves)
        {
            // Store the list of moves in order of decreasing sequence length.
            // This greatly simplifies the logic of the DetectMove method.
            Array.Sort(moves);
            Moves = moves;
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
    }
}