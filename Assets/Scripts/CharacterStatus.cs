using UnityEngine;
using GameTypes;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;

/*
 * キャラクターのステータスを管理するクラス
 */


public class CharacterStatus : MonoBehaviour, IPointerClickHandler
{
    int _maxHp; // 最大ヒットポイント
    int _hp; // ヒットポイント
    int _attack; // 攻撃力
    int _defense; // 防御力
    int _healAmount; // 回復量
    bool _alive; // 生存判定 
    SkillData[] _skills; // 所持スキル

    BattleUnitType _battleUnitType; // バトルユニットのタイプ

    GameTypes.CharacterType _characterType; // キャラクターのタイプ
    GameTypes.PassiveType _passiveType; // パッシブのタイプ

    Animator _animator; // キャラクターのアニメーターコントローラー

    float time = 0;

    // マウスクリックされたことを外部に知らせるイベント
    public event Action<CharacterStatus> OnClicked;

    // アニメーションイベントを通知するためのAction
    public event Action OnHitEvent;
    public event Action OnSkillAnimEvent;
    public event Action OnAnimationEndEvent;

    /// <summary>
    /// マウスクリック時にUnityが自動で呼んでくれる関数
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 引数として「自分自身(this)」を渡す
        OnClicked?.Invoke(this); 
    }

    /// <summary>
    /// UnityのAnimation Eventから呼ばれる関数（ヒットタイミング）
    /// </summary>
    public void AnimEvent_OnHit()
    {
        OnHitEvent?.Invoke();
    }

    /// <summary>
    /// UnityのAnimation Eventから呼ばれる関数（スキルアニメーションタイミング）
    /// </summary>
    public void AnimEvent_OnSkillAnim()
    {
        OnSkillAnimEvent?.Invoke();
    }

    /// <summary>
    /// UnityのAnimation Eventから呼ばれる関数（アニメーション終了）
    /// </summary>
    public void AnimEvent_OnAnimationEnd()
    {
        OnAnimationEndEvent?.Invoke();
    }

    /// <summary>
    /// ステータスを初期化する処理
    /// </summary>
    public void Initialize(CharacterData characterData)
    {
        // キャラクターデータからステータスを初期化する
        _maxHp = characterData.MaxHp;
        _attack = characterData.Attack;
        _defense = characterData.Defense;
        _healAmount = characterData.HealAmount;
        _battleUnitType = characterData.BattleUnitType;
        _characterType = characterData.CharacterType;
        _passiveType = characterData.PassiveType;
        _hp = _maxHp;
        _alive = true;
        _skills = characterData.Skills;

        _animator = this.GetComponent<Animator>();
    }

    public int Hp
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public int MaxHp
    {
        get { return _maxHp; }
    }

    public int Attack
    {
        get { return _attack; }
        set { _attack = value; }
    }

    public int Defense
    {
        get { return _defense; }
        set { _defense = value; }
    }

    public int HealAmount
    {
        get { return _healAmount; }
        set { _healAmount = value; }
    }

    public bool Alive
    {
        get { return _alive; }
        set { _alive = value; }
    }

    public BattleUnitType BattleUnitType
    {
        get { return _battleUnitType; }
        set { _battleUnitType = value; }
    }

    public CharacterType CharacterType
    {
        get { return _characterType; }
        set { _characterType = value; }
    }

    public PassiveType PassiveType
    {
        get { return _passiveType; }
        set { _passiveType = value; }
    }

    public SkillData[] Skills => _skills;

    /// <summary>
    /// ダメージを受けるときにHPを減らす関数
    /// </summary>
    public void DecreaseHp(int damage)
    {
        if (damage < 0)
        {
            return; // ダメージが負の値の場合はHPを変更せずに返す
        }

        _hp -= damage;

        if (_hp <= 0)
        {
            _hp = 0; // HPが0未満にならないようにする
            _alive = false; // 死んだ判定にする
        }

        return;
    }

    /// <summary>
    /// HPを回復する関数
    /// </summary>
    public void IncreaseHp(int heal)
    {
        if (heal < 0)
        {
            return; // 回復量が負の値の場合はHPを変更せずに返す
        }

        _hp += heal;

        if (_hp > _maxHp)
        {
            _hp = _maxHp; // HPが最大HPを超えないようにする
        }

        return;
    }

    /// <summary>
    /// 攻撃アニメーションを再生する関数
    /// </summary>
    public void PlayAttackAnimation()
    {
        Debug.Log("攻撃アニメーション再生");
        _animator.SetTrigger("AttackTrigger");
    }

    /// <summary>
    /// 被弾アニメーションを再生する関数
    /// </summary>
    public void PlayDamagedAnimation()
    {
        Debug.Log("被弾アニメーション再生");
        _animator.SetTrigger("DamagedTrigger");
    }

    /// <summary>
    /// 待機アニメーションを再生する関数
    /// </summary>
    public void PlayIdleAnimation()
    {
        // 待機アニメーションを再生する処理
    }

    /// <summary>
    /// スキルアニメーションを再生する関数
    /// </summary>
    public void PlaySkillAnimation()
    {
        Debug.Log("スキルアニメーション再生");
        _animator.SetTrigger("SkillTrigger");
    }
}
