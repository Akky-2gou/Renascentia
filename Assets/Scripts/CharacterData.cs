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
}
