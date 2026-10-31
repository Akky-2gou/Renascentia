using System.Collections.Generic;
using UnityEngine;

/*
 * 決まった順番で進むシーンの並び（タイトル後の最初の流れなど）
 * 各シーンが終わるたびに、SequenceFlow が次の行き先へ進める
 */

[CreateAssetMenu(menuName = "Game/Flow/SceneSequence")]
public class SceneSequence : ScriptableObject
{
    [SerializeField] private List<TransitionTarget> _steps = new List<TransitionTarget>(); // 上から順に進む

    public IReadOnlyList<TransitionTarget> Steps => _steps;
}