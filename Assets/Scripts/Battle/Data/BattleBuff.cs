using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>強化の種類</summary>
    public enum BuffStat
    {
        Power,      // 威力
        CritRate,   // クリティカル率（割合のみ使う）
        CritDamage, // クリティカルダメージ（割合のみ使う）
        None,
    }

    /// <summary>強化の期間</summary>
    public enum BuffDuration
    {
        NextAttack, // 次の行動（通常攻撃またはスキル）が終わるまで
        Permanent,  // 戦闘中ずっと
        None,
    }

    /// <summary>強化1件分。誰に、何を、どれだけ、いつまで</summary>
    public class PendingBuff
    {
        public CharacterStatus Target { get; }   // 強化を受けるキャラ
        public BuffStat Stat { get; }            // 何を上げるか
        public int Stage { get; }                // 段階（ポケモン方式）。割合の強化では 0
        public float Rate { get; }               // 割合（0.1 なら +10%）。段階の強化では 0
        public BuffDuration Duration { get; }    // いつまでか

        public PendingBuff(CharacterStatus target, BuffStat stat, int stage, float rate, BuffDuration duration)
        {
            Target = target;
            Stat = stat;
            Stage = stage;
            Rate = rate;
            Duration = duration;
        }
    }

    /// <summary>1回の行動で使う強化の合計。作成後は変わらない</summary>
    public class BuffTotals
    {
        const int MAX_STAGE = 6; // 段階の上限（±）

        public static readonly BuffTotals None = new BuffTotals(new PendingBuff[0]); // 強化なし

        public float PowerMultiplier { get; }   // 威力にかける倍率
        public float CritRateBonus { get; }     // クリ率に足す値（0.1 なら +10%）
        public float CritDamageBonus { get; }   // クリダメージ倍率に足す値（0.3 なら 1.5 → 1.8）

        public BuffTotals(IEnumerable<PendingBuff> buffs)
        {
            int powerStage = 0;
            float powerRate = 0f;
            float critRate = 0f;
            float critDamage = 0f;

            // 同じ種類の強化を、種類ごとに足し合わせる
            foreach (var buff in buffs)
            {
                switch (buff.Stat)
                {
                    case BuffStat.Power:
                        powerStage += buff.Stage;
                        powerRate += buff.Rate;
                        break;
                    case BuffStat.CritRate:
                        critRate += buff.Rate;
                        break;
                    case BuffStat.CritDamage:
                        critDamage += buff.Rate;
                        break;
                    default:
                        Debug.LogError("不明なバフ");
                        break;
                }
            }

            // 段階は上限に丸めて倍率にし、割合は (1 + 割合) として掛ける
            PowerMultiplier = StageToMultiplier(Mathf.Clamp(powerStage, -MAX_STAGE, MAX_STAGE)) * (1f + powerRate);
            CritRateBonus = critRate;
            CritDamageBonus = critDamage;
        }

        // 0段階=1.0、+1=3/2、+2=4/2、+3=5/2 ... / -1=2/3、-2=2/4 ...
        static float StageToMultiplier(int stage)
            => stage >= 0 ? (2f + stage) / 2f : 2f / (2f - stage);
    }
}