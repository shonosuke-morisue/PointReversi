using UnityEngine;

/// <summary>
/// ボードの状態（フィールド配列・ターン・ポイント・セルオブジェクト）を管理するクラス
/// </summary>
public class BoardManager
{
    // 各マスのセルGameObject（0始まり座標）
    private GameObject[,] cells = new GameObject[8, 8];
    // 現在の手番プレイヤー（1:黒 2:白）
    public int TurnPlayer { get; private set; } = 1;

    // 非手番プレイヤー（1:黒 2:白）
    public int NotTurnPlayer { get; private set; } = 2;

    // 経過ターン数
    public int TurnCount { get; private set; } = 0;

    // 黒の合計ポイント
    public int BlackPoint { get; private set; } = 0;

    // 白の合計ポイント
    public int WhitePoint { get; private set; } = 0;

    // ゲームフィールド [x, y, 0:色 / 1:点数]（色: 0=なし 1=黒 2=白）
    private int[,,] gameField = new int[8, 8, 2];

    // フィールドの色を取得
    public int GetFieldColor(int x, int y) => gameField[x, y, 0];

    // フィールドの点数を取得
    public int GetFieldPoint(int x, int y) => gameField[x, y, 1];

    // フィールドの色をセット
    public void SetFieldColor(int x, int y, int color) => gameField[x, y, 0] = color;

    // フィールドの点数をセット
    public void SetFieldPoint(int x, int y, int point) => gameField[x, y, 1] = point;

    // 黒ポイントを加算
    public void AddBlackPoint(int points) { BlackPoint += points; }

    // 黒ポイントを減算
    public void SubtractBlackPoint(int points) { BlackPoint -= points; }

    // 白ポイントを加算
    public void AddWhitePoint(int points) { WhitePoint += points; }

    // 白ポイントを減算
    public void SubtractWhitePoint(int points) { WhitePoint -= points; }

    /// <summary>
    /// セルGameObjectを登録する（BoardManager初期化時にGameManagerから呼ぶ）
    /// </summary>
    public void RegisterCell(int x, int y, GameObject cell)
    {
        cells[x, y] = cell;
    }

    /// <summary>
    /// 指定座標のセルGameObjectを返す
    /// </summary>
    public GameObject GetCell(int x, int y)
    {
        return cells[x, y];
    }

    /// <summary>
    /// 指定座標にplayerが石を置いたとき、1枚以上ひっくり返せるか判定する
    /// CellState.Empty=0, Black=1, White=2 がgameFieldの整数値と対応している
    /// </summary>
    public bool HasReversibleDisks(int x, int y, CellController.CellState player)
    {
        int playerColor = (int)player;        // Black=1, White=2
        int opponentColor = playerColor == 1 ? 2 : 1;

        // 8方向の差分（dx, dy）
        int[] dx = { -1,  0,  1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1,  0, 0,  1, 1, 1 };

        for (int d = 0; d < 8; d++)
        {
            int nx = x + dx[d];
            int ny = y + dy[d];
            bool hasOpponent = false;

            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
            {
                int fieldColor = GetFieldColor(nx, ny);

                if (fieldColor == opponentColor)
                {
                    // 相手の石が続く
                    hasOpponent = true;
                }
                else if (fieldColor == playerColor && hasOpponent)
                {
                    // 相手の石を挟んで自分の石があればひっくり返せる
                    return true;
                }
                else
                {
                    // 空きマスか、相手石なしで自分の石に到達 → この方向はNG
                    break;
                }

                nx += dx[d];
                ny += dy[d];
            }
        }

        return false;
    }

    /// <summary>
    /// ターンを交代する
    /// </summary>
    public void ChangeTurn()
    {
        TurnCount++;

        if (TurnPlayer == 1)
        {
            TurnPlayer = 2;
            NotTurnPlayer = 1;
        }
        else
        {
            TurnPlayer = 1;
            NotTurnPlayer = 2;
        }
    }
}
