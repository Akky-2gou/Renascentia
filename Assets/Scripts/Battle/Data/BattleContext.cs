using System;
using System.Collections.Generic;

/// <summary>
/// バトル情報を保存するクラス
/// </summary>

namespace Battle
{
    public enum BattleActionType
    {
        NormalAttack,
        Skill,
        None,
    }

    /// <summary>
    /// 1回の行動の記録（履歴1件）。作成後は変更できないクラス
    /// </summary>
    public class BattleRecord
    {
        public SkillData Skill { get; } // 使用スキル
        public int Turn { get; } // ターン
        public CharacterStatus Attacker { get; } // 攻撃者
        public IReadOnlyList<CharacterStatus> Targets { get; } // 標的
        public BattleActionType ActionType { get; } // 行動種類

        // コンストラクタ
        public BattleRecord(int turn, CharacterStatus attacker, IReadOnlyList<CharacterStatus> targets,
                     BattleActionType actionType, SkillData skill = null)
        {
            Turn = turn;
            Attacker = attacker;
            Targets = targets;
            ActionType = actionType;
            Skill = skill;
        }
    }

    /// <summary>
    /// 「攻撃者→1体」の1回分のダメージ計算に使う情報
    /// </summary>
    public class DamageContext
    {
        public CharacterStatus Attacker { get; }
        public CharacterStatus Defender { get; }
        public float Power { get; }

        public DamageContext(CharacterStatus attacker, CharacterStatus defender, float power)
        {
            Attacker = attacker;
            Defender = defender;
            Power = power;
        }
    }

    /// <summary>
    /// スキル・パッシブ・ダメージ計算に渡す戦闘情報。戦闘中ずっと1つを使い回すクラス
    /// </summary>
    public class BattleContext
    {
        // 変数
        public CharacterStatus Attacker { get; private set; } // 攻撃者
        public IReadOnlyList<CharacterStatus> Targets { get; private set; } // 標的
        public IReadOnlyCollection<CharacterStatus> CharacterQueue { get; } // 戦闘にいる全キャラ情報
        public BattleActionType ActionType { get; private set; } // 行動種類
        public SkillData Skill { get; private set;} // スキル
        public int Turn { get; private set; } // ターン
        readonly List<BattleRecord> _history = new List<BattleRecord>(); // 行動履歴
        public IReadOnlyList<BattleRecord> History => _history;     // 行動履歴のプロパティ
        readonly List<PendingBuff> _buffs = new List<PendingBuff>();       // 戦闘中に付いている強化すべて（一時的なものも永続も）
        readonly List<PendingBuff> _lockedBuffs = new List<PendingBuff>(); // 今回の行動で使うと確定した強化
        public BuffTotals CurrentBuffs { get; private set; } = BuffTotals.None; // 今回の行動で使う強化の合計（ダメージ計算が読む）
        
        /// <summary>
        /// 通常攻撃時のcontext登録
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="targets"></param>
        /// <param name="turn"></param>
        public void BeginNormalAction(CharacterStatus attacker, CharacterStatus[] targets, int turn)
        {
            Attacker = attacker;
            Targets = targets;
            Turn = turn;
            ActionType = BattleActionType.NormalAttack;
            Skill = null;
        }

        /// <summary>
        /// スキル攻撃時のcontext登録
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="targets"></param>
        /// <param name="skill"></param>
        /// <param name="turn"></param>
        public void BeginSkillAction(CharacterStatus attacker, CharacterStatus[] targets, SkillData skill, int turn)
        {
            Attacker = attacker;
            Targets = targets;
            Turn = turn;
            ActionType = BattleActionType.Skill;
            Skill = skill;
        }

        // 行動の効果を反映した後に呼ぶ
        public void AddNormalAttackHistory(int turn)
            => _history.Add(new BattleRecord(turn, Attacker, Targets, BattleActionType.NormalAttack));

        public void AddSkillHistory(int turn, SkillData skill)
            => _history.Add(new BattleRecord(turn, Attacker, Targets, BattleActionType.Skill, skill));

        /// <summary>段階の強化を付ける（パッシブやスキル効果から呼ぶ）。stage は +1 で 3/2 倍</summary>
        public void AddStageBuff(CharacterStatus target, BuffStat stat, int stage, BuffDuration duration)
        {
            _buffs.Add(new PendingBuff(target, stat, stage, 0f, duration));
        }

        /// <summary>割合の強化を付ける（パッシブやスキル効果から呼ぶ）。rate が 0.1 なら +10%</summary>
        public void AddRateBuff(CharacterStatus target, BuffStat stat, float rate, BuffDuration duration)
        {
            _buffs.Add(new PendingBuff(target, stat, 0, rate, duration));
        }

        /// <summary>
        /// 攻撃者に付いている強化を、今回の行動用に確定する。
        /// 行動前パッシブが終わったあと、ダメージ計算の前に BattleManager が呼ぶ
        /// </summary>
        public void LockBuffs()
        {
            _lockedBuffs.Clear();
            foreach (var buff in _buffs)
            {
                if (buff.Target == Attacker) _lockedBuffs.Add(buff); // 一時的なものも永続も、攻撃者のものを全部拾う
            }
            CurrentBuffs = new BuffTotals(_lockedBuffs); // 合計を計算して固定する
        }

        /// <summary>
        /// 今回の行動で使った強化のうち、「次の攻撃だけ」のものを消す。
        /// 標的全員のダメージ処理が終わったあとに、BattleManager が1回だけ呼ぶ
        /// </summary>
        public void ConsumeLockedBuffs()
        {
            foreach (var buff in _lockedBuffs)
            {
                if (buff.Duration == BuffDuration.NextAttack) _buffs.Remove(buff); // 永続は残る
            }
            _lockedBuffs.Clear();
            CurrentBuffs = BuffTotals.None; // 行動外では強化なしに戻しておく
        }
        
        // コンストラクタ
        public BattleContext(Queue<CharacterStatus> characterQueue)
        {
            CharacterQueue = characterQueue;
        }
    }
}