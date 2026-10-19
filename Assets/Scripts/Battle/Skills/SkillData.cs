using Battle;
using UnityEngine;
using GameTypes;

/*
 * スキルのタイプを表す列挙型の名前空間
 */

/// <summary>
/// 対象範囲
/// </summary>
public enum SkillTargetType
{
    SingleEnemy,  // 敵1体を選択
    AllEnemies,   // 敵全体
    SingleAlly,   // 味方1体を選択
    AllAllies,    // 味方全体
    Self,         // 自分のみ
    None,         // 初期化値
}

public abstract class SkillData : ScriptableObject
{
    // 確変数
    [SerializeField] private string _skillName; // スキル名
    [SerializeField] private int _cost; // 消費SP
    public ElementType Element { get; }

    [SerializeField][TextArea] private string _description; // スキル説明

    [SerializeField] protected SkillTargetType _targetType = SkillTargetType.None; // 対象範囲

    // プロパティ
    public string SkillName => _skillName;
    public int Cost => _cost;
    public string Description => _description;
    public SkillTargetType TargetType => _targetType;

    // 発動関数
    public abstract void Execute(BattleContext battleContext);

    // ダメージ計算の前に呼ばれる。その攻撃に乗せたい強化の付与などは、ここに書く（標準は何もしない）
    public virtual void BeforeDamage(BattleContext battleContext) { }
}