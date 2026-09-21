using UnityEngine;

/*
 *  敵全体攻撃スキルのScriptableObject作成処理
 */

[CreateAssetMenu(menuName = "Game/Skills/AllAttack")]
public class AllAttackSkill : AttackSkillBase
{
    void Reset() => _targetType = SkillTargetType.None; // アセット作成時のデフォルト設定
}