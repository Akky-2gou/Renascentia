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
    /// スキル・パッシブ・ダメージ計算に渡す戦闘情報。戦闘中ずっと1つを使い回すクラス
    /// </summary>
    public class BattleContext
    {
        // 変数
        public CharacterStatus Attacker { get; private set; } // 攻撃者
        public IReadOnlyList<CharacterStatus> Targets { get; private set; } // 標的
        public IReadOnlyCollection<CharacterStatus> CharacterQueue { get; } // 戦闘にいる全キャラ情報
        readonly List<BattleRecord> _history = new List<BattleRecord>(); // 行動履歴
        public IReadOnlyList<BattleRecord> History => _history;     // 行動履歴のプロパティ
        public float NextAttackPowerUp { get; private set; } // 次の攻撃時の追加効果（0 なら効果なし。0.5 なら威力+50%）
        
        /// <summary>
        /// 攻撃者と標的の登録
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="targets"></param>
        public void BeginAction(CharacterStatus attacker, CharacterStatus[] targets)
        {
            Attacker = attacker;
            Targets = targets;
        }

        // 行動の効果を反映した後に呼ぶ
        public void AddNormalAttackHistory(int turn)
            => _history.Add(new BattleRecord(turn, Attacker, Targets, BattleActionType.NormalAttack));

        public void AddSkillHistory(int turn, SkillData skill)
            => _history.Add(new BattleRecord(turn, Attacker, Targets, BattleActionType.Skill, skill));

        // 追加効果を足す（重ねがけは加算）
        public void AddNextAttackPowerUp(float amount)
        {
            NextAttackPowerUp += amount;
        }

        // 追加効果を消す（使い終わったら呼ぶ）
        public void ResetNextAttackPowerUp()
        {
            NextAttackPowerUp = 0f;
        }
        
        // コンストラクタ
        public BattleContext(Queue<CharacterStatus> characterQueue)
        {
            CharacterQueue = characterQueue;
        }
    }
}