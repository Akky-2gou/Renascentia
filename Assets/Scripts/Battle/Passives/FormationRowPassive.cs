using Battle;
using GameTypes;
using UnityEngine;

/// <summary>
/// 標的に後衛の敵がいるとき、自分に強化を付ける（ひまわり用）
/// 現状だと、全体攻撃の際は無条件で全員に攻撃力アップがつく
/// </summary>

[CreateAssetMenu(menuName = "Game/Passives/FormationRowPassive")]
public class FormationRowPassive : PassiveBase
{
    public override void Execute(BattleContext battleContext)
    {
        if (!HasBackRowTarget(battleContext)) return;
        base.Execute(battleContext); // 強化の付与は PassiveBase に任せる
    }

    // 標的に後衛が1人でもいるか
    private bool HasBackRowTarget(BattleContext battleContext)
    {
        foreach (CharacterStatus target in battleContext.Targets)
        {
            if (target.Row == FormationRow.None)
            {
                Debug.LogError($"{target.name} の前衛後衛が未設定です");
                continue;
            }
            if (target.Row == FormationRow.Back) return true;
        }
        return false;
    }
}