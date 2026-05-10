using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体を統括するクラス。
/// ボードの初期化・石の配置・ターン管理・CPU対戦・ゲームオーバー判定を担う。
/// タイトルシーンで選択されたゲームモード・難易度を GameSettings から読み込む。
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ボード設定")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform boardParent;

    [Header("UI")]
    [SerializeField] private GameObject canvas;

    [Header("CPU設定")]
    [SerializeField] private bool isCpuMode = false;
    [SerializeField] private CellController.CellState cpuSide = CellController.CellState.White;

    [Header("リザルトウィンドウ")]
    [SerializeField] private GameObject resultPanel;                    // ゲーム終了時に表示するパネル
    [SerializeField] private string gameSceneName = "GameScene";        // リトライ時の遷移先シーン名

    [Header("オプションウィンドウ")]
    [SerializeField] private GameObject optionsPanel;                   // オプションウィンドウ

    private BoardManager board;
    private UIManager ui;

    private CellController.CellState currentTurn = CellController.CellState.Black;
    private bool isGameOver = false;

    // タイトルシーンから引き継いだ設定
    private GameSettings.GameMode gameMode;
    private CpuPlayer.AIDifficulty cpuDifficulty;

    void Start()
    {
        // タイトルで選択した設定を読み込む
        gameMode     = GameSettings.SelectedMode;
        cpuDifficulty = GameSettings.SelectedDifficulty;

        board = new BoardManager();
        ui    = new UIManager(canvas);

        // 各ウィンドウを初期非表示にする
        if (resultPanel  != null) resultPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        InitializeBoard();
        PlaceInitialStones();
        ShowValidMoves();
        ui.UpdateTurnText((int)currentTurn);
    }

    /// <summary>
    /// 8x8のセルグリッドを生成し、BoardManagerに登録する
    /// </summary>
    void InitializeBoard()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, boardParent);
                CellController cell = cellObj.GetComponent<CellController>();
                cell.SetPosition(x, y);
                cell.OnClicked += OnCellClicked;
                board.RegisterCell(x, y, cellObj);
            }
        }
    }

    /// <summary>
    /// オセロの初期配置（中央4石）をセットする
    /// </summary>
    void PlaceInitialStones()
    {
        PlaceStone(3, 3, CellController.CellState.White);
        PlaceStone(4, 3, CellController.CellState.Black);
        PlaceStone(3, 4, CellController.CellState.Black);
        PlaceStone(4, 4, CellController.CellState.White);
    }

    /// <summary>
    /// セルがクリックされたときの処理
    /// </summary>
    public void OnCellClicked(int x, int y)
    {
        if (isGameOver) return;
        if (optionsPanel != null && optionsPanel.activeSelf) return; // オプション表示中は操作不可
        if (isCpuMode && currentTurn == cpuSide) return;

        TryPlaceStone(x, y);
    }

    /// <summary>
    /// 指定マスへの着手を試みる。有効手でなければ何もしない。
    /// </summary>
    void TryPlaceStone(int x, int y)
    {
        if (board.GetFieldColor(x, y) != 0) return;
        if (!board.HasReversibleDisks(x, y, currentTurn)) return;

        PlaceStone(x, y, currentTurn);
        FlipStones(x, y, currentTurn);
        ChangeTurn();
    }

    /// <summary>
    /// 指定マスに石を置き、ボード状態とポイントを更新する
    /// </summary>
    void PlaceStone(int x, int y, CellController.CellState color)
    {
        var cell = board.GetCell(x, y).GetComponent<CellController>();
        cell.SetState(color);
        board.SetFieldColor(x, y, (int)color);
        board.SetFieldPoint(x, y, 1);

        // ポイントモードのみ石に点数を表示する
        if (gameMode == GameSettings.GameMode.Point)
            cell.SetPoint(1);

        if (color == CellController.CellState.Black) board.AddBlackPoint(1);
        else board.AddWhitePoint(1);
    }

    /// <summary>
    /// 石を置いた後、ひっくり返せる相手の石をすべて返す。
    /// ノーマルモード：石1枚＝1点の通常カウント
    /// ポイントモード：ひっくり返すたびに石の価値が上がる
    /// </summary>
    void FlipStones(int x, int y, CellController.CellState player)
    {
        int playerColor   = (int)player;
        int opponentColor = playerColor == 1 ? 2 : 1;

        int[] dx = { -1,  0,  1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1,  0, 0,  1, 1, 1 };

        for (int d = 0; d < 8; d++)
        {
            List<Vector2Int> toFlip = new List<Vector2Int>();
            int nx = x + dx[d];
            int ny = y + dy[d];

            while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
            {
                int fieldColor = board.GetFieldColor(nx, ny);

                if (fieldColor == opponentColor)
                {
                    toFlip.Add(new Vector2Int(nx, ny));
                }
                else if (fieldColor == playerColor && toFlip.Count > 0)
                {
                    foreach (var pos in toFlip)
                    {
                        FlipSingleStone(pos.x, pos.y, player);
                    }
                    break;
                }
                else
                {
                    break;
                }

                nx += dx[d];
                ny += dy[d];
            }
        }

        ui.UpdatePoints(board.BlackPoint, board.WhitePoint);
    }

    /// <summary>
    /// 1枚の石をひっくり返す。ゲームモードに応じて点数計算を切り替える。
    /// </summary>
    void FlipSingleStone(int x, int y, CellController.CellState player)
    {
        var cell = board.GetCell(x, y).GetComponent<CellController>();
        cell.SetState(player);
        board.SetFieldColor(x, y, (int)player);

        if (gameMode == GameSettings.GameMode.Point)
        {
            // ポイントモード：石の価値はひっくり返された回数分増加する
            int stonePoint = board.GetFieldPoint(x, y) + 1;
            board.SetFieldPoint(x, y, stonePoint);
            cell.SetPoint(stonePoint); // 石に点数を表示

            if (player == CellController.CellState.Black)
            {
                board.AddBlackPoint(stonePoint);
                board.SubtractWhitePoint(stonePoint - 1);
            }
            else
            {
                board.AddWhitePoint(stonePoint);
                board.SubtractBlackPoint(stonePoint - 1);
            }
        }
        else
        {
            // ノーマルモード：石1枚＝1点の通常カウント
            if (player == CellController.CellState.Black)
            {
                board.AddBlackPoint(1);
                board.SubtractWhitePoint(1);
            }
            else
            {
                board.AddWhitePoint(1);
                board.SubtractBlackPoint(1);
            }
        }
    }

    /// <summary>
    /// ターンを交代する。置ける手がなければパス、双方パスならゲームオーバー。
    /// </summary>
    void ChangeTurn()
    {
        ClearHighlights();

        currentTurn = currentTurn == CellController.CellState.Black
            ? CellController.CellState.White
            : CellController.CellState.Black;

        if (!HasAnyValidMove(currentTurn))
        {
            CellController.CellState opponent = currentTurn == CellController.CellState.Black
                ? CellController.CellState.White
                : CellController.CellState.Black;

            if (!HasAnyValidMove(opponent))
            {
                isGameOver = true;
                ui.ShowGameOver(board.BlackPoint, board.WhitePoint);
                if (resultPanel != null) resultPanel.SetActive(true); // リザルトウィンドウを表示
                return;
            }

            // 現在のプレイヤーはパス → 相手に戻す
            currentTurn = opponent;
        }

        ui.UpdateTurnText((int)currentTurn);
        ShowValidMoves();

        if (isCpuMode && currentTurn == cpuSide)
        {
            StartCoroutine(CpuTurn());
        }
    }

    bool HasAnyValidMove(CellController.CellState player)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (board.GetFieldColor(x, y) == 0 && board.HasReversibleDisks(x, y, player))
                    return true;
        return false;
    }

    void ShowValidMoves()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                bool isValid = board.GetFieldColor(x, y) == 0
                            && board.HasReversibleDisks(x, y, currentTurn);
                board.GetCell(x, y).GetComponent<CellController>().SetHighlight(isValid);
            }
    }

    void ClearHighlights()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                board.GetCell(x, y).GetComponent<CellController>().SetHighlight(false);
    }

    IEnumerator CpuTurn()
    {
        yield return new WaitForSeconds(0.5f);
        Vector2Int move = CpuPlayer.GetMove(board, cpuSide, cpuDifficulty);
        if (move.x >= 0)
            TryPlaceStone(move.x, move.y);
    }

    // ─── ボタン共通アクション（Inspector の OnClick から割り当て） ───

    /// <summary>タイトルシーンへ遷移する</summary>
    public void GoToTitle() => SceneManager.LoadScene("TitleScene");

    /// <summary>現在のモード・難易度を保持したままゲームをリセットする</summary>
    public void RetryGame() => SceneManager.LoadScene(gameSceneName);

    /// <summary>オプションウィンドウを開く</summary>
    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    /// <summary>オプションウィンドウを閉じる</summary>
    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }
}
