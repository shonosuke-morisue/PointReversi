using System;
using UnityEngine;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public enum CellState { Empty, Black, White }

    [SerializeField] private Image diskImage;

    private int x, y;
    private CellState state = CellState.Empty;
    private Color cellDefaultColor;

    // クリック時に通知するイベント（購読者がGameManagerへの依存を持つ）
    public event Action<int, int> OnClicked;

    void Awake()
    {
        // セル背景のデフォルト色を記憶しておく
        cellDefaultColor = GetComponent<Image>().color;
    }

    // 座標を設定（BoardManagerから呼ばれる）
    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    // 状態を設定し、見た目を更新
    public void SetState(CellState newState)
    {
        state = newState;
        if (diskImage == null) { Debug.LogError($"diskImage が null です ({x},{y})"); return; }

        switch (state)
        {
            case CellState.Empty:
                diskImage.color = new Color(0, 0, 0, 0); // 透明
                break;
            case CellState.Black:
                diskImage.color = Color.black;
                break;
            case CellState.White:
                diskImage.color = Color.white;
                break;
        }

        var cellBg = GetComponent<Image>();
        Debug.Log($"SetState ({x},{y}) state={state} | diskImage.gameObject={diskImage.gameObject.name} | diskImage==cellBg:{diskImage == cellBg} | diskImage.color={diskImage.color} | diskImage.rectSize={diskImage.rectTransform.rect.size}");
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
        // 石があるマスはハイライト操作不要
        // （diskImageを誤って上書きしないよう早期リターン）
        if (state != CellState.Empty) return;

        GetComponent<Image>().color = isHighlighted
            ? new Color(0.8f, 0.8f, 1f, 1f)
            : cellDefaultColor;
    }
}
