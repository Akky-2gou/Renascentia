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

    [SerializeField] int _normalAttackPower;
    public int NormalAttackPower => _normalAttackPower;

    [SerializeField] int _healAmount;
    public int HealAmount => _healAmount;

    [SerializeField] CharacterType _characterType;
    public CharacterType CharacterType => _characterType;

    [SerializeField] BattleUnitType _battleUnitType;
    public BattleUnitType BattleUnitType => _battleUnitType;

    [SerializeField] ElementType _attackElement; // 通常攻撃のエレメント属性
    public ElementType AttackElement => _attackElement;

    [SerializeField] ElementType[] _weaknesses; // 苦手なエレメント属性（複数可）
    public IReadOnlyList<ElementType> Weaknesses => _weaknesses;

    [Header("HPバー用アイコン")]
    [SerializeField] private Sprite _icon; // キャラのアイコン画像
    public Sprite Icon => _icon;

    [SerializeField] private Vector2 _iconOffset; // 表示位置の補正(中心からのズレ)
    public Vector2 IconOffset => _iconOffset;

    [SerializeField] private float _iconScale = 1f; // 表示スケールの補正(画像ごとの余白の違いを吸収)
    public float IconScale => _iconScale;

    [Header("戦闘勝利時の画像の位置・大きさ")]
    [SerializeField] private Vector2 _winOffset; // 表示位置の補正(中心からのズレ)
    public Vector2 WinOffset => _winOffset;
    [SerializeField] private float _winScale = 1f; // 表示スケールの補正(画像ごとの余白の違いを吸収)
    public float WinScale => _winScale;

    [Header("キャラクターPreafab")]
    [SerializeField] GameObject _characterPrefab;
    public GameObject CharacterPrefab => _characterPrefab;

    [Header("バトルでの大きさの倍率（1 で通常。大きく見せたい敵だけ変える）")]
    [SerializeField, Min(0.1f)] float _battleScaleMultiplier = 1f;
    public float BattleScaleMultiplier => _battleScaleMultiplier;

    [Header("スキル")]
    [SerializeField] SkillData[] _skills;
    public SkillData[] Skills => _skills;

    [Header("パッシブ")]
    [SerializeField] PassiveData _passive;
    public PassiveData Passive => _passive;

    [Header("スキル演出（スキル1 → 演出1、スキル2 → 演出2、スキル3 → 両方）")]
    [SerializeField] SkillEffectSlot _skillEffect1;
    public SkillEffectSlot SkillEffect1 => _skillEffect1;

    [SerializeField] SkillEffectSlot _skillEffect2;
    public SkillEffectSlot SkillEffect2 => _skillEffect2;

    [Header("別の形態（形態変化するキャラのみ設定）")]
    [SerializeField] CharacterData _otherForm;
    public CharacterData OtherForm => _otherForm;

    [Header("敵の行動パターン（敵のみ設定）")]
    [SerializeField] EnemyBehavior _enemyBehavior;
    public EnemyBehavior EnemyBehavior => _enemyBehavior;
}
