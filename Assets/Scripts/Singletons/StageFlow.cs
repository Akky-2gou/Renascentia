using UnityEngine;
using Cysharp.Threading.Tasks;

/*
 * ステージ中の流れ(会話 → 準備 → バトル → 会話 → マップ)を進める
 * 各シーンは「終わった」ことを伝えるだけで、次の行き先はここで決める
 */

public class StageFlow : MonoBehaviour
{
    private enum Step { None, BeforeTalk, Battle, AfterTalk }

    public static StageFlow Instance { get; private set; }

    [SerializeField] private StageTable _stageTable;

    private StageData _current; // 今進めているステージ
    private Step _step = Step.None;

    /// <summary>ステージの流れの途中か(マップでステージを選んでから、マップに戻るまで)</summary>
    public bool IsInStage => _step != Step.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("StageFlowが複数存在します");
            return;
        }
        Instance = this;

        if (_stageTable == null) Debug.LogError("StageFlow: StageTable が設定されていません");
    }

    // マップでステージを選んだとき
    public void StartStage(MapId stage)
    {
        if (_stageTable == null)
        {
            Debug.LogError("StageFlow: StageTable が設定されていません");
            return;
        }
        if (!_stageTable.TryGet(stage, out _current)) return;

        // LastMapId は、戦闘準備へ遷移するときに SceneNavigator がセットする

        if (_current.HasBeforeTalk)
        {
            _step = Step.BeforeTalk;
            Go(new TransitionTarget(SceneId.Dialogue, _current.BeforeTalkId));
        }
        else
        {
            GoToPreparation();
        }
    }

    // 会話シーンが終わったとき(会話シーンから呼ぶ)
    public void OnTalkFinished()
    {
        switch (_step)
        {
            case Step.BeforeTalk: GoToPreparation(); break;
            case Step.AfterTalk: GoToMap(); break;
            default:
                Debug.LogError($"StageFlow: 想定外のタイミングで会話が終わりました ({_step})");
                break;
        }
    }

    /// <summary>会話シーンが始まったときに呼ぶ。会話のあとに行くシーンを、先に読み込んでおく</summary>
    public void PrepareAfterTalk()
    {
        PrepareAfterTalkAsync().Forget();
    }

    private async UniTaskVoid PrepareAfterTalkAsync()
    {
        SceneId next;
        switch (_step)
        {
            case Step.BeforeTalk: next = SceneId.BattlePreparation; break;
            case Step.AfterTalk: next = SceneId.Map; break;
            default:
                Debug.LogError($"StageFlow: 会話中ではないので、事前読み込みできません ({_step})");
                return;
        }

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("StageFlow: SceneLoader が存在しません");
            return;
        }

        // 会話シーンへの遷移がまだ終わっていなければ、終わるまで待つ
        Step stepAtStart = _step;
        await UniTask.WaitWhile(() => SceneLoader.Instance.IsTransitioning, cancellationToken: destroyCancellationToken);
        if (_step != stepAtStart) return; // 待っている間に会話が終わって、次へ進んでいたら何もしない

        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("StageFlow: SceneNavigator が存在しません");
            return;
        }
        SceneNavigator.Instance.Prepare(new TransitionTarget(next));
    }

    // バトルに勝ったとき(勝利後のボタン入力で呼ぶ)
    public void OnBattleWon()
    {
        if (_current == null || _step != Step.Battle)
        {
            Debug.LogError($"StageFlow: ステージのバトル中ではありません ({_step})");
            return;
        }

        UnlockNextStage(_current.Stage);

        if (_current.HasAfterTalk)
        {
            _step = Step.AfterTalk;
            Go(new TransitionTarget(SceneId.Dialogue, _current.AfterTalkId));
        }
        else
        {
            GoToMap();
        }
    }

    /// <summary>ステージの流れから抜ける（マップ・タイトル・町へ行くとき、SceneNavigator から呼ばれる）</summary>
    public void LeaveStage()
    {
        _step = Step.None;
        _current = null;
    }

    private void GoToPreparation()
    {
        _step = Step.Battle;
        Go(new TransitionTarget(SceneId.BattlePreparation, stage: _current.Stage));
    }

    private void GoToMap()
    {
        _step = Step.None;
        _current = null;
        Go(new TransitionTarget(SceneId.Map));
    }

    // クリアしたステージの次を解放する。すでに先まで解放済みなら何もしない
    private void UnlockNextStage(MapId cleared)
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("StageFlow: GameProgress が存在しないため、ステージを解放できません");
            return;
        }

        MapId next = (MapId)((int)cleared + 1);
        if (!System.Enum.IsDefined(typeof(MapId), next)) return; // 最後のステージなので、次はない

        if ((int)next > (int)GameProgress.Instance.StageProgress)
        {
            GameProgress.Instance.StageProgress = next;
        }
    }

    private void Go(TransitionTarget target)
    {
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("StageFlow: SceneNavigator が存在しません");
            return;
        }
        SceneNavigator.Instance.Go(target);
    }
}