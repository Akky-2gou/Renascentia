using Battle;
using GameTypes;
using UnityEngine;

/*
 * 攻撃したあと、味方のキャラの威力を永続で上げるスキル
 */

[CreateAssetMenu(fileName = "PowerUpAttackSkill", menuName = "Game/Skills/PowerUpAttackSkill")]
public class PowerUpAttackSkill : AttackSkillBase
{
    [SerializeField] private CharacterType _characterType = CharacterType.None;
    [SerializeField] private int _stage = 0; // 上げる段階

    public override void Execute(BattleContext battleContext)
    {
        base.Execute(battleContext); // まずダメージを与える

        // その後、生きている味方の浪漫キャラに、永続の威力アップを付ける
        foreach (var ally in battleContext.CharacterQueue)
        {
            if (ally.BattleUnitType != battleContext.Attacker.BattleUnitType) continue; // 味方だけ（終端ダミーもここで外れる）
            if (!ally.Alive) continue;
            if (ally.CharacterType != _characterType) continue;

            battleContext.AddStageBuff(ally, BuffStat.Power, _stage, BuffDuration.Permanent);
        }
    }
}