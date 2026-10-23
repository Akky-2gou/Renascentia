using UnityEngine;
using GameTypes;

/*
 * 1回の戦闘の情報。敵の配置（スロット1・2が前衛、3・4が後衛）など
 * ステージ画像情報なども含む
 */

[CreateAssetMenu(menuName = "Game/Encounter/EncounterData")]
public class EncounterData : ScriptableObject
{
    public const int SlotCount = 4;

    [Header("前衛（空欄OK、4スロット全部空はNG）")]
    [SerializeField] private CharacterData _slot1;
    [SerializeField] private CharacterData _slot2;

    [Header("後衛（空欄OK、4スロット全部空はNG）")]
    [SerializeField] private CharacterData _slot3;
    [SerializeField] private CharacterData _slot4;

    // 指定スロット（0〜3）の敵。空スロットなら null
    public CharacterData GetEnemyAt(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return _slot1;
            case 1: return _slot2;
            case 2: return _slot3;
            case 3: return _slot4;
            default:
                Debug.LogError($"{name}: スロット番号{slotIndex}は範囲外です（0〜{SlotCount - 1}）");
                return null;
        }
    }

    // 指定スロット（0〜3）の前衛後衛。0・1が前衛、2・3が後衛
    public FormationRow GetRowAt(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
            case 1:
                return FormationRow.Front;
            case 2:
            case 3:
                return FormationRow.Back;
            default:
                Debug.LogError($"{name}: スロット番号{slotIndex}は範囲外です（0〜{SlotCount - 1}）");
                return FormationRow.None;
        }
    }

    // 1体でも敵が入っているか
    public bool HasAnyEnemy()
    {
        return _slot1 != null || _slot2 != null || _slot3 != null || _slot4 != null;
    }
}