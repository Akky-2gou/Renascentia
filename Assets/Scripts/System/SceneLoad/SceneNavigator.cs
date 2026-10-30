using UnityEngine;

/*
 * 「どこへ行くか」を受け取って、必要な状態をセットしてから、SceneLoader に遷移を頼む
 * ボタンや会話データ・ステージデータは、シーン遷移を直接呼ばずに、ここを通す
 */

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("SceneNavigatorが複数存在します");
            return;
        }
        Instance = this;
    }

    /// <summary>指定の行き先へ遷移する</summary>
    public void Go(TransitionTarget target)
    {
        if (!TryApplyParameter(target)) return;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneNavigator: SceneLoader が存在しません");
            return;
        }
        SceneLoader.Instance.LoadScene(target.Scene);
    }

    /// <summary>行き先が確実に分かっているときに、先に読み込んでおく（状態は変えない）</summary>
    public void Prepare(TransitionTarget target)
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneNavigator: SceneLoader が存在しません");
            return;
        }
        SceneLoader.Instance.Preload(target.Scene);
    }

    // 行き先に応じて、遷移の前に状態をセットする。失敗したら false
    private bool TryApplyParameter(TransitionTarget target)
    {
        switch (target.Scene)
        {
            case SceneId.Dialogue:
                if (target.Parameter < 0)
                {
                    Debug.LogError($"SceneNavigator: 会話IDが不正です: {target.Parameter}（0以上を指定してください）");
                    return false;
                }
                if (CurrentConversation.Instance == null)
                {
                    Debug.LogError("SceneNavigator: CurrentConversation が存在しません");
                    return false;
                }
                CurrentConversation.Instance.SetConversation(target.Parameter);
                return true;

            case SceneId.BattlePreparation:
                if (target.Stage == MapId.None)
                {
                    Debug.LogError("SceneNavigator: 戦闘準備へ遷移するには、ステージの指定が必要です");
                    return false;
                }
                if (GameProgress.Instance == null)
                {
                    Debug.LogError("SceneNavigator: GameProgress が存在しません");
                    return false;
                }
                GameProgress.Instance.LastMapId = target.Stage; // 以降の会話やバトルは、ここから読む
                return true;

            // ステージの外へ出るシーン。ステージの途中で抜けたときのために、ステージの状態を消す
            case SceneId.Title:
            case SceneId.Town:
            case SceneId.TownTutorial:
            case SceneId.Map:
                if (StageFlow.Instance != null) StageFlow.Instance.LeaveStage();
                return true;

            // パラメータが要らないシーン
            case SceneId.Battle:
            case SceneId.BattleTutorial:
            case SceneId.TownToMapUI:
                return true;

            // バトルなど、パラメータの扱いが決まっていないシーンは、エラーにして気づけるようにする
            default:
                Debug.LogError($"SceneNavigator: {target} 未知の設定です");
                return false;
        }
    }
}