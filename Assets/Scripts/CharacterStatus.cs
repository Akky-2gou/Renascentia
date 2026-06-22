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

    BattleUnitType _battleUnitType; // バトルユニットのタイプ

    GameTypes.CharacterType _characterType; // キャラクターのタイプ
    GameTypes.PassiveType _passiveType; // パッシブのタイプ

    SpriteRenderer _spriteRenderer; // キャラクターのスプライトレンダラー

    private List<Sprite> _attackAnimationSprites; 
    private List<Sprite> _damagedAnimationSprites; 
    private List<Sprite> _idleAnimationSprites; 
    private List<Sprite> _skillAnimationSprites;

    private float  _attackAnimationFrameDelay = 0.1f; // 攻撃アニメーションのフレーム遅延を格納する配列
    private float  _damagedAnimationFrameDelay = 0.1f; // ダメージアニメーションのフレーム遅延を格納する配列
    private float  _idleAnimationFrameDelay = 0.1f; // アイドルアニメーションのフレーム遅延を格納する配列
    private float  _skillAnimationFrameDelay = 0.1f; // スキルアニメーションのフレーム遅延を格納する配列

    // マウスクリックされたことを外部に知らせるイベント
    public event Action<CharacterStatus> OnClicked;

    // ★マウスクリック時にUnityが自動で呼んでくれる関数
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " がクリックされました！");
        // 引数として「自分自身(this)」を渡す
        OnClicked?.Invoke(this); 
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

        // スプライトレンダラーを取得する
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // アニメーションデータを取得
        _attackAnimationSprites = characterData.CharacterAttackAnimation;
        _attackAnimationFrameDelay = 0.1f;

        _damagedAnimationSprites = characterData.CharacterDamagedAnimation;
        _damagedAnimationFrameDelay = 0.1f;

        _idleAnimationSprites = characterData.CharacterIdleAnimation;
        _idleAnimationFrameDelay = 0.1f;

        _skillAnimationSprites = characterData.CharacterSkillAnimation;
        _skillAnimationFrameDelay = 0.1f;

        // tokenをメンバ変数にすべきか迷ってるよ
        CancellationTokenSource manualCts = new CancellationTokenSource();
        CancellationToken destroyToken = this.GetCancellationTokenOnDestroy(); // オブジェクトが消えた時にキャンセルされるトークン
        // 2つのトークンを「合成」
        CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(manualCts.Token, destroyToken);
        CancellationToken combinedToken = linkedCts.Token;

        // 初期状態はアイドルアニメーションを再生
        PlayIdleAnimation(combinedToken).Forget();
    }

    // テクスチャをスプライトに変換する関数
    private static Sprite Texture2DtoSprite(Texture2D tex)
        => Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

    // アニメーションデータを読み込む関数
    void ReadAnimationData(List<Sprite> animationData, List<Sprite> animationSprites, float frameDelays)
    {
        animationSprites = animationData;
        frameDelays = 0.1f;

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

        if (_hp < 0)
        {
            _hp = 0; // HPが0未満にならないようにする
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
    public async UniTask PlayAttackAnimation(CancellationToken token)
    {
        await PlayAnimationAsync(_attackAnimationSprites, _attackAnimationFrameDelay, token);
    }

    /// <summary>
    /// 被弾アニメーションを再生する関数
    /// </summary>
    public async UniTask PlayDamagedAnimation(CancellationToken token)
    {
        // ここで待機アニメーションをキャンセル

        await PlayAnimationAsync(_damagedAnimationSprites, _damagedAnimationFrameDelay, token);

        // トークンを再作成し，待機アニメーションを再生(.Forget()を忘れずに！)
    }

    /// <summary>
    /// 待機アニメーションを再生する関数
    /// </summary>
    public async UniTask PlayIdleAnimation(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await PlayAnimationAsync(_idleAnimationSprites, _idleAnimationFrameDelay, token);
        }
    }

    /// <summary>
    /// スキルアニメーションを再生する関数
    /// </summary>
    public async UniTask PlaySkillAnimation(CancellationToken token)
    {
        await PlayAnimationAsync(_skillAnimationSprites, _skillAnimationFrameDelay, token);
    }
    
    // 与えられたアニメーションを再生する非同期関数
    async UniTask PlayAnimationAsync(List<Sprite> animationSprites, float frameDelays, CancellationToken token)
    {
        for (int i = 0; i < animationSprites.Count; i++)
        {
            _spriteRenderer.sprite = animationSprites[i];
            await UniTask.Delay(TimeSpan.FromSeconds(frameDelays), cancellationToken: token);
        }
    }
}
