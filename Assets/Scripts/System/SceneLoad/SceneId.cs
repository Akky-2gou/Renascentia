/*
 * 遷移先のシーン
 */

public enum SceneId
{
    None,
    Title,             // タイトル
    Dialogue,          // 会話
    Town,              // 町探索
    TownTutorial,      // 町チュートリアル
    Map,               // マップ
    BattlePreparation, // 戦闘準備フェーズ
    Battle,            // バトル
    BattleTutorial,     // バトルチュートリアル
    TownToMapUI,         // マップ移動UI
}