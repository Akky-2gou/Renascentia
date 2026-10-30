using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem;

/*
 * 会話可能なオブジェクトを表すスクリプト
 */

public class TalkableObject : MonoBehaviour
{
    [SerializeField] private DialogueDisplay _dialogueDisplay; // DialogueDisplayを参照するための変数
    [SerializeField] private int _episodeIndex = 0; // 現在のエピソードのインデックス

    public DialogueDisplay DialogueDisplay { set { _dialogueDisplay = value; } }
    public int EpisodeIndex { set { _episodeIndex = value; } }

    // <summary>
    // 会話を非同期で行う処理
    // </summary>
    public async UniTask TalkAsync(CancellationToken cancellationToken)
    {
        // 会話開始
        _dialogueDisplay.StartTalking(_episodeIndex, DialogueMode.InTown);

        // 外部のキャンセル要求とオブジェクト破棄時のキャンセルを結合(usingをつけるとlinkedCtsを自動的に破棄)
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());

        // 会話終了まで待機
        await UniTask.WaitUntil(() => _dialogueDisplay.IsEpisodeCompleted, cancellationToken: linkedCts.Token);
    }
}
