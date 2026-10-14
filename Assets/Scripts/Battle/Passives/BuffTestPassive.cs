using Battle;
using UnityEngine;

/// <summary>動作確認用：自分に強化を付ける。確認が終わったら消してよい</summary>
[CreateAssetMenu(menuName = "Game/Passives/BuffTest")]
public class BuffTestPassive : PassiveData
{
    [SerializeField] private BuffStat _stat = BuffStat.Power;
    [SerializeField] private int _stage = 1;       // 段階（0 なら付けない）
    [SerializeField] private float _rate = 0f;     // 割合（0 なら付けない）
    [SerializeField] private BuffDuration _duration = BuffDuration.NextAttack;

    public override void Execute(BattleContext battleContext)
    {
        if (_stage != 0) battleContext.AddStageBuff(battleContext.Attacker, _stat, _stage, _duration);
        if (_rate != 0f) battleContext.AddRateBuff(battleContext.Attacker, _stat, _rate, _duration);
    }
}