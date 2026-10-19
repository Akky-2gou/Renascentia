using Battle;
using UnityEngine;

/// <summary>動作確認用：自分に強化を付ける。確認が終わったら消してよい</summary>
[CreateAssetMenu(menuName = "Game/Passives/PassiveBase")]
public class PassiveBase : PassiveData
{
    [SerializeField] private BuffStat _stat = BuffStat.None;
    [SerializeField] private int _stage = 0;       // 段階（0 なら付けない）
    [SerializeField] private float _rate = 0f;     // 割合（0 なら付けない）
    [SerializeField] private BuffDuration _duration = BuffDuration.None;

    public override void Execute(BattleContext battleContext)
    {
        if (_stage != 0) battleContext.AddStageBuff(battleContext.Attacker, _stat, _stage, _duration);
        if (_rate != 0f) battleContext.AddRateBuff(battleContext.Attacker, _stat, _rate, _duration);
    }
}