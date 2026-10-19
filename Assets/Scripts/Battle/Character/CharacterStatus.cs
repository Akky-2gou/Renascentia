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


public class CharacterStatus : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    int _maxHp; // 最大ヒットポイント
    int _hp; // ヒットポイント
    int _attack; // 攻撃力
    int _defense; // 防御力
    int _normalAttackPower; // 通常攻撃威力
    int _healAmount; // 回復量
    bool _alive; // 生存判定 
    SkillData[] _skills; // 所持スキル
    PassiveData _passive; // パッシブ

    CharacterHPBar _hpBar; // HPバーの参照

    BattleUnitType _battleUnitType; // バトルユニットのタイプ

    GameTypes.CharacterType _characterType; // キャラクターのタイプ

    ElementType _attackElement; // 通常攻撃のエレメント属性

    IReadOnlyList<ElementType> _weaknesses; // 苦手なエレメント属性

    EnemyBehavior _enemyBehavior; // 敵の行動パターン（敵のみ使う）

    Animator _animator; // キャラクターのアニメーターコントローラー

    float time = 0;

    // マウスクリックされたことを外部に知らせるイベント
    public event Action<CharacterStatus> OnClicked;

    // アニメーションイベントを通知するためのAction
    public event Action OnHitEvent;
    public event Action OnSkillAnimEvent;
    public event Action OnAnimationEndEvent;

    [SerializeField] private GameObject _targetMarkRoot; // 標的マークの画像
    private bool _hoverEnabled = false; // ホバーで標的マークを出してよい状態かどうか

    void Awake()
    {
        if (_targetMarkRoot != null)
        {
            _targetMarkRoot.SetActive(false);
            _hoverEnabled = false;
        }
    }

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
        _normalAttackPower = characterData.NormalAttackPower;
        _healAmount = characterData.HealAmount;
        _battleUnitType = characterData.BattleUnitType;
        _characterType = characterData.CharacterType;
        _hp = _maxHp;
        _alive = true;
        _attackElement = characterData.AttackElement;
        _weaknesses = characterData.Weaknesses;
        _skills = characterData.Skills;
        _passive = characterData.Passive;
        _enemyBehavior = characterData.EnemyBehavior;

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

    public int NormalAttackPower
    {
        get { return _normalAttackPower; }
        set { _normalAttackPower = value; }
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

    // Setし、HPの反映までさせる
    public void SetHpBar(CharacterHPBar hpBar)
    {
        _hpBar = hpBar;
        _hpBar.SetHp(_hp, _maxHp); // 紐付けた瞬間に、今のHPを表示に反映しておく
    }

    public CharacterHPBar HpBar
    {
        get { return _hpBar; }
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

    public SkillData[] Skills => _skills;

    public PassiveData Passive => _passive;

    public ElementType AttackElement => _attackElement;

    public IReadOnlyList<ElementType> Weaknesses => _weaknesses;

    public EnemyBehavior EnemyBehavior => _enemyBehavior;

    /// <summary>指定したエレメント属性が苦手か</summary>
    public bool IsWeakTo(ElementType element)
    {
        if (element == ElementType.None) Debug.LogError("苦手属性が選択されていません");

        foreach (var weakness in _weaknesses)
        {
            if (weakness == element) return true;
        }
        return false;
    }

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

        _hpBar?.SetHp(_hp, _maxHp); // HPバーへの反映

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

        _hpBar?.SetHp(_hp, _maxHp); // HPバーへの反映

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

    // ====================================================
    // ターゲットマークに関する処理
    // ====================================================

    /// <summary>
    /// 単体選択スキルの対象になりうる間だけ、ホバーでの標的マーク表示を許可する
    /// </summary>
    public void SetHoverEnabled(bool enabled)
    {
        _hoverEnabled = enabled;
        if (!enabled)
        {
            HideTargetMark(); // 選択不可になったら、表示中のマークも消す
        }
    }

    // カーソルに乗ったら
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_hoverEnabled) return;
        ShowTargetMark();
    }

    // カーソルが外れたら
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hoverEnabled) return;
        HideTargetMark();
    }

    /// <summary>
    /// 標的マークを表示し、HPバーも広げる
    /// </summary>
    public void ShowTargetMark()
    {
        if (_battleUnitType == BattleUnitType.EndPoint) return;
        _targetMarkRoot.SetActive(true);
        _hpBar?.SetTargeted(true);
    }

    /// <summary>
    /// 標的マークを非表示にし、HPバーも元に戻す
    /// </summary>
    public void HideTargetMark()
    {
        if (_battleUnitType == BattleUnitType.EndPoint) return;
        _targetMarkRoot.SetActive(false);
        _hpBar?.SetTargeted(false);
    }
}
