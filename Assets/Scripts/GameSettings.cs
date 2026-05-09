/// <summary>
/// シーン間でゲーム設定を引き渡す静的クラス
/// </summary>
public static class GameSettings
{
    public enum GameMode
    {
        Normal, // 通常オセロ（石1枚＝1点）
        Point   // ポイント制（ひっくり返すたびに石の価値が上がる）
    }

    public static GameMode SelectedMode       = GameMode.Normal;
    public static CpuPlayer.AIDifficulty SelectedDifficulty = CpuPlayer.AIDifficulty.Normal;
}
