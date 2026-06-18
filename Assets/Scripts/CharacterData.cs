using UnityEngine;
using GameTypes;

[CreateAssetMenu(fileName = "CharacterData", menuName = "GameData/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("基本ステータス")]
    [SerializeField] int _maxHp;
    public int MaxHp => _maxHp;

    [SerializeField] int _attack;
    public int Attack => _attack;

    [SerializeField] int _defense;
    public int Defense => _defense;

    [SerializeField] int _healAmount;
    public int HealAmount => _healAmount;

    [SerializeField] CharacterType _characterType;
    public CharacterType CharacterType => _characterType;

    [SerializeField] PassiveType _passiveType;
    public PassiveType PassiveType => _passiveType;

    [SerializeField] BattleUnitType _battleUnitType;
    public BattleUnitType BattleUnitType => _battleUnitType;

    // [Header("見た目のデータ")]
    [SerializeField] TextAsset _characterAttackAnimation;
    public TextAsset CharacterAttackAnimation => _characterAttackAnimation;

    [SerializeField] TextAsset _characterSkillAnimation;
    public TextAsset CharacterSkillAnimation => _characterSkillAnimation;

    [SerializeField] TextAsset _characterIdleAnimation;
    public TextAsset CharacterIdleAnimation => _characterIdleAnimation;

    [SerializeField] TextAsset _characterDamagedAnimation;
    public TextAsset CharacterDamagedAnimation => _characterDamagedAnimation;
    
}
