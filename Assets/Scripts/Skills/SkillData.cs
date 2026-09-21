using UnityEngine;

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
    [SerializeField][TextArea] private string _description; // スキル説明

    [SerializeField] protected SkillTargetType _targetType; // 対象範囲

    // プロパティ
    public string SkillName => _skillName;
    public int Cost => _cost;
    public string Description => _description;
    public SkillTargetType TargetType => _targetType;

    // 実際の効果。targetsはBattleManager側で TargetType に応じて集めたものを渡す
    public abstract void Execute(CharacterStatus user, CharacterStatus[] targets);
}