using UnityEngine;

/*
 * パーティ情報を保持するシングルトン
 */

public class Party : MonoBehaviour
{
    const int MAX_PARTY_MEMBERS = 4; // パーティーメンバーの最大数
    private CharacterStatus[] _partyMembers = new CharacterStatus[MAX_PARTY_MEMBERS]; // パーティーメンバーのステータスを格納する配列

    void Start()
    {
        // パーティーメンバーの初期化
        for (int i = 0; i < MAX_PARTY_MEMBERS; i++)
        {
            _partyMembers[i] = null; // 最初は全てのメンバーがいない状態にする
        }
    }

    public CharacterStatus[] GetPartyMember()
    {
        return _partyMembers;
    }

    public void SetPartyMember(CharacterStatus partyMember, int index)
    {
        if (index < 0 || index >= MAX_PARTY_MEMBERS)
        {
            Debug.LogError("Invalid party member index: " + index);
            return;
        }
        _partyMembers[index] = partyMember;
    }
}
