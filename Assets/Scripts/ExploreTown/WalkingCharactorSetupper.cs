using UnityEngine;
using System.Collections.Generic;

/*
 * 歩くキャラクターのセットアップするスクリプト
 */

public class WalkingCharactorSetupper : MonoBehaviour
{
    [SerializeField] private List<GameObject> _charactorPrefabs; // キャラクターのプレハブを格納するリスト
    [SerializeField] private PlayerController _playerController; // キャラクターコントローラの参照
    [SerializeField] private float _characterVisualScale = 0.65f; // キャラクターの見た目のオブジェクトの大きさ

    private GameObject _characterVisualObj = null; // キャラクターの見た目のオブジェクトを保持する変数

    /// <summary>
    /// 歩くキャラクターのセットアップを行う
    /// </summary>
    public void SetupWalkingCharactor(int charactorID)
    {
        // キャラクターIDに対応するプレハブが登録されていなければ，エラーを出力
        if (_charactorPrefabs[charactorID] == null)
        {
            Debug.LogError($"WalkingCharactorSetupper: キャラクターID{charactorID}に対応するキャラクターのプレハブが登録されていません");
            return;
        }

        // キャラクターの見た目のオブジェクトが既に存在すれば，それを削除
        if (_characterVisualObj != null)
        {
            Destroy(_characterVisualObj);
        }

        // キャラクターのプレハブを用いて，自身の子としてキャラクターの見た目を生成
        _characterVisualObj = Instantiate(_charactorPrefabs[charactorID], transform);
        _characterVisualObj.transform.localScale = new Vector3(_characterVisualScale, _characterVisualScale, 1f);

        // キャラクターのChangeVisualDirection，Animatorを取得
        ChangeVisualDirection changeVisualDirection = _characterVisualObj.GetComponent<ChangeVisualDirection>();
        Animator animator = _characterVisualObj.GetComponent<Animator>();

        // 取得したものをPlayerControllerに渡す
        _playerController.SetPlayerVisualComponet(changeVisualDirection, animator);
    }
}
