using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/*
 * シーン遷移の流れを担当するシングルトン
 * 流れ：暗転 → シーン変更 → 明転。遷移中は、クリックを止め、キーボードの選択も外す
 * 次のシーンが確実に分かっているときは、Preload で事前読み込みできる
 */

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("シーン名の対応")]
    [SerializeField] private SceneTable _sceneTable;

    [Header("暗転・明転")]
    [SerializeField] private SceneFade _fade;

    private bool _isTransitioning = false;
    private SceneId _preloadedSceneId = SceneId.None; // 事前読み込み中のシーン
    private AsyncOperation _preloadOperation;

    /// <summary>遷移中か。プレイヤーの移動など、直接入力を読むスクリプトは、これが true の間は動かさないこと</summary>
    public bool IsTransitioning => _isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("SceneLoaderが複数存在します");
            return;
        }
        Instance = this;

        if (_fade == null) Debug.LogError("SceneLoader: SceneFade が設定されていません");

        if (_sceneTable == null) Debug.LogError("SceneLoader: SceneTable が設定されていません");
        else _sceneTable.Validate(); // 設定の間違いを、起動時に見つける
    }

    /// <summary>次のシーンを事前に読み込んでおく。次のシーンが確実に分かっているときだけ使うこと</summary>
    public void Preload(SceneId id)
    {
        if (_isTransitioning)
        {
            Debug.LogError("遷移中は事前読み込みできません");
            return;
        }
        if (!TryGetSceneName(id, out string sceneName)) return;

        if (_preloadedSceneId == id) return; // すでに事前読み込み済み
        if (_preloadedSceneId != SceneId.None)
        {
            Debug.LogError($"すでに{_preloadedSceneId}を事前読み込み中です。同時に事前読み込みできるのは1つだけです");
            return;
        }

        _preloadOperation = SceneManager.LoadSceneAsync(sceneName);
        _preloadOperation.allowSceneActivation = false; // 読み込みだけ進めて、切り替えは遷移のときまで待つ
        _preloadedSceneId = id;
    }

    /// <summary>暗転 → シーン変更 → 明転 を行う</summary>
    public void LoadScene(SceneId id)
    {
        if (_fade == null || !_fade.IsReady)
        {
            Debug.LogError("SceneLoader: SceneFade が使えないため、遷移できません");
            return;
        }
        if (_isTransitioning)
        {
            Debug.LogWarning("遷移中です。新しい遷移の指示は無視しました");
            return;
        }
        if (!TryGetSceneName(id, out string sceneName)) return;

        // 事前読み込みしたシーン以外へは、遷移できない（読み込み待ちの処理が詰まるため）
        if (_preloadedSceneId != SceneId.None && _preloadedSceneId != id)
        {
            Debug.LogError($"{_preloadedSceneId}を事前読み込み中なので、{id}へは遷移できません");
            return;
        }

        TransitionAsync(sceneName).Forget();
    }

    // 遷移の本体
    private async UniTaskVoid TransitionAsync(string sceneName)
    {
        _isTransitioning = true;
        _fade.SetBlocking(true); // クリックを止める
        DeselectUI();            // キーボードの選択を外す

        await _fade.FadeAsync(1f); // 暗転

        await LoadAsync(sceneName); // シーン変更

        _fade.SetBlocking(false);
        _isTransitioning = false;

        _fade.FadeAsync(0f).Forget(); // 明転
    }

    // シーンを読み込む。事前読み込み済みなら、切り替えるだけ
    private async UniTask LoadAsync(string sceneName)
    {
        if (_preloadedSceneId != SceneId.None)
        {
            _preloadOperation.allowSceneActivation = true; // 準備済みのシーンに切り替える
            await _preloadOperation;
            _preloadedSceneId = SceneId.None;
            return;
        }

        await SceneManager.LoadSceneAsync(sceneName);
    }

    // キーボードで選択されている物を外す
    private void DeselectUI()
    {
        if (EventSystem.current == null) return; // EventSystemがないシーンもある
        EventSystem.current.SetSelectedGameObject(null);
    }

    // シーンの種類から、シーン名を引く
    private bool TryGetSceneName(SceneId id, out string sceneName)
    {
        if (_sceneTable == null)
        {
            Debug.LogError("SceneLoader: SceneTable が設定されていません");
            sceneName = null;
            return false;
        }
        return _sceneTable.TryGetSceneName(id, out sceneName);
    }
}