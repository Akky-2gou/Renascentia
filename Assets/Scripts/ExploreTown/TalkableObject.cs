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
    
    private int _sistineID = 2;
    public DialogueDisplay DialogueDisplay { set { _dialogueDisplay = value; } }
    public int CharacterID { private get; set; }

    // <summary>
    // 会話を非同期で行う処理
    // </summary>
    public async UniTask TalkAsync(CancellationToken cancellationToken, int episodeIDBase)
    {
        int chara1ID = (episodeIDBase - 900) / 10;
        int id1 = chara1ID + 2;
        if (_sistineID + 2 < id1) id1 = _sistineID + 2;
        int id2 = CharacterID + 2;
        if (_sistineID + 2 < id2) id2 = _sistineID + 2;
        int epiId = 900 + 10 * id1 + id2;
        Debug.Log($"会話ID: {id1}");
        Debug.Log($"会話ID: {id2}");
        Debug.Log($"会話ID: {epiId}");

        // 会話開始
        _dialogueDisplay.StartTalking(epiId, DialogueMode.InTown);

        // 外部のキャンセル要求とオブジェクト破棄時のキャンセルを結合(usingをつけるとlinkedCtsを自動的に破棄)
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());

        // 会話終了まで待機
        await UniTask.WaitUntil(() => _dialogueDisplay.IsEpisodeCompleted, cancellationToken: linkedCts.Token);
    }
}
