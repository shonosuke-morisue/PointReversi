using System.Collections.Generic;
using UnityEngine;

public static class CpuPlayer
{
    public enum AIDifficulty { Easy, Normal, Hard }

    // 各マスの戦略的価値（コーナー=高、コーナー隣接=低）
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

    private const int SearchDepth = 4; // Hard の探索深さ

    // ─── 外部向けエントリポイント ───────────────────────────────

    public static Vector2Int GetMove(BoardManager boardManager, CellController.CellState cpuSide, AIDifficulty difficulty)
    {
        List<Vector2Int> validMoves = GetValidMoves(boardManager, cpuSide);
        if (validMoves.Count == 0) return new Vector2Int(-1, -1);

        switch (difficulty)
        {
            case AIDifficulty.Easy:
                // ランダムに手を選ぶ
                return validMoves[Random.Range(0, validMoves.Count)];

            case AIDifficulty.Normal:
                // 評価マップで最高スコアのマスに打つ（1手先）
                return GetEvaluationMapMove(validMoves);

            case AIDifficulty.Hard:
                // ミニマックス法 + αβ枝刈りで最善手を選ぶ（SearchDepth 手先）
                int[,] board = BoardToArray(boardManager);
                return GetMinimaxMove(board, (int)cpuSide);

            default:
                return validMoves[Random.Range(0, validMoves.Count)];
        }
    }

    // ─── Normal：評価マップによる1手先選択 ──────────────────────

