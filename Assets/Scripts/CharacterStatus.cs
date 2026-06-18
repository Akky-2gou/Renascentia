using UnityEngine;
using GameTypes;
using System;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

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

    private readonly List<Sprite> _attackAnimationSprites = new List<Sprite>(); // 攻撃アニメーションのフレームを格納する配列
    private readonly List<Sprite> _damagedAnimationSprites = new List<Sprite>(); // ダメージアニメーションのフレームを格納する配列
    private readonly List<Sprite> _idleAnimationSprites = new List<Sprite>(); // アイドルアニメーションのフレームを格納する配列
    private readonly List<Sprite> _skillAnimationSprites = new List<Sprite>(); // スキルアニメーションのフレームを格納する配列

    private readonly List<float>  _attackAnimationFrameDelay = new List<float>(); // 攻撃アニメーションのフレーム遅延を格納する配列
    private readonly List<float>  _damagedAnimationFrameDelay = new List<float>(); // ダメージアニメーションのフレーム遅延を格納する配列
    private readonly List<float>  _idleAnimationFrameDelay = new List<float>(); // アイドルアニメーションのフレーム遅延を格納する配列
    private readonly List<float>  _skillAnimationFrameDelay = new List<float>(); // スキルアニメーションのフレーム遅延を格納する配列

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
        ReadAnimationData(characterData.AttackAnimationData, _attackAnimationSprites, _attackAnimationFrameDelay);
        ReadAnimationData(characterData.DamagedAnimationData, _damagedAnimationSprites, _damagedAnimationFrameDelay);
        ReadAnimationData(characterData.IdleAnimationData, _idleAnimationSprites, _idleAnimationFrameDelay);
        ReadAnimationData(characterData.SkillAnimationData, _skillAnimationSprites, _skillAnimationFrameDelay);

        // 初期状態はアイドルアニメーションの最初のフレームを表示する
        _spriteRenderer.sprite = _idleAnimationSprites[0];
    }

    // アニメーションデータを読み込む関数
    void ReadAnimationData(TextAsset animationData, List<Sprite> animationSprites, List<float> frameDelays)
    {
        using( var decoder = new MG.GIF.Decoder( animationData.bytes ))
        {
            var img = decoder.NextImage();

            while( img != null )
            {
                animationSprites.Add( Texture2DtoSprite(img.CreateTexture()));
                frameDelays.Add(img.Delay/1000.0f);
                img = decoder.NextImage();
            }
        }
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
    public void PlayAnimation(TextAsset animationData)
    {
        _spriteRenderer.sprite = _attackAnimationSprites[0]; // とりあえず攻撃アニメーションの最初のフレームを表示する
    }
}
