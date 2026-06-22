using UnityEngine;
using GameTypes;
using System.Collections.Generic;

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
    [SerializeField] List<Sprite> _characterAttackAnimation;
    public List<Sprite> CharacterAttackAnimation => _characterAttackAnimation;

    [SerializeField] List<Sprite> _characterSkillAnimation;
    public List<Sprite> CharacterSkillAnimation => _characterSkillAnimation;

    [SerializeField] List<Sprite> _characterIdleAnimation;
    public List<Sprite> CharacterIdleAnimation => _characterIdleAnimation;

    [SerializeField] List<Sprite> _characterDamagedAnimation;
    public List<Sprite> CharacterDamagedAnimation => _characterDamagedAnimation;
    
}