    private static Vector2Int GetEvaluationMapMove(List<Vector2Int> validMoves)
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = validMoves[0];
        foreach (var move in validMoves)
        {
            int score = evaluationMap[move.y, move.x]; // y,x 順に注意
            if (score > bestScore)
            {
                bestScore = score;
                bestMove  = move;
            }
        }
        return bestMove;
    }

    // ─── Hard：ミニマックス法 + αβ枝刈り ───────────────────────

    /// <summary>
    /// ルートノードで全有効手を評価し、最善手を返す
    /// </summary>
    private static Vector2Int GetMinimaxMove(int[,] board, int cpuColor)
    {
        List<Vector2Int> validMoves = GetValidMovesFromArray(board, cpuColor);
        if (validMoves.Count == 0) return new Vector2Int(-1, -1);

        int bestScore  = int.MinValue;
        Vector2Int bestMove = validMoves[0];
        int alpha = int.MinValue;
        int beta  = int.MaxValue;

        foreach (var move in validMoves)
        {
            int[,] newBoard = SimulateMove(board, move.x, move.y, cpuColor);
            int opponentColor = cpuColor == 1 ? 2 : 1;
            int score = Minimax(newBoard, SearchDepth - 1, alpha, beta, false, cpuColor, opponentColor);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove  = move;
            }
            if (score > alpha) alpha = score;
        }

        return bestMove;
    }

    /// <summary>
    /// ミニマックス再帰探索。isMaximizing=true のとき CPU ターン（最大化）。
    /// αβ枝刈りにより探索不要な枝をスキップする。
    /// </summary>
    private static int Minimax(int[,] board, int depth, int alpha, int beta, bool isMaximizing, int cpuColor, int opponentColor)
    {
        int currentColor  = isMaximizing ? cpuColor : opponentColor;
        List<Vector2Int> validMoves = GetValidMovesFromArray(board, currentColor);

        // 手がない場合：パスまたはゲーム終了の判定
        if (validMoves.Count == 0)
        {
            if (depth > 0)
            {
                // 相手側に手があればパス（手番を交代して継続）
                int otherColor = isMaximizing ? opponentColor : cpuColor;
                if (GetValidMovesFromArray(board, otherColor).Count > 0)
                    return Minimax(board, depth - 1, alpha, beta, !isMaximizing, cpuColor, opponentColor);
            }
            // 双方手なし or 深さ0 → 盤面評価を返す
            return EvaluateBoard(board, cpuColor);
        }

        // 探索深さの上限に達したら盤面を評価して返す
        if (depth == 0)
            return EvaluateBoard(board, cpuColor);

        if (isMaximizing)
        {
            int maxScore = int.MinValue;
            foreach (var move in validMoves)
            {
                int[,] newBoard = SimulateMove(board, move.x, move.y, cpuColor);
                int score = Minimax(newBoard, depth - 1, alpha, beta, false, cpuColor, opponentColor);
                if (score > maxScore) maxScore = score;
                if (score > alpha)   alpha     = score;
                if (beta <= alpha)   break; // βカット：これ以上探索しても最小化側は選ばない
            }
            return maxScore;
        }
        else
        {
            int minScore = int.MaxValue;
            foreach (var move in validMoves)
            {
                int[,] newBoard = SimulateMove(board, move.x, move.y, opponentColor);
                int score = Minimax(newBoard, depth - 1, alpha, beta, true, cpuColor, opponentColor);
                if (score < minScore) minScore = score;
                if (score < beta)     beta     = score;
                if (beta <= alpha)    break; // αカット：これ以上探索しても最大化側は選ばない
            }
            return minScore;
        }
    }

    /// <summary>
    /// 盤面を評価スコアに変換する。
    /// CPU の石の評価マップ合計 − 相手の石の評価マップ合計
    /// </summary>
    private static int EvaluateBoard(int[,] board, int cpuColor)
    {
        int opponentColor = cpuColor == 1 ? 2 : 1;
        int score = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if      (board[y, x] == cpuColor)      score += evaluationMap[y, x];
                else if (board[y, x] == opponentColor)  score -= evaluationMap[y, x];
            }
        return score;
    }

    // ─── 盤面シミュレーション用ユーティリティ ────────────────────

    /// <summary>
    /// BoardManager の状態を int[y,x] の配列にコピーして返す
    /// </summary>
    private static int[,] BoardToArray(BoardManager boardManager)
    {
        int[,] board = new int[8, 8];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                board[y, x] = boardManager.GetFieldColor(x, y);
        return board;
    }

    /// <summary>
    /// 指定した手を仮想的に打った後の盤面を返す（元の盤面は変更しない）
    /// </summary>
    private static int[,] SimulateMove(int[,] board, int x, int y, int color)
    {
        int[,] newBoard = (int[,])board.Clone();
        newBoard[y, x]  = color;

        int   opponentColor = color == 1 ? 2 : 1;
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0,  1, 1, 1 };

        for (int d = 0; d < 8; d++)
        {
            var toFlip = new List<Vector2Int>();
            int nx = x + dx[d];
            int ny = y + dy[d];

            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
            {
                if (newBoard[ny, nx] == opponentColor)
                    toFlip.Add(new Vector2Int(nx, ny));
                else if (newBoard[ny, nx] == color && toFlip.Count > 0)
                {
                    foreach (var pos in toFlip)
                        newBoard[pos.y, pos.x] = color;
                    break;
                }
                else
                    break;

                nx += dx[d];
                ny += dy[d];
            }
        }

        return newBoard;
    }

    /// <summary>
    /// int[,] 盤面から指定プレイヤーの有効手リストを生成する
    /// </summary>
    private static List<Vector2Int> GetValidMovesFromArray(int[,] board, int color)
    {
        var moves = new List<Vector2Int>();
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (board[y, x] == 0 && HasReversibleDisksOnArray(board, x, y, color))
                    moves.Add(new Vector2Int(x, y));
        return moves;
    }

    /// <summary>
    /// int[,] 盤面で指定座標にひっくり返せる石があるか判定する
    /// </summary>
    private static bool HasReversibleDisksOnArray(int[,] board, int x, int y, int color)
    {
        int   opponentColor = color == 1 ? 2 : 1;
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0,  1, 1, 1 };

        for (int d = 0; d < 8; d++)
        {
            int  nx          = x + dx[d];
            int  ny          = y + dy[d];
            bool hasOpponent = false;

            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
            {
                if      (board[ny, nx] == opponentColor) hasOpponent = true;
                else if (board[ny, nx] == color && hasOpponent) return true;
                else    break;

                nx += dx[d];
                ny += dy[d];
            }
        }
        return false;
    }

    /// <summary>
    /// BoardManager から指定プレイヤーの有効手リストを生成する（Easy/Normal 用）
    /// </summary>
    private static List<Vector2Int> GetValidMoves(BoardManager boardManager, CellController.CellState player)
    {
        var moves = new List<Vector2Int>();
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                var cell = boardManager.GetCell(x, y).GetComponent<CellController>();
                if (cell.GetState() == CellController.CellState.Empty &&
                    boardManager.HasReversibleDisks(x, y, player))
                    moves.Add(new Vector2Int(x, y));
            }
        return moves;
    }
}
