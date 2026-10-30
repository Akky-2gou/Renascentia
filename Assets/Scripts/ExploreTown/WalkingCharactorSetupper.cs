using UnityEngine;
using System.Collections.Generic;

/*
 * 歩くキャラクターのセットアップするスクリプト
 */

public class WalkingCharactorSetupper : MonoBehaviour
{
    [Header("WakingCharacterDataの参照")]
    [SerializeField] private WakingCharacterData _wakingCharacterData;

    [Header("二人以上が歩く場合のキャラクター同士の間隔")]
    [SerializeField] private float _charactersXPositionSpace;


    [SerializeField] private PlayerController _playerController = null; // キャラクターコントローラの参照
    [SerializeField] private StageSelectPlayer _stageSelectPlayer = null; // ステージセレクトプレイヤーの参照
    [SerializeField] private float _characterVisualScale = 0.65f; // キャラクターの見た目のオブジェクトの大きさ

    private List<GameObject> _characterVisualObj = new List<GameObject>(); // キャラクターの見た目のオブジェクトを保持する変数

    /// <summary>
    /// 歩くキャラクターのセットアップを行う
    /// </summary>
    public void Setup(int charactorID)
    {
        // キャラクターIDに対応するプレハブが登録されていなければ，エラーを出力
        if (_wakingCharacterData[charactorID] == null)
        {
            Debug.LogError($"WalkingCharactorSetupper: キャラクターID{charactorID}に対応するキャラクターのプレハブが登録されていません");
            return;
        }

        // キャラクターの見た目のオブジェクトが既に存在すれば，それを削除
        if (_characterVisualObj != null)
        {
            foreach (GameObject obj in _characterVisualObj) if (obj != null) Destroy(obj);
            _characterVisualObj.Clear();
        }

        // キャラクターのプレハブを用いて，自身の子としてキャラクターの見た目を生成
        _characterVisualObj.Add(Instantiate(_wakingCharacterData[charactorID], transform));
        _characterVisualObj[0].transform.localScale = new Vector3(_characterVisualScale, _characterVisualScale, 1f);

        if (charactorID == _wakingCharacterData.SistineBId)
        {
            _characterVisualObj.Add(Instantiate(_wakingCharacterData[_wakingCharacterData.SistineRId], transform));
            _characterVisualObj[1].transform.localPosition = new Vector3(_charactersXPositionSpace, 0f, 1f);
            _characterVisualObj[1].transform.localScale = new Vector3(_characterVisualScale, _characterVisualScale, 1f);
        }
        else if (charactorID == _wakingCharacterData.SistineRId)
        {
            _characterVisualObj.Add(Instantiate(_wakingCharacterData[_wakingCharacterData.SistineBId], transform));
            _characterVisualObj[1].transform.localPosition = new Vector3(_charactersXPositionSpace, 0f, 1f);
            _characterVisualObj[1].transform.localScale = new Vector3(_characterVisualScale, _characterVisualScale, 1f);
        }

        // キャラクターのChangeVisualDirection，Animatorを取得
        List<Animator> animators = new List<Animator>();
        foreach (GameObject obj in _characterVisualObj)
        {
            animators.Add(obj.GetComponent<Animator>());
        }

        // 取得したものをPlayerControllerまたはStageSelectPlayerに渡す
        if (_playerController != null) _playerController.SetPlayerVisualComponet(_characterVisualObj, animators);
        if (_stageSelectPlayer != null) _stageSelectPlayer.SetPlayerVisualComponet(_characterVisualObj, animators);
    }
}
