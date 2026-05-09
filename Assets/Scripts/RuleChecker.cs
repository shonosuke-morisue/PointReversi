/// <summary>
/// オセロのルール判定を担当するクラス（有効手チェック・方向計算）
/// </summary>
public class RuleChecker
{
    private BoardManager board;

    public RuleChecker(BoardManager board)
    {
        this.board = board;
    }

    /// <summary>
    /// 指定マスに石が置けるか、8方向それぞれについて判定する
    /// </summary>
    /// <returns>各方向に置けるか（true=置ける）の配列（要素数8）</returns>
    public bool[] IsPutStone(int key_x, int key_y)
    {
        bool[] isPossiblePutDir = new bool[8];

        for (int i = 0; i < 8; i++)
        {
            int checkPosX = key_x;
            int checkPosY = key_y;

            for (int ii = 0; ii < 8; ii++)
            {
                int[] checkPos = CheckPosDir(i, checkPosX, checkPosY);
                checkPosX = checkPos[0];
                checkPosY = checkPos[1];

                // 盤からはみ出たら終了
                if (checkPosX > 8 || checkPosX < 1 || checkPosY > 8 || checkPosY < 1) break;

                // 1マス目に自分の石があれば終了
                if (board.GetFieldColor(checkPosX - 1, checkPosY - 1) == board.TurnPlayer && ii == 0) break;

                // 空白マスがあれば終了
                if (board.GetFieldColor(checkPosX - 1, checkPosY - 1) == 0) break;

                // 2マス目以降に自分の石があればこの方向に置ける
                if (board.GetFieldColor(checkPosX - 1, checkPosY - 1) == board.TurnPlayer)
                {
                    isPossiblePutDir[i] = true;
                    break;
                }
            }
        }

        return isPossiblePutDir;
    }

    /// <summary>
    /// 方向インデックスに対応する次のマス座標を返す
    /// </summary>
    /// <param name="dir">方向（0=上 1=右上 2=右 3=右下 4=下 5=左下 6=左 7=左上）</param>
    public int[] CheckPosDir(int dir, int checkPosX, int checkPosY)
    {
        switch (dir)
        {
            case 0: checkPosY -= 1; break;                          // 上
            case 1: checkPosX += 1; checkPosY -= 1; break;         // 右上
            case 2: checkPosX += 1; break;                          // 右
            case 3: checkPosX += 1; checkPosY += 1; break;         // 右下
            case 4: checkPosY += 1; break;                          // 下
            case 5: checkPosX -= 1; checkPosY += 1; break;         // 左下
            case 6: checkPosX -= 1; break;                          // 左
            case 7: checkPosX -= 1; checkPosY -= 1; break;         // 左上
        }

        return new int[] { checkPosX, checkPosY };
    }
}
