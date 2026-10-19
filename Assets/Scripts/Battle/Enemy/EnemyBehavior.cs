using System.Collections.Generic;
using UnityEngine;

/*
 * 敵の行動表を取り扱うスクリプト
 */

/// <summary>敵の行動表の1行。使うスキルと、選ばれやすさ（重み）</summary>
[System.Serializable]
public class EnemyActionEntry
{
    [SerializeField] private SkillData _skill; // 使うスキル
    [SerializeField] private int _weight;      // 重み（大きいほど選ばれやすい。0 なら選ばれない）

    public SkillData Skill => _skill;
    public int Weight => _weight;
}

/// <summary>体力が一定以下になったときに切り替わる行動表</summary>
[System.Serializable]
public class EnemyPhase
{
    [SerializeField][Range(0f, 1f)] private float _hpRatioBelow = 0.5f; // この割合以下で使う（0.5 なら体力50%以下）
    [SerializeField] private EnemyActionEntry[] _actions;

    public float HpRatioBelow => _hpRatioBelow;
    public IReadOnlyList<EnemyActionEntry> Actions => _actions;
}

/// <summary>敵の行動パターン。基本の行動表と、体力で切り替わる段階</summary>
[System.Serializable]
public class EnemyBehavior
{
    [SerializeField] private EnemyActionEntry[] _defaultActions; // 基本の行動表
    [SerializeField] private EnemyPhase[] _phases;               // 体力で切り替わる行動表（通常の敵は空でよい）

    /// <summary>今の体力の割合で使う行動表を返す</summary>
    public IReadOnlyList<EnemyActionEntry> GetActions(float hpRatio)
    {
        IReadOnlyList<EnemyActionEntry> result = _defaultActions;
        float lowest = float.MaxValue; // 条件を満たした段階のうち、いちばん低い境目

        foreach (var phase in _phases)
        {
            if (hpRatio <= phase.HpRatioBelow && phase.HpRatioBelow < lowest)
            {
                lowest = phase.HpRatioBelow;
                result = phase.Actions;
            }
        }
        return result;
    }
}