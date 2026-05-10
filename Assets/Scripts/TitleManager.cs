using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// タイトルシーンのUI制御を担当するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("ゲームモード選択ドロップダウン")]
    [SerializeField] private TMP_Dropdown modeDropdown;      // 「ノーマル / ポイント」

    [Header("難易度選択ドロップダウン")]
    [SerializeField] private TMP_Dropdown difficultyDropdown; // 「Easy / Normal / Hard」

    [Header("開始ボタン")]
    [SerializeField] private Button startButton;

    [Header("遷移先シーン名")]
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        // ゲームモード選択肢を設定（index 0: Normal Mode, 1: Point Mode）
        modeDropdown.ClearOptions();
        modeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Normal Mode",
            "Point Mode"
        });
        modeDropdown.value = 0;
        modeDropdown.RefreshShownValue();

        // 難易度選択肢を設定（index 0: Easy, 1: Normal, 2: Hard）
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Easy",
            "Normal",
            "Hard"
        });
        difficultyDropdown.value = 0; // デフォルト: Easy
        difficultyDropdown.RefreshShownValue();

        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    /// <summary>
    /// 「開始」ボタンが押されたときの処理
    /// </summary>
    void OnStartButtonClicked()
    {
        // ゲームモードを保存（0: ノーマル, 1: ポイント）
        GameSettings.SelectedMode = modeDropdown.value == 1
            ? GameSettings.GameMode.Point
            : GameSettings.GameMode.Normal;

        // 難易度を保存（0: Easy, 1: Normal, 2: Hard）
        switch (difficultyDropdown.value)
        {
            case 0:
                GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Easy;
                break;
            case 2:
                GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Hard;
                break;
            default:
                GameSettings.SelectedDifficulty = CpuPlayer.AIDifficulty.Normal;
                break;
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
