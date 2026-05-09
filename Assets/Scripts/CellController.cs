using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public enum CellState { Empty, Black, White }

    [SerializeField] private Image diskImage;
    [SerializeField] private TMP_Text pointText; // 石の点数表示テキスト

    private int x, y;
    private CellState state = CellState.Empty;
    private Color cellDefaultColor;

    public event Action<int, int> OnClicked;

    void Awake()
    {
        cellDefaultColor = GetComponent<Image>().color;
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 石の色と状態を更新する。石を置いた際にハイライトも自動解除する。
    /// </summary>
    public void SetState(CellState newState)
    {
        state = newState;
        if (diskImage == null) { Debug.LogError($"diskImage が null です ({x},{y})"); return; }

        // 石を置く・取り除く際にセル背景をデフォルト色に戻す（ハイライト解除）
        GetComponent<Image>().color = cellDefaultColor;

        switch (state)
        {
            case CellState.Empty:
                diskImage.color = new Color(0, 0, 0, 0); // 透明
                SetPoint(0);
                break;
            case CellState.Black:
                diskImage.color = Color.black;
                break;
            case CellState.White:
                diskImage.color = Color.white;
                break;
        }
    }

    /// <summary>
    /// 石の点数を表示する。0以下の場合は非表示にする。
    /// </summary>
    public void SetPoint(int point)
    {
        if (pointText == null) return;
        pointText.text = point > 0 ? point.ToString() : "";
    }

    public CellState GetState()
    {
        return state;
    }

    public void OnClick()
    {
        OnClicked?.Invoke(x, y);
    }

    public void SetHighlight(bool isHighlighted)
    {
        // 石があるマスはハイライト対象外
        if (state != CellState.Empty) return;

        GetComponent<Image>().color = isHighlighted
            ? new Color(0.8f, 0.8f, 1f, 1f)
            : cellDefaultColor;
    }
}
