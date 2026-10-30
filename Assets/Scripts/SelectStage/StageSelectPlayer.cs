using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * ステージ選択画面でのプレイヤーキャラクターの移動を制御するスクリプト
 */
public class StageSelectPlayer : MonoBehaviour
{
    [Serializable]
    public class StageTarget
    {
        public MapId stageId;
        public Transform marker;
        public Vector3 offset;
    }

    [SerializeField] private List<StageTarget> stageTargets = new List<StageTarget>();
    [SerializeField] private MapId initialStageId = MapId.Map1_1;
    [Header("移動速度設定")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField, Range(0.1f, 3f)] private float moveSpeedScale = 1f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private Transform moveTarget;
    [Header("位置合わせ設定")]
    [SerializeField] private Vector3 pivotToFootOffset = Vector3.zero;
    [SerializeField] private string battleSceneName = "MapToBattle";
    [SerializeField] private ConfirmDialogController _battleConfirm;
    [SerializeField] private int _seId = 3; // 歩きSEのID

    private Rigidbody2D targetRb;
    private readonly List<int> pendingRouteIndices = new List<int>();
    private int currentIndex;
    private MapId currentStageId = MapId.None;
    private int selectedIndex;
    private bool isMoving;
    private Vector3 currentTargetPosition;
    private Coroutine movementCoroutine;
    private List<Animator> _playerAnimator; // Animatorを保持する変数
    private GroupChangeVisualDirection _groupChangeVisualDirection; // GroupChangeVisualDirectionを保持する変数

    public bool IsMoving => isMoving;
    public MapId CurrentStageId => currentStageId;
    public float MoveSpeed => moveSpeed * moveSpeedScale;
    private bool _isOpeningDialogue = false;
    public bool IsOpeningDialogue { set{_isOpeningDialogue = value;} get{return _isOpeningDialogue;} }

    private int _dialogueClosedFrame = -1; // ダイアログを閉じたフレーム

    public MapId SelectedStageId
    {
        get
        {
            if (!HasValidSelection())
            {
                return MapId.None;
            }

            return stageTargets[selectedIndex].stageId;
        }
    }

    /// <summary>ダイアログを閉じたときに呼ぶ（IsOpeningDialogue を直接 false にする代わり）</summary>
    public void NotifyDialogueClosed()
    {
        IsOpeningDialogue = false;
        _dialogueClosedFrame = Time.frameCount;
    }

    // 決定の入力を受け付けてよいか
    public bool CanAcceptSubmit()
    {
        if (IsOpeningDialogue) return false;
        if (Time.frameCount == _dialogueClosedFrame) return false; // 閉じたのと同じ入力で、開き直さない
        return true;
    }

    /// <summary>
    /// プレイヤーの見た目を制御するコンポーネントの取得
    /// </summary>
    public void SetPlayerVisualComponet(List<GameObject> playersObj, List<Animator> animator)
    {
        if (_groupChangeVisualDirection == null)
        {
            Debug.Log("PlayerController: GroupChangeVisualDirectionがnullのため，処理を中断");
            return;
        }

        // プレイヤーとなるメインキャラとそれにくっつくサブキャラクターを登録
        _groupChangeVisualDirection.UpdateMainCharacter(playersObj[0]);
        _groupChangeVisualDirection.ClearSubCharacters();
        for (int i = 1; i < playersObj.Count; i++) _groupChangeVisualDirection.AddSubCharacters(playersObj[i]);

        // 各キャラのAnimatorを取得
        _playerAnimator = animator;
        foreach (Animator anim in _playerAnimator) anim.Rebind(); 
    }

    // 移動用の初期状態を設定する
    private void Awake()
    {
        if (moveTarget == null)
        {
            Debug.LogError("StageSelectPlayer の moveTarget が未設定です。移動させたいキャラクターを割り当ててください。");
            enabled = false;
            return;
        }

        targetRb = moveTarget.GetComponent<Rigidbody2D>();

        currentIndex = GetStageIndexById(initialStageId);
        if (currentIndex < 0)
        {
            currentIndex = 0;
            Debug.LogWarning("初期ステージIDが見つからないため、先頭のノードを初期位置として扱います: " + initialStageId);
        }

        selectedIndex = currentIndex;
        ClampSelection();
        PrepareTargetPositionForIndex(currentIndex);
        currentStageId = GetStageId(currentIndex);
        Debug.Log("初期ノード: " + currentStageId);
        
        _groupChangeVisualDirection = GetComponent<GroupChangeVisualDirection>(); // GroupChangeVisualDirectionを取得
        if (_groupChangeVisualDirection == null)
        {
            Debug.LogError("GroupChangeVisualDirectionがアタッチされていません");
        }
        else
        {
            Debug.Log("GroupChangeVisualDirection取得成功");
        }
    }

