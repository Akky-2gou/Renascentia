using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

/*
 * 戦闘開始時の演出を管理するスクリプト
 */

public class BattleStartPresentationManager : MonoBehaviour
{
    [Header("味方が歩く距離・時間")]
    [SerializeField] private Vector2 _allyWalkingVector;
    [SerializeField] private float _allyWalkingDuration;

    [Header("カメラ")]
    [SerializeField] private GameObject _cameraObj;
    [SerializeField] private Transform _camera;

    [Header("バトルで利用するキャンバス")]
    [SerializeField] private Canvas _duringBattleCanvas;

    [Header("「戦闘開始」の文字のリスト")]
    [SerializeField] private List<UIAnimationInCanvas> _battleStartChrs;

    [Header("黒背景")]
    [SerializeField] private UIAnimationInCanvas _backGround;

    private GameObject[] _allyCharacters;

    // 必要なコンポーネント
    private List<Transform> _allyTransforms = new List<Transform>();
    private List<Animator> _allyAnimators = new List<Animator>();


    /// <summary>
    /// バトル開始演出のセットアップを行う
    /// </summary>
    public void Setup(GameObject[] allyCharacters)
    {
        // 空スロット（null）を除いた、実際にいる味方だけを集める
        List<GameObject> presentAllies = new List<GameObject>();
        foreach (GameObject obj in allyCharacters)
        {
            if (obj != null) presentAllies.Add(obj);
        }

        // 全部 null のときだけエラーにする
        if (presentAllies.Count == 0)
        {
            Debug.LogError("BattleStartPresentationManager: 味方が1人もいません");
        }

        // ButtleStart で Transform / Animator と同じ添字で使えるように、null を除いた配列を保持する
        _allyCharacters = presentAllies.ToArray();

        // 各味方キャラTransformとAnimatorを取得
        foreach (GameObject obj in _allyCharacters)
        {

            Transform transform = obj.GetComponent<Transform>();
            if (transform == null)
            {
                Debug.LogError($"BattleStartPresentationManager: {obj.name}にTransformがアタッチされていません");
            }
            _allyTransforms.Add(transform);
            
            Animator animator = obj.GetComponent<Animator>();
            if (transform == null)
            {
                Debug.LogError($"BattleStartPresentationManager: {obj.name}にAnimatorがアタッチされていません");
            }
            _allyAnimators.Add(animator);
        }

        // バトル中のUIは見た目を消す
        _duringBattleCanvas.enabled = false;
    }


    /// <summary>
    /// バトル開始演出を行う
    /// </summary>
    public async UniTask BattleStart()
    {
        // 歩き始める位置に移動
        List<Vector3> targetPos = new List<Vector3>(_allyTransforms.Count);
        foreach (Transform transform in _allyTransforms) {
            targetPos.Add(transform.position);
            transform.position -= new Vector3(_allyWalkingVector.x, _allyWalkingVector.y, 0);
        }

        // カメラも移動
        Vector3 cameraTargetPos = _camera.position;
        _camera.position -= new Vector3(_allyWalkingVector.x, _allyWalkingVector.y, 0);

        // 見た目の歩行アニメーション開始
        foreach (Animator animator in _allyAnimators) {
            animator.SetBool("WalkBool", true);
        }

        // 歩行アニメーションの設定
        List<UniTask> animations = new List<UniTask>(_allyTransforms.Count);
        for (int i = 0; i < _allyTransforms.Count; i++) {
            animations.Add(DOTween.Sequence()
                           .Join(_allyTransforms[i].DOMove(targetPos[i], _allyWalkingDuration))
                           .SetLink(_allyCharacters[i]) // オブジェクトが消えたら一緒に消す
                           .SetLink(this.gameObject) // オブジェクトが消えたら一緒に消す
                           .SetEase(Ease.OutSine) // イージング設定
                           .AsyncWaitForCompletion() // 完了まで待つ
                           .AsUniTask()); // Unitask型に変換
        }

        // カメラアニメーションの設定
        animations.Add(DOTween.Sequence()
                       .Join(_camera.DOMove(cameraTargetPos, _allyWalkingDuration))
                       .SetLink(_cameraObj) // オブジェクトが消えたら一緒に消す
                       .SetLink(this.gameObject) // オブジェクトが消えたら一緒に消す
                       .SetEase(Ease.OutSine) // イージング設定
                       .AsyncWaitForCompletion() // 完了まで待つ
                       .AsUniTask()); // Unitask型に変換

        // 全ての歩行アニメーション終了まで待つ
        await UniTask.WhenAll(animations);

        // 見た目の歩行アニメーション終了
        foreach (Animator animator in _allyAnimators) {
            animator.SetBool("WalkBool", false);
        }
        
        // リリックモーションを開始し，終了まで待つ
        animations = new List<UniTask>(_battleStartChrs.Count);
        foreach (UIAnimationInCanvas uIAnimationInCanvas in _battleStartChrs)
        {
            animations.Add(uIAnimationInCanvas.StartAnim(this.destroyCancellationToken));
        }
        await UniTask.WhenAll(animations);
        
        // 黒背景・文字のアニメーションを開始し，終了まで待つ
        animations = new List<UniTask>(_battleStartChrs.Count + 1);
        animations.Add(_backGround.StartAnim(this.destroyCancellationToken));
        foreach (UIAnimationInCanvas uIAnimationInCanvas in _battleStartChrs)
        {
            animations.Add(uIAnimationInCanvas.ReturnAnim(this.destroyCancellationToken));
        }
        await UniTask.WhenAll(animations);

        // バトル中のUIを出現させる
        _duringBattleCanvas.enabled = true;
    }
}
