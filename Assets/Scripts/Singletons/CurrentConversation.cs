using UnityEngine;

/*
 * 次に会話シーンで読む、会話IDを受け渡すためのシングルトン（シーンをまたいで残る）
 * 保存の対象ではない。会話に入るたびに、必ず上書きされる
 */

public class CurrentConversation : MonoBehaviour
{
    public static CurrentConversation Instance { get; private set; }

    private const int UNSET_ID = -1; // 未設定を表す値（会話IDは非負整数）

    [Header("会話シーンを直接再生するときの会話ID（テスト用。通常は -1 のまま）")]
    [SerializeField] private int _initialConversationId = UNSET_ID;

    private int _conversationId = UNSET_ID;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("CurrentConversationが複数存在します");
            return;
        }
        Instance = this;

        _conversationId = Mathf.Max(_initialConversationId, UNSET_ID);
    }

    /// <summary>読む会話のID</summary>
    public int ConversationId => _conversationId;

    /// <summary>会話IDが設定されているか。会話シーンの開始時に確認する</summary>
    public bool IsSet => _conversationId != UNSET_ID;

    /// <summary>読む会話を指定する（会話シーンに行く前に呼ぶ）</summary>
    public void SetConversation(int conversationId)
    {
        if (conversationId <= UNSET_ID)
        {
            Debug.LogError($"会話IDが不正です: {conversationId}（0以上を指定してください）");
            return;
        }
        _conversationId = conversationId;
    }
}