    // 移動中のキャラクター位置を更新する（Updateは使わずコルーチンで実行）
    private IEnumerator MoveRouteCoroutine()
    {
        while (isMoving && pendingRouteIndices.Count > 0 && HasValidSelection())
        {
            Vector3 currentPosition = moveTarget.position;
            float step = MoveSpeed * Time.fixedDeltaTime;
            float stopDistance = Mathf.Max(arriveDistance, step);
            float remainingDistance = Vector3.Distance(currentPosition, currentTargetPosition);

            // 最後だけ吸い込まれる見え方を防ぐため、1FixedUpdate分で届くときはそのまま到達扱いにする
            if (remainingDistance <= stopDistance)
            {
                if (targetRb != null)
                {
                    targetRb.linearVelocity = Vector2.zero;
                    targetRb.MovePosition(currentTargetPosition);
                }
                else
                {
                    moveTarget.position = currentTargetPosition;
                }

                AdvanceRoute();
                if (!isMoving)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
                continue;
            }

            Vector3 nextPosition = Vector3.MoveTowards(currentPosition, currentTargetPosition, step);

            if (targetRb != null)
            {
                targetRb.MovePosition(nextPosition);
            }
            else
            {
                moveTarget.position = nextPosition;
            }

            yield return new WaitForFixedUpdate();
        }

        movementCoroutine = null;
    }

