using Shashki;

namespace Shashki
{
    /// <summary>
    /// Описание возможного хода (обычного или с поеданием).
    /// </summary>
    public class Move
    {
        public BoardCell From;
        public BoardCell To;
        public bool IsCapture;
        public PieceView CapturedPiece;
    }
}