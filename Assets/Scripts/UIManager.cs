using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas上のUI表示を管理するクラス
/// </summary>
public class UIManager
{
    // Canvas の GameObject（子要素: 0=手番テキスト 1=黒ポイント 2=白ポイント）
    private GameObject canvas;

    public UIManager(GameObject canvas)
    {
        this.canvas = canvas;
    }

    /// <summary>
    /// 手番テキストを更新する
    /// </summary>
    public void UpdateTurnText(int turnPlayer)
    {
        Text turnText = canvas.transform.GetChild(0).GetComponent<Text>();

        if (turnPlayer == 1)
        {
            turnText.color = new Color(0, 0, 0, 1);
            turnText.text = "黒の手番";
        }
        else
        {
            turnText.color = new Color(1, 1, 1, 1);
            turnText.text = "白の手番";
        }
    }

    /// <summary>
    /// 黒・白のポイント表示を更新する
    /// </summary>
    public void UpdatePoints(int blackPoint, int whitePoint)
    {
        canvas.transform.GetChild(1).GetComponent<Text>().text = "黒 " + blackPoint + "点";
        canvas.transform.GetChild(2).GetComponent<Text>().text = "白 " + whitePoint + "点";
    }

    /// <summary>
    /// ゲームオーバー画面を表示する
    /// </summary>
    public void ShowGameOver(int blackPoint, int whitePoint)
    {
        string resultText;

        if (blackPoint == whitePoint)
        {
            resultText = "引き分け";
        }
        else if (blackPoint > whitePoint)
        {
            resultText = "黒の勝ち";
        }
        else
        {
            resultText = "白の勝ち";
        }

        canvas.transform.GetChild(0).GetComponent<Text>().text = "GAME OVER\n" + resultText;
    }
}
