using System.Collections.Generic;
using UnityEngine;

public static class CpuPlayer
{
    public enum AIDifficulty { Easy, Normal, Hard }

    private static readonly int[,] evaluationMap = new int[8, 8]
    {
        { 100, -20, 10,  5,  5, 10, -20, 100 },
        { -20, -50, -2, -2, -2, -2, -50, -20 },
        {  10,  -2,  0,  0,  0,  0,  -2,  10 },
        {   5,  -2,  0,  0,  0,  0,  -2,   5 },
        {   5,  -2,  0,  0,  0,  0,  -2,   5 },
        {  10,  -2,  0,  0,  0,  0,  -2,  10 },
        { -20, -50, -2, -2, -2, -2, -50, -20 },
        { 100, -20, 10,  5,  5, 10, -20, 100 },
    };

    public static Vector2Int GetMove(BoardManager boardManager, CellController.CellState cpuSide, AIDifficulty difficulty)
    {
        List<Vector2Int> validMoves = GetValidMoves(boardManager, cpuSide);
        if (validMoves.Count == 0) return new Vector2Int(-1, -1);

        switch (difficulty)
        {
            case AIDifficulty.Easy:
                return validMoves[Random.Range(0, validMoves.Count)];

            case AIDifficulty.Normal:
                Vector2Int[] corners = {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 7),
                    new Vector2Int(7, 0),
                    new Vector2Int(7, 7)
                };
                foreach (var corner in corners)
                    if (validMoves.Contains(corner)) return corner;
                return validMoves[Random.Range(0, validMoves.Count)];

            case AIDifficulty.Hard:
                int bestScore = int.MinValue;
                Vector2Int bestMove = validMoves[0];
                foreach (var move in validMoves)
                {
                    int score = evaluationMap[move.y, move.x]; // y,x順注意
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
                return bestMove;

            default:
                return validMoves[Random.Range(0, validMoves.Count)];
        }
    }

    private static List<Vector2Int> GetValidMoves(BoardManager boardManager, CellController.CellState player)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var cell = boardManager.GetCell(x, y).GetComponent<CellController>();
                if (cell.GetState() == CellController.CellState.Empty &&
                    boardManager.HasReversibleDisks(x, y, player)) // ここはboardManager内の関数を利用
                {
                    moves.Add(new Vector2Int(x, y));
                }
            }
        }

        return moves;
    }
}
