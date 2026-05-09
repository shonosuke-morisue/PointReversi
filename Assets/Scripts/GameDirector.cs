using UnityEngine;

/// <summary>
/// ゲーム全体の司令塔。各マネージャーを初期化し、入力とゲームの流れを制御する。
/// </summary>
public class GameDirector : MonoBehaviour
{
    public GameObject stonePrefab;
    public GameObject cellPrefab;
    public GameObject canvas;

    private Camera cam;
    private BoardManager board;
    private UIManager ui;
    private StoneManager stoneManager;
    private RuleChecker ruleChecker;

    void Start()
    {
        cam = Camera.main;

        // 各マネージャーを初期化
        board = new BoardManager();
        ui = new UIManager(canvas);
        stoneManager = new StoneManager(stonePrefab, cellPrefab, this.transform, board, ui);
        ruleChecker = new RuleChecker(board);

        // 初期配置（オセロの開始4石）
        stoneManager.GenerateStone(4, 4, 2);
        stoneManager.GenerateStone(5, 4, 1);
        stoneManager.GenerateStone(4, 5, 1);
        stoneManager.GenerateStone(5, 5, 2);

        CheckGameState();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = 15;
            Vector3 v = cam.ScreenToWorldPoint(screenPoint);
            int key_x = (int)(Mathf.Floor(v.x) + 5.0f);
            int key_y = (int)(9 - (Mathf.Floor(v.z) + 5.0f));

            // クリックが盤面内かつ空きマスであれば石を置く
            if (key_x >= 1 && key_x <= 8 && key_y >= 1 && key_y <= 8)
            {
                if (board.GetFieldColor(key_x - 1, key_y - 1) == 0)
                {
                    PutStone(key_x, key_y);
                }
            }
        }
    }

    /// <summary>
    /// 石を置き、ひっくり返し処理・ターン交代・状態更新を行う
    /// </summary>
    void PutStone(int key_x, int key_y)
    {
        bool[] isPossiblePutDir = ruleChecker.IsPutStone(key_x, key_y);
        bool isPossiblePut = false;

        foreach (var item in isPossiblePutDir)
        {
            if (item) { isPossiblePut = true; break; }
        }

        if (!isPossiblePut) return;

        // 石を配置
        stoneManager.GenerateStone(key_x, key_y, board.TurnPlayer);

        // 各方向についてひっくり返せる石を処理
        for (int i = 0; i < 8; i++)
        {
            if (!isPossiblePutDir[i]) continue;

            int checkPosX = key_x;
            int checkPosY = key_y;

            for (int ii = 0; ii < 8; ii++)
            {
                int[] checkPos = ruleChecker.CheckPosDir(i, checkPosX, checkPosY);
                checkPosX = checkPos[0];
                checkPosY = checkPos[1];

                // 自分の石に到達したら終了
                if (board.GetFieldColor(checkPosX - 1, checkPosY - 1) == board.TurnPlayer) break;

                // 相手の石があればひっくり返す
                if (board.GetFieldColor(checkPosX - 1, checkPosY - 1) == board.NotTurnPlayer)
                {
                    stoneManager.ReverseStone(checkPosX, checkPosY);
                }
            }
        }

        // ターン交代・UI更新・次の有効手チェック
        board.ChangeTurn();
        ui.UpdateTurnText(board.TurnPlayer);
        stoneManager.ClearCells();
        CheckGameState();
    }

    /// <summary>
    /// 現在のターンで置けるマスを調べ、有効手を表示する。
    /// 置ける場所がなければゲームオーバーを表示する。
    /// </summary>
    void CheckGameState()
    {
        bool isPossiblePut = false;

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                if (board.GetFieldColor(col, row) != 0) continue;

                bool[] dirs = ruleChecker.IsPutStone(col + 1, row + 1);
                foreach (var d in dirs)
                {
                    if (d)
                    {
                        isPossiblePut = true;
                        stoneManager.ShowValidCell(col, row);
                        break; // 同一マスに重複して表示しないよう1方向でもtrueなら抜ける
                    }
                }
            }
        }

        if (!isPossiblePut)
        {
            ui.ShowGameOver(board.BlackPoint, board.WhitePoint);
        }
    }
}
