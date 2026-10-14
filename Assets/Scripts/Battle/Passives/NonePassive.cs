using Battle;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passives/None")]
public class NonePassive : PassiveData
{
    public override void Execute(BattleContext battleContext) { } // 何もしない
}