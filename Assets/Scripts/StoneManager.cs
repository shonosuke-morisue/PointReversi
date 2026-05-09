using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 石オブジェクトの生成・ひっくり返し・有効マス表示を管理するクラス
/// </summary>
public class StoneManager
{
    private GameObject stonePrefab;
    private GameObject cellPrefab;
    private Transform parent;      // 石を子要素として格納する親Transform
    private BoardManager board;
    private UIManager ui;

    public StoneManager(GameObject stonePrefab, GameObject cellPrefab, Transform parent, BoardManager board, UIManager ui)
    {
        this.stonePrefab = stonePrefab;
        this.cellPrefab = cellPrefab;
        this.parent = parent;
        this.board = board;
        this.ui = ui;
    }

    /// <summary>
    /// 指定座標に石を生成し、フィールド情報を更新する
    /// </summary>
    public void GenerateStone(int x, int y, int stoneColor)
    {
        float xPos = x - 4.5f;
        float yPos = (9 - y) - 4.5f;

        GameObject stone = Object.Instantiate(stonePrefab, new Vector3(xPos, 0.01f, yPos), Quaternion.identity);
        stone.name = "stone_" + x + "_" + y;
        stone.transform.parent = parent;

        if (stoneColor == 1)
        {
            // 黒石
            stone.GetComponent<Renderer>().material.color = Color.black;
            stone.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.white;
            board.AddBlackPoint(1);
        }
        else if (stoneColor == 2)
        {
            // 白石
            board.AddWhitePoint(1);
        }

        board.SetFieldColor(x - 1, y - 1, stoneColor);
        board.SetFieldPoint(x - 1, y - 1, 1);
    }

    /// <summary>
    /// 指定座標の石をひっくり返す（ポイント制ルールを適用）
    /// </summary>
    public void ReverseStone(int checkPosX, int checkPosY)
    {
        string stoneName = "stone_" + checkPosX + "_" + checkPosY;
        GameObject stone = parent.Find(stoneName).gameObject;

        // ひっくり返された回数分だけ点数が増加する
        int stonePoint = board.GetFieldPoint(checkPosX - 1, checkPosY - 1) + 1;
        stone.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = stonePoint.ToString();

        if (board.TurnPlayer == 1)
        {
            // 黒番がひっくり返す
            stone.GetComponent<Renderer>().material.color = Color.black;
            stone.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.white;
            board.AddBlackPoint(stonePoint);
            board.SubtractWhitePoint(stonePoint - 1);
            board.SetFieldColor(checkPosX - 1, checkPosY - 1, 1);
        }
        else
        {
            // 白番がひっくり返す
            stone.GetComponent<Renderer>().material.color = Color.white;
            stone.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.black;
            board.SubtractBlackPoint(stonePoint - 1);
            board.AddWhitePoint(stonePoint);
            board.SetFieldColor(checkPosX - 1, checkPosY - 1, 2);
        }

        board.SetFieldPoint(checkPosX - 1, checkPosY - 1, stonePoint);

        // ポイント表示を更新
        ui.UpdatePoints(board.BlackPoint, board.WhitePoint);
    }

    /// <summary>
    /// 指定マスに有効手表示セルを生成する
    /// </summary>
    public void ShowValidCell(int col, int row)
    {
        float xPos = col - 3.5f;
        float yPos = (8 - row) - 4.5f;
        Object.Instantiate(cellPrefab, new Vector3(xPos, 0.01f, yPos), Quaternion.identity);
    }

    /// <summary>
    /// 全ての有効手表示セルを削除する
    /// </summary>
    public void ClearCells()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag("cell");
        foreach (GameObject cell in cells)
        {
            Object.Destroy(cell);
        }
    }
}
