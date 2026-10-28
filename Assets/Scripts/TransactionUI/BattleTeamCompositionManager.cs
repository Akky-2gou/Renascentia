using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * バトルチーム編成画面の管理スクリプト
 */
public class BattleTeamCompositionManager : MonoBehaviour
{
    [SerializeField] private GameObject compositionPanel;
    [SerializeField] private GameObject sortieConfirmDialog;
    [SerializeField] private string battleSceneName = "MapToBattle";

    private TransitionTarget _nextScene;

    private void Start()
    {
        _nextScene = new TransitionTarget(SceneId.Battle, CurrentConversation.Instance.ConversationId, GameProgress.Instance.LastMapId);
        SceneNavigator.Instance.Prepare(_nextScene);
    }

    //編成画面を閉じる
    public void CancelBattleTeamComposition()
    {
        Debug.Log("編成画面を閉じます");

        if (compositionPanel != null)
        {
            compositionPanel.SetActive(false);
        }
    }

    //出撃確認ダイアログを表示
    public void SortieToBattleScene()
    {
        Debug.Log("出撃確認ダイアログを表示します");

        if (sortieConfirmDialog != null)
        {
            Debug.Log("nillじゃないから表示されるはず");
            sortieConfirmDialog.SetActive(true);
            return;
        }

        Debug.LogWarning("Sortie confirmation dialog is not assigned.");
        LoadBattleScene();
    }

    public void CancelSortieToBattleScene()
    {
        if (sortieConfirmDialog != null)
        {
            sortieConfirmDialog.SetActive(false);
            return;
        }

        Debug.LogWarning("Sortie confirmation dialog is not assigned.");
    }

    //本当にバトルシーンへ移動
    public void ConfirmSortieToBattleScene()
    {
        LoadBattleScene();
    }

    //バトルシーンへ移動
    private void LoadBattleScene()
    {
        Debug.Log("出撃: " + battleSceneName + " へ移動します");
        SceneNavigator.Instance.Go(_nextScene);
    }
}
