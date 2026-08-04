using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

/*
 * プレイヤーの見た目の高さを地形に沿って調整するスクリプト
 * アーチ橋などに利用
 */

public class VisualGroundSnapper : MonoBehaviour
{
    // 対象の地面のレイヤーを参照 
    [SerializeField] private List<LayerMask> _targetGroundLayers;

    // 見た目の高さのオフセット値
    [SerializeField] private float _heightOffset = 0.4f; 

    // 地形に沿って高さを変更するRayの長さを設定
    [SerializeField] private float _rayLength = 15.0f;

    // Rayの開始位置を少し上にオフセットするための値
    [SerializeField] private float _rayStartOffsetY = 5.0f; 

    private Collider2D _parentCollider; // 親オブジェクトのCollider2Dを参照する変数
    private CancellationTokenSource _cancellationTokenSource; // CancellationTokenを管理するクラス

    void Start()
    {
        if (transform.parent == null)
        {
            Debug.LogError("VisualGroundSnapper: 親オブジェクトが設定されていません。");
        }

        _parentCollider = transform.parent.GetComponent<Collider2D>();
        if (_parentCollider == null)
        {
            Debug.LogError("VisualGroundSnapper: 親オブジェクトにCollider2Dがアタッチされていません。");
        }

        // オブジェクトが破棄されたときにキャンセルされるトークンソースを作成
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        UpdateVisualHeight(_cancellationTokenSource.Token).Forget(); // 非同期で高さを更新する処理を開始
    }

    // 地形に沿って高さを変更する関数
    private async UniTaskVoid UpdateVisualHeight(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 自分より少し高いところからRayを飛ばす
            Vector2 rayStartPoint = transform.position;
            rayStartPoint += new Vector2(0, _rayStartOffsetY); // 少し上にオフセットしてRayを飛ばす

            bool hitDetected = false; // Rayが地面に当たったかどうかのフラグ

            // 各対象の地面レイヤーに対してRayを飛ばす
            foreach (LayerMask layer in _targetGroundLayers)
            {
                // 下に向かってRayを飛ばし，地面の座標を取得
                RaycastHit2D hit = Physics2D.Raycast(rayStartPoint, Vector2.down, _rayLength, layer);

                if (hit.collider != null)
                {
                    // Rayが当たった地点が親オブジェクトの下端より高い場合，地面の高さに合わせる
                    if (_parentCollider.bounds.min.y < hit.point.y)
                    {
                        // Rayが当たった座標を，親オブジェクトから見たローカル座標に変換
                        Vector3 localHitPoint = transform.parent.InverseTransformPoint(hit.point);

                        // 親オブジェクトから見た自分のローカル座標を，地面の高さに合わせる
                        transform.localPosition = new Vector3(transform.localPosition.x, localHitPoint.y + _heightOffset, transform.localPosition.z);
                    }

                    hitDetected = true; // Rayが地面に当たったことを記録
                    break; // 1つのレイヤーでヒットしたらループを抜ける
                }
            }

            // Rayが地面に当たらなかった場合，デフォルトの高さに戻す
            if (!hitDetected)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            }

            // 毎フレーム待機
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _cancellationTokenSource.Token);
        }
    }
}
