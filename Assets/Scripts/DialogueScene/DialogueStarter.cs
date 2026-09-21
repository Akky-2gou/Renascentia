using UnityEngine;

/*
 * 会話シーンの会話を始めるスクリプト
 */

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueDisplay _dialogueDisplay; // DialogueDisplayを参照するための変数
    [SerializeField] private int _episodeIndex = 0; // 現在のエピソードのインデックス
    
    void Start()
    {
        // 会話開始
        _dialogueDisplay.StartTalking(_episodeIndex, false);
    }
}
