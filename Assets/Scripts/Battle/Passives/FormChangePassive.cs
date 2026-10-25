using Battle;
using UnityEngine;

/// <summary>
/// スキルを使うと別の形態に切り替わる（システィーナ用）。
/// 発動タイミングは AfterSkillAttack にすること
/// </summary>

[CreateAssetMenu(menuName = "Game/Passives/FormChangePassive")]
public class FormChangePassive : PassiveData
{
    public override void Execute(BattleContext battleContext)
    {
        battleContext.RequestFormChange(battleContext.Attacker); // 入れ替え自体は BattleManager が行う
    }
}