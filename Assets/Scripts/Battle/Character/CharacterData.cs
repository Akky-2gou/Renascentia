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

    [SerializeField] int _normalAttackPower;
    public int NormalAttackPower => _normalAttackPower;

    [SerializeField] int _healAmount;
    public int HealAmount => _healAmount;

    [SerializeField] CharacterType _characterType;
    public CharacterType CharacterType => _characterType;

    [SerializeField] PassiveType _passiveType;
    public PassiveType PassiveType => _passiveType;

    [SerializeField] BattleUnitType _battleUnitType;
    public BattleUnitType BattleUnitType => _battleUnitType;

    [Header("HPバー用アイコン")]
    [SerializeField] private Sprite _icon; // キャラのアイコン画像
    public Sprite Icon => _icon;

    [SerializeField] private Vector2 _iconOffset; // 表示位置の補正(中心からのズレ)
    public Vector2 IconOffset => _iconOffset;

    [SerializeField] private float _iconScale = 1f; // 表示スケールの補正(画像ごとの余白の違いを吸収)
    public float IconScale => _iconScale;

    [Header("キャラクターPreafab")]
    [SerializeField] GameObject _characterPrefab;
    public GameObject CharacterPrefab => _characterPrefab;

    [Header("スキル")]
    [SerializeField] SkillData[] _skills;
    public SkillData[] Skills => _skills;
}
