using System.Collections.Generic;
using UnityEngine;
using GameTypes;

/*
 * 指定した敵のHPが、最大HPの指定割合以下になったら勝利
 */

[CreateAssetMenu(menuName = "Game/Encounter/VictoryCondition/BossHp")]
public class BossHpVictoryCondition : VictoryCondition
{
    [SerializeField] private CharacterData _boss;                  // 対象の敵
    [SerializeField, Range(0f, 1f)] private float _hpRatio = 0.5f; // この割合以下で勝利（0.5 なら半分以下）

    public override bool IsMet(IReadOnlyCollection<CharacterStatus> characters)
    {
        if (_boss == null)
        {
            Debug.LogError($"{name}: 対象の敵が設定されていません");
            return false;
        }

        foreach (CharacterStatus character in characters)
        {
            if (character.BattleUnitType != BattleUnitType.Enemy) continue; // 味方と終端ダミーは見ない
            if (character.Data != _boss) continue;

            return character.Hp <= character.MaxHp * _hpRatio;
        }

        return false; // 対象の敵がいない
    }
}