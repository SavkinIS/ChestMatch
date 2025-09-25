namespace Shashki
{
    public interface IBotStrategy
    {
        (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder);
    }
}