using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム全体を統括するクラス。
/// ボードの初期化・石の配置・ターン管理・CPU対戦・ゲームオーバー判定を担う。
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ボード設定")]
    [SerializeField] private GameObject cellPrefab;   // CellControllerがアタッチされたプレハブ
    [SerializeField] private Transform boardParent;   // セルを配置する親Transform（GridLayoutGroup推奨）

    [Header("UI")]
    [SerializeField] private GameObject canvas;

    [Header("CPU設定")]
    [SerializeField] private bool isCpuMode = false;
    [SerializeField] private CpuPlayer.AIDifficulty cpuDifficulty = CpuPlayer.AIDifficulty.Normal;
    [SerializeField] private CellController.CellState cpuSide = CellController.CellState.White;

    private BoardManager board;
    private UIManager ui;

    private CellController.CellState currentTurn = CellController.CellState.Black;
    private bool isGameOver = false;

    void Start()
    {
        board = new BoardManager();
        ui = new UIManager(canvas);

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
                cell.OnClicked += OnCellClicked;  // クリックイベントを購読
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
    /// セルがクリックされたときの処理（CellController.OnClickedから呼ばれる）
    /// </summary>
    public void OnCellClicked(int x, int y)
    {
        if (isGameOver) return;

        // CPU番のクリックは無視する
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
        board.GetCell(x, y).GetComponent<CellController>().SetState(color);
        board.SetFieldColor(x, y, (int)color);

        if (color == CellController.CellState.Black) board.AddBlackPoint(1);
        else board.AddWhitePoint(1);
    }

    /// <summary>
    /// 石を置いた後、ひっくり返せる相手の石をすべて返す
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
                    // 相手石を挟めたのでひっくり返す
                    foreach (var pos in toFlip)
                    {
                        board.GetCell(pos.x, pos.y).GetComponent<CellController>().SetState(player);
                        board.SetFieldColor(pos.x, pos.y, playerColor);

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
                // 双方置けない → ゲームオーバー
                isGameOver = true;
                ui.ShowGameOver(board.BlackPoint, board.WhitePoint);
                return;
            }

            // 現在のプレイヤーはパス → 相手に戻す
            currentTurn = opponent;
        }

        ui.UpdateTurnText((int)currentTurn);
        ShowValidMoves();

        // CPU番なら自動で着手する
        if (isCpuMode && currentTurn == cpuSide)
        {
            StartCoroutine(CpuTurn());
        }
    }

    /// <summary>
    /// 指定プレイヤーが1手以上置けるか確認する
    /// </summary>
    bool HasAnyValidMove(CellController.CellState player)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board.GetFieldColor(x, y) == 0 && board.HasReversibleDisks(x, y, player))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 現在のターンの有効手をハイライト表示する
    /// </summary>
    void ShowValidMoves()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool isValid = board.GetFieldColor(x, y) == 0
                            && board.HasReversibleDisks(x, y, currentTurn);
                board.GetCell(x, y).GetComponent<CellController>().SetHighlight(isValid);
            }
        }
    }

    /// <summary>
    /// 全セルのハイライトを解除する
    /// </summary>
    void ClearHighlights()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                board.GetCell(x, y).GetComponent<CellController>().SetHighlight(false);
            }
        }
    }

    /// <summary>
    /// CPUが少し待ってから着手するコルーチン
    /// </summary>
    IEnumerator CpuTurn()
    {
        yield return new WaitForSeconds(0.5f);
        Vector2Int move = CpuPlayer.GetMove(board, cpuSide, cpuDifficulty);
        if (move.x >= 0)
        {
            TryPlaceStone(move.x, move.y);
        }
    }
}
