using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 会話データを格納するスクリプタブルオブジェクト
 */

/// <summary>
/// 全エピソードを格納するスクリプタブルオブジェクト
/// </summary>
public class EpisodeDatabase : ScriptableObject {
    public EpisodeData[] _data; // 各エピソードをID順に格納
}

/// <summary>
/// 一連のセリフのまとまりを格納するクラス
/// </summary>
[System.Serializable]
public class EpisodeData
{
    [SerializeReference] public DialogueData[] _data; // セリフデータの配列
}

/// <summary>
/// 各セリフに関するデータを格納するクラス
/// </summary>
[System.Serializable]
public class DialogueData
{
    public int _leftCharacterId; // 左側のキャラクターのID
    public int _leftCharacterExpressionId; // 左側のキャラクターの表情ID

    public int _rightCharacterId; // 右側のキャラクターのID
    public int _rightCharacterExpressionId; // 右側のキャラクターの表情ID

    public int _isCharacterSpeaking; // キャラクターが話しているかどうか（-1: どちらも話していない、0: 右側、1: 左側）

    public string _dialogueText; // 会話テキスト

    public int _textBoxType; // テキストボックスの種類（0: 通常、1: トゲトゲなど）

    public int _backgroundId; // 背景のID

    public int _bgmId; // BGMのID

    public int _seId; // SEのID

    public bool _isChoice; // 選択肢かどうか（true: 選択肢、false: 通常のセリフ）

    [SerializeReference] public List<DialogueData> _nextDialogue; // 次のセリフデータへの参照
}