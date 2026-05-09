/// <summary>
/// ボードの状態（フィールド配列・ターン・ポイント）を管理するクラス
/// </summary>
public class BoardManager
{
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
