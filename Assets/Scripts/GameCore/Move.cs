using System.Collections.Generic;
using System.Numerics;
using Shashki;

namespace Shashki
{
    public class Move
    {
        public Vector2 From;
        public Vector2 To;
        public bool IsCapture;
        public List<PieceView> CapturedPieces = new List<PieceView>();
    }
}