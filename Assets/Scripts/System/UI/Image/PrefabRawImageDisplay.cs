using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

/*
 * PrefabをRawImageとして表示するスクリプト
 */

public class PrefabRawImageDisplay : MonoBehaviour
{
    [Header("カメラとその設置場所（何か他のものが映る場合は調整してください）")]
    [SerializeField] GameObject _cameraPrefab;
    [SerializeField] Vector3 _cameraPos = new Vector3(-400, 0, -10);

    [Header("映したいPrefabの位置・大きさ")]
    [SerializeField] Vector3 _targetOffsetPos = new Vector3(0, 0, 1);
    [SerializeField] Vector3 _targetScale = new Vector3(1, 1, 1);
    
    [Header("RenderTextureの大きさ")]
    [SerializeField] int _renderTextureSize = 1024;


    private RenderTexture _renderTexture;
    private Camera _camera;
    private GameObject _targetObj = null;


    void Start()
    {
        // カメラを生成
        if (_cameraPrefab == null)
        {
            Debug.LogError("PrefabImageDisplay: Cameraのプレハブがアタッチされていません");
            return;
        }
        GameObject cameraObj = Instantiate(_cameraPrefab, _cameraPos, Quaternion.identity);

        // カメラを取得
        _camera = cameraObj.GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("PrefabImageDisplay: CameraのプレハブにCameraがアタッチされていません");
            return;
        }

        // RenderTexture を生成 (幅, 高さ, 深度バッファのビット数)
        _renderTexture = new RenderTexture(_renderTextureSize, _renderTextureSize, 24);
        _renderTexture.Create(); // GPU上にテクスチャを実際に作成

        // RenderTexture をカメラのTargetTextureに設定
        _camera.targetTexture = _renderTexture;

        // RenderTexture を自身のRawImageに設定
        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError("PrefabImageDisplay: 自身にRawImageがアタッチされていません");
            return;
        }
        rawImage.texture = _renderTexture;
    }

    /// <summary>
    /// 指定されたプレハブを見せる
    /// </summary>
    public void ShowPrefab(GameObject prefab)
    {
        // 前のものは消去
        if (_targetObj != null) Destroy(_targetObj);

        // 映したいプレハブを生成
        if (prefab == null)
        {
            Debug.LogError("PrefabImageDisplay: 映したいPrefabがアタッチされていません");
            return;
        }
        _targetObj = Instantiate(prefab, _cameraPos + _targetOffsetPos, Quaternion.identity);
        _targetObj.transform.localScale = _targetScale;     

        // アニメーターを取得し，時が止まった状態でもアニメーションを流すようにする
        Animator animator = _targetObj.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.Log("PrefabImageDisplay: 映したいPrefabにはAnimatorがアタッチされていません");
        }
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    /// <summary>
    /// 何も表示しないようにする
    /// </summary>
    public void Clear()
    {
        // 前のものは消去
        if (_targetObj != null) Destroy(_targetObj);
    }

    void OnDestroy()
    {
        // RenderTextureはGC（ガベージコレクション）されないため手動で解放
        if (_renderTexture != null)
        {
            // シーンを閉じるとき、カメラが先に消えていることがある。残っているときだけ外す
            if (_camera != null)
            {
                _camera.targetTexture = null;
            }
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
