using UnityEngine;
using GameTypes;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;
using Live2D.Cubism.Rendering;

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

    private FormationRow _row;          // 前衛か後衛か
    CharacterData _data;            // 今の形態のキャラクターデータ

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

    [Header("死亡演出")]
    [SerializeField, Min(0f)] private float _deathFadeSeconds = 0.8f;       // フェードして消えるまでの秒数
    [SerializeField, Min(0.1f)] private float _deathAnimationTimeout = 5f;  // 死亡アニメーションを待つ最大秒数（保険）

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
    public void Initialize(CharacterData characterData, FormationRow row)
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
        _row = row;
        _data = characterData;

        _animator = GetComponentInChildren<Animator>(); // 自分を先に探し、なければ子オブジェクトから探す
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

    /// <summary>
    /// 形態変化の前のキャラから、HP・生存・HPバーを引き継ぐ（Initialize のあとに呼ぶ）
    /// </summary>
    public void InheritFrom(CharacterStatus previous)
    {
        _hp = Mathf.Min(previous._hp, _maxHp);
        _alive = previous._alive;
        SetHpBar(previous._hpBar); // HPバーを付け替え、今のHPを表示に反映する
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

    public FormationRow Row => _row;

    public CharacterData Data => _data;

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
    // 死亡演出に関する処理
    // ====================================================

    /// <summary>
    /// 死亡演出を行う。終わるまで待てる
    /// 敵: Death アニメーション → フェードして消える
    /// 味方: 被弾アニメーションと同時にフェードして消える
    /// </summary>
    public async UniTask PlayDeathAsync(CancellationToken ct)
    {
        if (_animator == null)
        {
            Debug.LogError($"{name}: Animator が見つからないため、アニメーションなしでフェードします");
            await FadeOutAsync(ct);
            return;
        }

        switch (_battleUnitType)
        {
            case BattleUnitType.Enemy:
                _animator.SetBool("DeathBool", true);
                await WaitDeathAnimationEndAsync(ct);
                await FadeOutAsync(ct);
                break;

            case BattleUnitType.Player:
                PlayDamagedAnimation();
                await FadeOutAsync(ct); // 被弾アニメーションと同時にフェードが進む
                break;

            default:
                Debug.LogError($"{name}: 死亡演出に対応していない種類です: {_battleUnitType}");
                break;
        }
    }

    // Death ステートの再生が最後まで終わるまで待つ（保険として、最大 _deathAnimationTimeout 秒）
    private async UniTask WaitDeathAnimationEndAsync(CancellationToken ct)
    {
        float elapsed = 0f;
        while (elapsed < _deathAnimationTimeout)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death") && !_animator.IsInTransition(0) && state.normalizedTime >= 1f) return;

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        Debug.LogWarning($"{name}: Death アニメーションの終わりを待ちきれなかったので、フェードに進みます");
    }

    // だんだん透明にして消す。Live2D のモデルなら CubismRenderController、そうでなければ SpriteRenderer の α を下げる
    private async UniTask FadeOutAsync(CancellationToken ct)
    {
        CubismRenderController cubism = GetComponentInChildren<CubismRenderController>();
        SpriteRenderer[] sprites = cubism == null ? GetComponentsInChildren<SpriteRenderer>() : new SpriteRenderer[0];

        if (cubism == null && sprites.Length == 0)
        {
            Debug.LogError($"{name}: フェードさせる見た目（CubismRenderController / SpriteRenderer）が見つかりません");
            return;
        }

        // SpriteRenderer は、もとの α を覚えておいて、そこから 0 へ下げる
        float[] startAlphas = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) startAlphas[i] = sprites[i].color.a;
        float cubismStart = cubism != null ? cubism.Opacity : 1f;

        float elapsed = 0f;
        while (elapsed < _deathFadeSeconds)
        {
            elapsed += Time.deltaTime;
            float rate = 1f - Mathf.Clamp01(elapsed / _deathFadeSeconds); // 1 → 0

            if (cubism != null) cubism.Opacity = cubismStart * rate;
            for (int i = 0; i < sprites.Length; i++)
            {
                Color c = sprites[i].color;
                c.a = startAlphas[i] * rate;
                sprites[i].color = c;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        if (cubism != null) cubism.Opacity = 0f;
        for (int i = 0; i < sprites.Length; i++)
        {
            Color c = sprites[i].color;
            c.a = 0f;
            sprites[i].color = c;
        }
    }

    // ====================================================
    // スキルエフェクトのアニメーションに関する処理
    // ====================================================

    /// <summary>
    /// スキルの演出を、自分の位置（＋ずれ）に生成する。コスト1 → 演出1、2 → 演出2、3 → 両方
    /// 生成した演出を返す（設定が足りないものはエラーを出して飛ばす）
    /// </summary>
    public List<SkillEffect> SpawnSkillEffects(int cost)
    {
        var effects = new List<SkillEffect>();

        if (_data == null)
        {
            Debug.LogError($"{name}: CharacterData が設定されていないため、スキル演出を出せません");
            return effects;
        }

        switch (cost)
        {
            case 1:
                TrySpawnSkillEffect(_data.SkillEffect1, effects);
                break;
            case 2:
                TrySpawnSkillEffect(_data.SkillEffect2, effects);
                break;
            case 3:
                TrySpawnSkillEffect(_data.SkillEffect1, effects);
                TrySpawnSkillEffect(_data.SkillEffect2, effects);
                break;
            default:
                Debug.LogError($"{_data.name}: コスト{cost}のスキル演出はありません");
                break;
        }
        return effects;
    }

    // 演出を1つ生成して、リストに入れる
    private void TrySpawnSkillEffect(SkillEffectSlot slot, List<SkillEffect> effects)
    {
        if (slot == null || slot.Prefab == null)
        {
            Debug.LogError($"{_data.name}: スキル演出のプレハブが設定されていません");
            return;
        }

        Vector3 position = transform.position + (Vector3)slot.Offset;
        SkillEffect effect = Instantiate(slot.Prefab, position, Quaternion.identity);

        // 倍率が 0 だと見えなくなるので、設定ミスとして知らせて、そのままの大きさで出す
        Vector2 scale = slot.Scale;
        if (scale.x == 0f || scale.y == 0f)
        {
            Debug.LogWarning($"{_data.name}: スキル演出の倍率が 0 です。倍率 1 で表示します");
            scale = Vector2.one;
        }

        Vector3 baseScale = effect.transform.localScale; // プレハブ本来の大きさ
        effect.transform.localScale = new Vector3(baseScale.x * scale.x, baseScale.y * scale.y, baseScale.z);

        effects.Add(effect);
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
