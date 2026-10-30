using UnityEngine;
using System.Collections.Generic;
using System;

/*
 * グループの視覚的な方向を変える
 */

public class GroupChangeVisualDirection : MonoBehaviour
{
    // メインキャラクターの情報
    private Transform _mainCharaTransform = null;
    private ChangeVisualDirection _mainCharaCVDs = null;

    // サブキャラクターの情報
    private List<Transform> _subCharaTransform = new List<Transform>();
    private List<ChangeVisualDirection> _subCharaCVDs = new List<ChangeVisualDirection>();

    
    /// <summary>
    /// メインキャラのキャラの設定
    /// </summary>
    public void UpdateMainCharacter(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("ChangeVisualDirection: メインキャラクターオブジェクトがnullです");
            return;
        } 

        _mainCharaTransform = obj.transform;
        _mainCharaCVDs = obj.GetComponent<ChangeVisualDirection>();

        if (_mainCharaCVDs == null)
        {
            Debug.LogError("ChangeVisualDirection: メインキャラクターのChangeVisualDirectionがありません");
            return;
        } 
    }
    
    /// <summary>
    /// サブキャラのリセット
    /// </summary>
    public void ClearSubCharacters()
    {
        foreach (Transform obj in _subCharaTransform) if (obj != null) Destroy(obj);
        _subCharaTransform.Clear();
        
        foreach (ChangeVisualDirection cvd in _subCharaCVDs) if (cvd != null) Destroy(cvd);
        _subCharaCVDs.Clear();
    }
    
    /// <summary>
    /// サブキャラの追加
    /// </summary>
    public void AddSubCharacters(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("ChangeVisualDirection: サブキャラクターオブジェクトがnullです");
            return;
        } 

        _subCharaTransform.Add(obj.transform);
        _subCharaCVDs.Add(obj.GetComponent<ChangeVisualDirection>());

        if (_subCharaCVDs[_subCharaCVDs.Count - 1] == null)
        {
            Debug.LogError("ChangeVisualDirection: サブキャラクターのChangeVisualDirectionがありません");
            return;
        } 
    }

    /// <summary>
    /// 全員の視覚的な方向を変える
    /// </summary>
    public void Change(ChangeVisualDirection.VisualDirection visualDirection)
    {
        // メインキャラクターの方向を変える
        _mainCharaCVDs.Change(visualDirection);

        // サブキャラクターの視覚的な方向を変える
        foreach (ChangeVisualDirection cvd in _subCharaCVDs)
        {
            cvd.Change(visualDirection);
        }

        // メインキャラクターの後ろに移動
        Vector3 newPos;
        switch (visualDirection)
        {
            case ChangeVisualDirection.VisualDirection.Left:
                foreach (Transform subTransform in _subCharaTransform)
                {
                    newPos.x = _mainCharaTransform.position.x + Math.Abs(subTransform.position.x - _mainCharaTransform.position.x);
                    newPos.y = subTransform.position.y;
                    newPos.z = subTransform.position.z;
                    subTransform.position = newPos;
                }
                break;
                
            case ChangeVisualDirection.VisualDirection.Right:
                foreach (Transform subTransform in _subCharaTransform)
                {
                    newPos.x = _mainCharaTransform.position.x - Math.Abs(subTransform.position.x - _mainCharaTransform.position.x);
                    newPos.y = subTransform.position.y;
                    newPos.z = subTransform.position.z;
                    subTransform.position = newPos;
                }
                break;
                
            default:
                break;
        }
    }
}
