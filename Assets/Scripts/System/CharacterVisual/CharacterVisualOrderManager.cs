using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Live2D.Cubism.Rendering;

/*
 * キャラクターの絶対的な順番を制御するスクリプト（Live2Dキャラより後ろに非Live2Dキャラを配置するのは非対応）
 */

public class CharacterVisualOrderManager : MonoBehaviour
{
    [Header("キャラクターに割り当ててよい順番の下限")]
    [field: SerializeField] public int MinOrderLive2d { get; set; } 
    [field: SerializeField] public int MinOrderSpriteSkin { get; set; } 

    [Header("キャラクターのリスト（基本的には他のスクリプトから変更するのを想定）")]
    [SerializeField] Dictionary<int, CubismRenderController> _live2DCharacters = new Dictionary<int, CubismRenderController>();
    [SerializeField] Dictionary<int, PartsAbsoluteOrderController> _spriteSkinCharacters = new Dictionary<int, PartsAbsoluteOrderController>();


    /// <summary>
    /// キャラクターのオブジェクトの登録
    /// </summary>
    public void AddCharacters(int order, GameObject obj)
    { 
        // CubismRenderController，PartsAbsoluteOrderControllerを取得
        CubismRenderController crc = obj.GetComponent<CubismRenderController>();
        PartsAbsoluteOrderController paoc = obj.GetComponent<PartsAbsoluteOrderController>();

        // CubismRenderControllerがあればLive2Dキャラとして追加
        if (crc != null)
        {
            _live2DCharacters.Add(order, crc);
            return;
        }

        // PartsAbsoluteOrderControllerがあればSpriteSkinキャラとして追加
        if (paoc != null)
        {
            _spriteSkinCharacters.Add(order, paoc);
            return;
        }

        // どちらもなければエラー
        Debug.LogError($"CharacterVisualOrderManager: {obj.name}にCubismRenderControllerまたはPartsAbsoluteOrderControllerがアタッチされていません");
    }

    /// <summary>
    /// キャラクターの見た目の順番を指定した順に変更
    /// </summary>
    public void AssignOrder()
    {
        // 指定された順番が下のLive2Dキャラから，見た目の順番を変更
        List<int> keys = _live2DCharacters.Keys.ToList();
        keys.Sort();
        int order = MinOrderLive2d;
        foreach (int key in keys)
        {
            if (_live2DCharacters[key] == null) continue;

            _live2DCharacters[key].GroupedSortingIndex = order; // 見た目の順番を変更
            order++;
        }
        
        // 指定された順番が下のSpriteSkinキャラから，見た目の順番を変更
        keys = _spriteSkinCharacters.Keys.ToList();
        keys.Sort();
        order = MinOrderSpriteSkin;
        foreach (int key in keys)
        {
            if (_spriteSkinCharacters[key] == null) continue;

            order = _spriteSkinCharacters[key].ChangeOrderFromLower(order) + 1; // 見た目の順番を変更
        }
    }
}
