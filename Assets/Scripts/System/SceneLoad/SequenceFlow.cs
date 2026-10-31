using UnityEngine;

/*
 * SceneSequence の並びを、上から順に進める
 * 各シーンは終わったときに Next() を呼ぶだけで、次の行き先はここで決める
 */

public class SequenceFlow : MonoBehaviour
{
    public static SequenceFlow Instance { get; private set; }

    private SceneSequence _current; // 今進めている並び
    private int _index = -1;        // 今いるのが並びの何番目か

    /// <summary>並びの途中か（最後の行き先へ遷移したら false に戻る）</summary>
    public bool IsRunning => _current != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("SequenceFlowが複数存在します");
            return;
        }
        Instance = this;
    }

    /// <summary>並びを最初から始める（1つ目の行き先へ遷移する）</summary>
    public void Begin(SceneSequence sequence)
    {
        if (sequence == null || sequence.Steps.Count == 0)
        {
            Debug.LogError("SequenceFlow: 並びが空です");
            return;
        }

        _current = sequence;
        _index = -1;
        Next();
    }

    /// <summary>今のシーンが終わったときに呼ぶ。並びの次の行き先へ遷移する</summary>
    public void Next()
    {
        if (_current == null)
        {
            Debug.LogError("SequenceFlow: 並びが始まっていません");
            return;
        }

        _index++;
        if (_index >= _current.Steps.Count)
        {
            Debug.LogError($"SequenceFlow: {_current.name} の最後より先へは進めません");
            End();
            return;
        }

        TransitionTarget target = _current.Steps[_index];
        if (_index == _current.Steps.Count - 1) End(); // 最後の行き先へ行ったら、並びは終わり（以降は普通の操作に戻る）

        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("SequenceFlow: SceneNavigator が存在しません");
            return;
        }
        SceneNavigator.Instance.Go(target);
    }

    /// <summary>並びをやめる（タイトルに戻ったときなど）</summary>
    public void End()
    {
        _current = null;
        _index = -1;
    }
}