using UnityEngine;

/*
 *  敵単体攻撃スキルのScriptableObject作成処理
 */

[CreateAssetMenu(menuName = "Game/Skills/SingleAttack")]
public class SingleAttackSkill : AttackSkillBase
{
    void Reset() => _targetType = SkillTargetType.None; // アセット作成時のデフォルト設定
}
