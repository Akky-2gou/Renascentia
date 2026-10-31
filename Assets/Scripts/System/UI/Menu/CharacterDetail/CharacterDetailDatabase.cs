using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDetailDatabase", menuName = "Renascentia/Character Detail Database")]
public class CharacterDetailDatabase : ScriptableObject
{
    [SerializeField] private List<CharacterDetailRecord> _records = new List<CharacterDetailRecord>();

    public IReadOnlyList<CharacterDetailRecord> Records => _records;

    public CharacterDetailRecord GetByCharacterId(int characterId)
    {
        return _records.Find(record => record.characterId == characterId);
    }
}

[System.Serializable]
public class CharacterDetailRecord
{
    public int characterId;
    public string characterName;
    public string species;
    public string hp;
    public string attack;
    public string defence;
    public string normalElement;
    public string normalRange;
    public string normalDamage;
    public string skillElement;
    public string skillRange;
    public string skillDamage;
    public string skillDescription;
    public string skill2Damage;
    public string skill2Description;
    public string skill3Damage;
    public string skill3Description;
    public string passive;
    public string bonusPassive;

    // 後から Inspector で差し込む背景アイコンと位置補正
    public Sprite detailBackgroundIcon;
    public Vector2 detailBackgroundIconOffset;
    public float detailBackgroundIconScale = 1f;

    public Sprite teamCompositionIcon;
    public Sprite characterSlotIcon;

    // 後から Inspector で差し込む、該当キャラクターの Prefab
    public GameObject characterPrefab;
}
