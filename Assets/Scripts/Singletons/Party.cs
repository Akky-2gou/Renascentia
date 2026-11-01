using System;
using Unity.VisualScripting;
using UnityEngine;
using GameTypes;

/*
 * パーティ情報とそのステータスを保持するシングルトン
 */

public class Party : MonoBehaviour
{
    const int MAX_PARTY_MEMBERS = 4; // パーティーメンバーの最大数
    const int FRONT_SLOT_COUNT = 2; // 先頭2枠が前衛、残りが後衛
    private CharacterData[] _partyMembers = new CharacterData[MAX_PARTY_MEMBERS]; // パーティーメンバーのステータスを格納する配列

    public static Party Instance { get; private set; }

    [Header("前衛（空欄OK、4スロット全部空はNG）")]
    [SerializeField] private CharacterData _slot1;
    [SerializeField] private CharacterData _slot2;
    [Header("後衛（空欄OK、4スロット全部空はNG）")]
    [SerializeField] private CharacterData _slot3;
    [SerializeField] private CharacterData _slot4;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("Partyが複数存在します");
            return;
        }
        Instance = this;
        
        // パーティーメンバーの初期化
        for (int i = 0; i < MAX_PARTY_MEMBERS; i++)
        {
            _partyMembers[i] = null; // 最初は全てのメンバーがいない状態にする
        }

        // Inspectorで指定した初期メンバーを反映する（空欄は null のまま）
        _partyMembers[0] = _slot1;
        _partyMembers[1] = _slot2;
        _partyMembers[2] = _slot3;
        _partyMembers[3] = _slot4;
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

    /// <summary>
    /// 指定スロット（0〜3）の前衛後衛。0・1が前衛、2・3が後衛
    /// </summary>
    public FormationRow GetRowAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_PARTY_MEMBERS)
        {
            Debug.LogError("Invalid party member index: " + slotIndex);
            return FormationRow.None;
        }
        return slotIndex < FRONT_SLOT_COUNT ? FormationRow.Front : FormationRow.Back;
    }

    /// <summary>
    /// 1人でもメンバーが入っているか
    /// </summary>
    public bool HasAnyMember()
    {
        foreach (CharacterData member in _partyMembers)
        {
            if (member != null) return true;
        }
        return false;
    }

    /// <summary>
    /// 学祭用: 編成を外から直接書き換える
    /// </summary>
    public void SetMembersForFestival(CharacterData slot1, CharacterData slot2, CharacterData slot3, CharacterData slot4)
    {
        // 実際に使われている編成（_partyMembers）を書き換える
        _partyMembers[0] = slot1;
        _partyMembers[1] = slot2;
        _partyMembers[2] = slot3;
        _partyMembers[3] = slot4;
    }
}
