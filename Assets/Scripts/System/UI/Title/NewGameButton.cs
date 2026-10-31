using UnityEngine;

public class NewGameButton : Button
{
    public override void OnClick()
    {
        base.OnClick();

        if (GameProgress.Instance == null)
        {
            Debug.LogWarning("NewGameButton: GameProgress が存在しません。");
            return;
        }

        GameProgress.Instance.StageProgress = MapId.Map1_1;
        GameProgress.Instance.LastMapId = MapId.Map1_1;

        GameProgress.Instance.UnlockedCharacterIds.Clear();
        GameProgress.Instance.UnlockedCharacterIds.Add(0);

        //　シーン遷移の処理
    }
}
