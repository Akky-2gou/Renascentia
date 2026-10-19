using Battle;
using UnityEngine;

/// <summary>
/// 直前の味方と違う種類の行動（通常攻撃/スキル）をしたときだけ、
/// 自身の攻撃を強化するパッシブ
/// </summary>

[CreateAssetMenu(menuName = "Game/Passives/DifferentActionPassive")]
public class DifferentActionPassive : PassiveBase
{
    public override void Execute(BattleContext battleContext)
    {
        // 直前の味方の行動を探す。いなければ（戦闘の最初など）何もしない
        if (!TryGetPreviousAllyRecord(battleContext, out var previous)) return;

        // 今の行動が、直前の味方と同じ種類なら何もしない
        if (previous.ActionType == battleContext.ActionType) return;

        base.Execute(battleContext); // 違うときだけ、Inspector で設定した強化を付ける
    }

    /// <summary>
    /// 攻撃者より前に行動した「別の味方」の、いちばん新しい行動記録を探す。いなければ false
    /// </summary>
    private bool TryGetPreviousAllyRecord(BattleContext battleContext, out BattleRecord record)
    {
        var self = battleContext.Attacker;
        var history = battleContext.History;

        for (int i = history.Count - 1; i >= 0; i--) // 新しい方から遡る
        {
            var r = history[i];
            if (r.Attacker == self) continue;                               // 自分の行動は飛ばす
            if (r.Attacker.BattleUnitType != self.BattleUnitType) continue; // 敵の行動は飛ばす

            record = r;
            return true;
        }

        record = null;
        return false;
    }
}