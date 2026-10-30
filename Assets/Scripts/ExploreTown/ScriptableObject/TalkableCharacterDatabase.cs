using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 町探索で話しかけられるキャラクターのデータを格納するスクリプタブルオブジェクト
 */

/// <summary>
/// 町探索で話しかけられる一人一人のキャラクターのデータを格納するクラス
/// </summary>
[System.Serializable]
public class TalkableCharacterData
{
    [Header("キャラクターID")]
    [SerializeField] private int _characterID;

    [Header("キャラクターの位置・大きさ・向き")]
    [SerializeField] private Vector3 _position;
    [SerializeField] private Vector3 _scale;
    [SerializeField] private ChangeVisualDirection.VisualDirection _direction;

    [Header("会話ID")]
    [SerializeField] private int _episodeID;

    public int CharacterID { get { return _characterID; } }
    public Vector3 Position { get { return _position; } }
    public Vector3 Scale { get { return _scale; } }
    public ChangeVisualDirection.VisualDirection Direction { get { return _direction; } }
    public int EpisodeID { get { return _episodeID; } }
}

/// <summary>
/// 町探索で話しかけられるキャラクター達のデータを格納するスクリプタブルオブジェクト
/// </summary>
[CreateAssetMenu(fileName = "TalkableCharacterDatabase", menuName = "ScriptableObjects/TalkableCharacterDatabase")]
public class TalkableCharacterDatabase : ScriptableObject
{
    [Header("話しかけられるキャラクターのデータ")]
    [SerializeField] private List<TalkableCharacterData> _talkableCharacterData;

    public TalkableCharacterData this[int index] => _talkableCharacterData[index];
    public IEnumerator<TalkableCharacterData> GetEnumerator() => _talkableCharacterData.GetEnumerator();
}
