using UnityEngine;

/*
 * 学祭用: タイトルシーンに来るたびに、編成を「Slot1 に HimawariW だけ」に戻す
 * タイトルシーンの空のオブジェクトにアタッチして使う
 */
public class TitlePartyReset : MonoBehaviour
{
    [SerializeField] private CharacterData _himawari; // Inspector で HimawariW の CharacterData を入れる

    private void Start()
    {
        Debug.Log("yobare");
        if (_himawari == null)
        {
            Debug.LogError("TitlePartyReset: HimawariW が設定されていないため、編成を初期化できません");
            return;
        }

        if (Party.Instance == null)
        {
            Debug.LogError("TitlePartyReset: Party が存在しないため、編成を初期化できません");
            return;
        }

        Party.Instance.SetMembersForFestival(_himawari, null, null, null);
        Debug.Log("TitlePartyReset: 編成を初期化しました（Slot1 = HimawariW）");
    }
}