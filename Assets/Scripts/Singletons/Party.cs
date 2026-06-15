using System;
using Unity.VisualScripting;
using UnityEngine;

/*
 * パーティ情報とそのステータスを保持するシングルトン
 */

public class Party : MonoBehaviour
{
    const int MAX_PARTY_MEMBERS = 4; // パーティーメンバーの最大数
    private CharacterData[] _partyMembers = new CharacterData[MAX_PARTY_MEMBERS]; // パーティーメンバーのステータスを格納する配列

    /*
     * テスト用プログラム
     */
    [SerializeField] private CharacterData playerCharacter;

    void Awake()
    {
        // パーティーメンバーの初期化
        for (int i = 0; i < MAX_PARTY_MEMBERS; i++)
        {
            _partyMembers[i] = null; // 最初は全てのメンバーがいない状態にする
        }

        // テスト用コード
        _partyMembers[0] = playerCharacter;
        _partyMembers[1] = playerCharacter;
    }

    /// <summary>
    /// パーティメンバー全員を取得するgetter
    /// </summary>
    public CharacterData[] Members
    {
        get { return _partyMembers; }
    }

    /// <summary>
    /// パーティメンバーの一部を取得・変更するgetter・setter
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CharacterData this[int index]
    {
        set
        {
            if (index < 0 || index >= MAX_PARTY_MEMBERS)
            {
                Debug.LogError("Invalid party member index: " + index);
                return;
            }
            _partyMembers[index] = value; 
        }
        get 
        {
            if (index < 0 || index >= MAX_PARTY_MEMBERS)
            {
                Debug.LogError("Invalid party member index: " + index);
                return null;
            }
            return _partyMembers[index];
        }
    }
}
