using UnityEngine;

/*
 * 会話シーンの会話を始めるスクリプト
 */

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueDisplay _dialogueDisplay; // DialogueDisplayを参照するための変数
    
    void Start()
    {
        if (CurrentConversation.Instance == null)
        {
            Debug.LogError("CurrentConversationが見つかりません（シングルトンの親オブジェクトがシーンにありません）");
            return;
        }

        CurrentConversation conversation = CurrentConversation.Instance;
        if (!conversation.IsSet)
        {
            Debug.LogError("会話IDが設定されていません");
            return;
        }

        // 会話開始
        _dialogueDisplay.StartTalking(conversation.ConversationId, DialogueMode.InDialogueScene);
    }
}
