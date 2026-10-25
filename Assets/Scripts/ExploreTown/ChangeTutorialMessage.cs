using UnityEngine;

/*
 * チュートリアルメッセージを切り替える
 */

public class ChangeTutorialMessage : MonoBehaviour
{
    [Header("チュートリアルメッセージのオブジェクト")]
    [SerializeField] GameObject _tutorialMessagePre;
    [SerializeField] GameObject _tutorialMessagePost;

    [Header("プレイヤーのタグ")]
    [SerializeField] private string _playerTag = "Player";

    private bool _isChanged = false; // 切り替え済みかどうか


    private void Start()
    {
        // メッセージの表示を初期化
        _tutorialMessagePre.SetActive(true);
        _tutorialMessagePost.SetActive(false);

        _isChanged = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // まだ切り替えておらず，Player に接触した場合，切り替える
        if (collision.gameObject.CompareTag(_playerTag) && !_isChanged)
        {
            _isChanged = true;
            _tutorialMessagePre.SetActive(false);
            _tutorialMessagePost.SetActive(true);
        }
    }
}
