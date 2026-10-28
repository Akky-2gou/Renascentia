using UnityEngine;

/*
 * シーンの種類と、シーン名の対応を持つデータ
 * シーンを増やすときは、SceneId・ここの欄・TryGetSceneName の switch の3か所を直すこと
 */

[CreateAssetMenu(fileName = "SceneTable", menuName = "Game/SceneTable")]
public class SceneTable : ScriptableObject
{
    [Header("Build Settings に登録したシーン名")]
    [SerializeField] private string _title;             // タイトル
    [SerializeField] private string _dialogue;          // 会話
    [SerializeField] private string _town;              // 町探索
    [SerializeField] private string _townTutorial;      // 町チュートリアル
    [SerializeField] private string _map;               // マップ
    [SerializeField] private string _battlePreparation; // 戦闘準備フェーズ
    [SerializeField] private string _battle;            // バトル
    [SerializeField] private string _battleTutorial;    // バトルチュートリアル
    [SerializeField] private string _townToMapUI;       // マップ移動UI

    /// <summary>シーンの種類から、シーン名を引く。設定が間違っていれば、エラーを出して false を返す</summary>
    public bool TryGetSceneName(SceneId id, out string sceneName)
    {
        switch (id)
        {
            case SceneId.Title:             sceneName = _title; break;
            case SceneId.Dialogue:          sceneName = _dialogue; break;
            case SceneId.Town:              sceneName = _town; break;
            case SceneId.TownTutorial:      sceneName = _townTutorial; break;
            case SceneId.Map:               sceneName = _map; break;
            case SceneId.BattlePreparation: sceneName = _battlePreparation; break;
            case SceneId.Battle:            sceneName = _battle; break;
            case SceneId.BattleTutorial:    sceneName = _battleTutorial; break;
            case SceneId.TownToMapUI:       sceneName = _townToMapUI; break;
            default:
                sceneName = null;
                Debug.LogError($"SceneTable: {id} は遷移先として指定できません");
                return false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"SceneTable: {id} のシーン名が空です");
            return false;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"SceneTable: シーン「{sceneName}」（{id}）が Build Settings に登録されていません");
            return false;
        }
        return true;
    }

    /// <summary>全部のシーンの設定を確認する。起動時に呼んで、間違いを早めに見つける</summary>
    public void Validate()
    {
        foreach (SceneId id in System.Enum.GetValues(typeof(SceneId)))
        {
            if (id == SceneId.None) continue;
            TryGetSceneName(id, out string _);
        }
    }
}