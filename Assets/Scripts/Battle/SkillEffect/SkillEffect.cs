using System;
using UnityEngine;

/*
 * スキルの演出プレハブに付ける
 * 生成されると、Animator の最初のステートが自動で再生される
 * アニメーションの Animation Event から AnimEvent_OnHit / AnimEvent_OnAnimationEnd を呼ぶ
 * 終わったら自分を消す
 */

[RequireComponent(typeof(Animator))] // Animation Event は Animator と同じオブジェクトのスクリプトにしか届かないため
public class SkillEffect : MonoBehaviour
{
    public event Action HitEvent; // ヒットのタイミング
    public event Action EndEvent; // 演出の終わり

    /// <summary>Animation Event から呼ばれる（ヒットのタイミング）</summary>
    public void AnimEvent_OnHit()
    {
        HitEvent?.Invoke();
    }

    /// <summary>Animation Event から呼ばれる（演出の終わり）。知らせたあと、自分を消す</summary>
    public void AnimEvent_OnAnimationEnd()
    {
        EndEvent?.Invoke();
        Destroy(gameObject);
    }
}

/// <summary>スキル演出1つ分の設定。プレハブと、キャラの位置からのずれ</summary>
[System.Serializable]
public class SkillEffectSlot
{
    [SerializeField] SkillEffect _prefab;              // 演出のプレハブ（SkillEffect が付いているもの）
    [SerializeField] Vector2 _offset;                  // キャラの位置からのずれ（ワールド座標）
    [SerializeField] Vector2 _scale = Vector2.one;     // プレハブ本来の大きさに掛ける倍率（1 でそのまま）

    public SkillEffect Prefab => _prefab;
    public Vector2 Offset => _offset;
    public Vector2 Scale => _scale;
}