    /// <summary>
    /// 基準移動速度を設定する（0以下は0.01として扱う）
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.01f, speed);
    }

    /// <summary>
    /// 移動速度係数を設定する（0以下は0.01として扱う）
    /// </summary>
    public void SetMoveSpeedScale(float scale)
    {
        moveSpeedScale = Mathf.Max(0.01f, scale);
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        moveSpeedScale = Mathf.Max(0.01f, moveSpeedScale);
        arriveDistance = Mathf.Max(0f, arriveDistance);
    }

    // 指定インデックスのステージを選択する
    public void SelectStage(int index)
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, stageTargets.Count - 1);
        PrepareTargetPosition();
    }

    // 次のステージを選択する
    public void SelectNextStage()
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex++;
        if (selectedIndex >= stageTargets.Count)
        {
            selectedIndex = 0;
        }

        PrepareTargetPosition();
    }

    // 前のステージを選択する
    public void SelectPreviousStage()
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = stageTargets.Count - 1;
        }

        PrepareTargetPosition();
    }

    // 選択中のステージへ移動を開始する
    public void MoveToSelectedStage()
    {
        if (!HasValidSelection())
        {
            Debug.LogWarning("ステージが選択されていません。");
            return;
        }

        if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(SelectedStageId))
        {
            Debug.Log($"まだ開放されていないステージのため移動できません: {SelectedStageId}");
            return;
        }

        if (isMoving)
        {
            return;
        }

        List<int> route = BuildRouteIndices(currentIndex, selectedIndex);
        if (route.Count == 0)
        {
            return;
        }

        // 歩きSEをループで鳴らす
        SoundManager.Instance.PlayLoopSe(_seId);
        BeginRoute(route);
    }

    // 指定ステージへ移動する
    public void MoveToStage(MapId stageId)
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        if (isMoving)
        {
            return;
        }

        for (int i = 0; i < stageTargets.Count; i++)
        {
            if (stageTargets[i].stageId == stageId)
            {
                selectedIndex = i;

                if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(stageId))
                {
                    Debug.LogWarning($"まだ開放されていないステージのため移動できません: {stageId}");
                    return;
                }

                MoveToSelectedStage();
                return;
            }
        }

        Debug.LogWarning("指定したステージIDが見つかりません: " + stageId);
    }

    // 現在地でバトル開始要求を出す
    public void RequestBattleStartOnCurrentStage()
    {
        if (isMoving)
        {
            return;
        }

        if (currentStageId == MapId.None)
        {
            Debug.LogWarning("現在地のステージが未設定のため、バトル開始できません。");
            return;
        }

        if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(currentStageId))
        {
            Debug.LogWarning($"未開放ステージのため、バトル開始できません: {currentStageId}");
            return;
        }

        if (_isOpeningDialogue) return; // ダイアログ表示中は再度押させない

        ConfirmBattleStartDialogue();
    }

    // バトル開始ダイアログの表示
    public void ConfirmBattleStartDialogue()
    {
        _battleConfirm.OpenDialog();
        _isOpeningDialogue = true;
    }

    // バトル開始処理
    public void StartBattle()
    {
        Debug.Log("バトル開始: " + currentStageId);

        if (string.IsNullOrEmpty(battleSceneName))
        {
            Debug.LogWarning("battleSceneName が未設定のため、シーン遷移をスキップします。");
            return;
        }
         //シーンの遷移（前の会話があれば会話へ、なければ戦闘準備へ。行き先は StageFlow が決める）
        if (StageFlow.Instance == null)
        {
            Debug.LogError("StageSelectPlayer: StageFlow が存在しません");
            return;
        }
        StageFlow.Instance.StartStage(currentStageId);
    }

    // 選択状態が有効か確認する
    private bool HasValidSelection()
    {
        return stageTargets != null && stageTargets.Count > 0 && selectedIndex >= 0 && selectedIndex < stageTargets.Count;
    }

    // インデックスが有効か確認する
    private bool HasValidIndex(int index)
    {
        return stageTargets != null && index >= 0 && index < stageTargets.Count;
    }

    // ステージIDからインデックスを探す
    private int GetStageIndexById(MapId stageId)
    {
        if (stageTargets == null)
        {
            return -1;
        }

        for (int i = 0; i < stageTargets.Count; i++)
        {
            if (stageTargets[i] != null && stageTargets[i].stageId == stageId)
            {
                return i;
            }
        }

        return -1;
    }

    // 現在のインデックスを範囲内に収める
    private void ClampSelection()
    {
        if (stageTargets == null || stageTargets.Count == 0)
        {
            currentIndex = 0;
            selectedIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, stageTargets.Count - 1);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, stageTargets.Count - 1);
    }

    // 選択中ステージの目標位置を再計算する
    private void PrepareTargetPosition()
    {
        PrepareTargetPositionForIndex(selectedIndex);
    }

    // 指定インデックスの目標位置を設定する
    private bool PrepareTargetPositionForIndex(int index)
    {
        if (!HasValidIndex(index))
        {
            return false;
        }

        StageTarget target = stageTargets[index];
        if (target == null || target.marker == null)
        {
            Debug.LogWarning("選択中のステージにマーカーが設定されていません。");
            return false;
        }

        // ノード中心に足元を合わせるため、moveTargetの中心位置を足元オフセット分だけ補正する
        currentTargetPosition = target.marker.position + target.offset - pivotToFootOffset;

        // 目標位置と現在位置を比較し，進行方向を向く
        if (_groupChangeVisualDirection != null)
        {
            if (currentTargetPosition.x < moveTarget.position.x) _groupChangeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Left); 
            else _groupChangeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Right);
        } 

        return true;
    }

    // 現在地から目的地までの移動ルートを作る
    private List<int> BuildRouteIndices(int fromIndex, int toIndex)
    {
        List<int> route = new List<int>();

        if (!HasValidIndex(fromIndex) || !HasValidIndex(toIndex) || fromIndex == toIndex)
        {
            return route;
        }

        int step = fromIndex < toIndex ? 1 : -1;
        for (int index = fromIndex + step; ; index += step)
        {
            route.Add(index);

            if (index == toIndex)
            {
                break;
            }
        }

        return route;
    }

    // ルート移動を開始する
    private void BeginRoute(List<int> route)
    {
        pendingRouteIndices.Clear();
        pendingRouteIndices.AddRange(route);

        if (pendingRouteIndices.Count == 0)
        {
            return;
        }

        if (!PrepareTargetPositionForIndex(pendingRouteIndices[0]))
        {
            pendingRouteIndices.Clear();
            return;
        }

        // Walkアニメーションの開始
        if (_playerAnimator != null)
        {
            foreach (Animator anim in _playerAnimator) anim.SetBool("WalkBool", true);
        }
        else
        {
            Debug.LogError("playerAnimator is null in OnMove");
        }

        isMoving = true;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveRouteCoroutine());
        Debug.Log("ステージ移動開始: 現在=" + currentStageId + " / 目的地=" + SelectedStageId);
    }

    // ルートの次地点へ進める
    private void AdvanceRoute()
    {
        if (pendingRouteIndices.Count == 0)
        {
            FinishMovement();
            return;
        }

        currentIndex = pendingRouteIndices[0];
        currentStageId = GetStageId(currentIndex);
        pendingRouteIndices.RemoveAt(0);

        if (pendingRouteIndices.Count == 0)
        {
            FinishMovement();
            return;
        }

        if (!PrepareTargetPositionForIndex(pendingRouteIndices[0]))
        {
            pendingRouteIndices.Clear();
            FinishMovement();
        }
    }

    // 目的地にぴったり合わせる
    private void SnapToTarget()
    {
        if (targetRb != null)
        {
            targetRb.linearVelocity = Vector2.zero;
            targetRb.MovePosition(currentTargetPosition);
        }
        else
        {
            moveTarget.position = currentTargetPosition;
        }
    }

    // インデックスからステージIDを取得する
    private MapId GetStageId(int index)
    {
        if (!HasValidIndex(index))
        {
            return MapId.None;
        }

        StageTarget target = stageTargets[index];
        if (target == null)
        {
            return MapId.None;
        }

        return target.stageId;
    }

    // 最後に到達したマップIDを保存する
    private void UpdateGameProgressLastMapId(MapId mapId)
    {
        if (GameProgress.Instance == null)
        {
            return;
        }

        GameProgress.Instance.LastMapId = mapId;
    }

    // 移動完了時の後始末をする
    private void FinishMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        isMoving = false;
        pendingRouteIndices.Clear();
        // 歩行SEを止める
        SoundManager.Instance.StopLoopSe(_seId);

        if (currentStageId != MapId.None)
        {
            UpdateGameProgressLastMapId(currentStageId);
        }

        // Walkアニメーションの終了
        if (_playerAnimator != null)
        {
            foreach (Animator anim in _playerAnimator) anim.SetBool("WalkBool", false);
        }
        else
        {
            Debug.LogError("playerAnimator is null in OnMove");
        }

        Debug.Log("ステージ移動完了: 現在=" + currentStageId);
    }
}
