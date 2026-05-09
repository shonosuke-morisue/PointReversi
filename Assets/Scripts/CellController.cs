using System;
using UnityEngine;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public enum CellState { Empty, Black, White }

    [SerializeField] private Image diskImage;

    private int x, y;
    private CellState state = CellState.Empty;

    // クリック時に通知するイベント（購読者がGameManagerへの依存を持つ）
    public event Action<int, int> OnClicked;

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

        Debug.Log($"SetState ({x},{y}) newState={newState} | color={diskImage.color} | enabled={diskImage.enabled} | active={diskImage.gameObject.activeInHierarchy} | sprite={diskImage.sprite}");
    }

    public CellState GetState()
    {
        return state;
    }

    public void OnClick()
    {
        Debug.Log($"OnClick() 呼び出し: ({x},{y})");
        OnClicked?.Invoke(x, y);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (state != CellState.Empty)
        {
            GetComponent<Image>().color = Color.white;
            return;
        }

        if (isHighlighted)
        {
            GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f, 1f);
        }
        else
        {
            GetComponent<Image>().color = Color.white;
        }
    }
}
