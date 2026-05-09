using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルシーンのUI制御を担当するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("ゲームモードトグル（ToggleGroupに所属させること）")]
    [SerializeField] private Toggle toggleNormal;  // 「ノーマル」
    [SerializeField] private Toggle togglePoint;   // 「ポイント」

    [Header("難易度トグル（ToggleGroupに所属させること）")]
    [SerializeField] private Toggle toggleEasy;    // 「Easy」
    [SerializeField] private Toggle toggleNormalDiff; // 「Normal」
    [SerializeField] private Toggle toggleHard;    // 「Hard」

    [Header("開始ボタン")]
    [SerializeField] private Button startButton;

    [Header("遷移先シーン名")]
    [SerializeField] private string gameSceneName = "SampleScene";

    void Start()
    {
        // デフォルト選択状態を設定
        toggleNormal.isOn    = true;
        toggleNormalDiff.isOn = true;

        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    /// <summary>
    /// 「開始」ボタンが押されたときの処理
    /// </summary>
    void OnStartButtonClicked()
    {
        // ゲームモードを保存
        GameSettings.SelectedMode = togglePoint.isOn
            ? GameSettings.GameMode.Point
            : GameSettings.GameMode.Normal;

        // 難易度を保存
        if (toggleEasy.isOn)
            GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Easy;
        else if (toggleHard.isOn)
            GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Hard;
        else
            GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Normal;

        SceneManager.LoadScene(gameSceneName);
    }
}
