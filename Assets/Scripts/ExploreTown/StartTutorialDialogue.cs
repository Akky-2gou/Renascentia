using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/*
 * チュートリアル会話を始める
 */

public class StartTutorialDialogue : MonoBehaviour
{
    [Header("DialogueDisplayの参照")]
    [SerializeField] private DialogueDisplay _dialogueDisplay;

    [Header("チュートリアル会話のID")]
    [SerializeField] private int _episodeIndex = 0;

    [Header("TutorialCanvasの参照")]
    [SerializeField] private Canvas _tutorialCanvas;


    // <summary>
    // 会話を非同期で行う処理
    // </summary>
    public async UniTask TalkAsync(CancellationToken cancellationToken)
    {
        // TutorialCanvas を非表示
        _tutorialCanvas.enabled = false;
        
        // 会話開始
        _dialogueDisplay.StartTalking(_episodeIndex, DialogueMode.InTown);

        // 外部のキャンセル要求とオブジェクト破棄時のキャンセルを結合(usingをつけるとlinkedCtsを自動的に破棄)
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());

        // 会話終了まで待機
        await UniTask.WaitUntil(() => _dialogueDisplay.IsEpisodeCompleted, cancellationToken: linkedCts.Token);

        // TutorialCanvas を表示
        _tutorialCanvas.enabled = true;

        // 自身のオブジェクトを無効化し，もう一度会話が始まるのを防ぐ
        this.gameObject.SetActive(false);
    }
}
