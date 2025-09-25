using System.Collections.Generic;
using Shashki;

namespace Shashki
{
    public class Move
    {
        public BoardCell From;
        public BoardCell To;
        public bool IsCapture;
        public List<PieceView> CapturedPieces = new List<PieceView>();
    }
}