using UnityEngine;

/*
 * シングルトンらを保持するスクリプト
 */

public class Singletons : MonoBehaviour
{
    private static Singletons instance;

    private void Awake()
    {
        // すでに存在する場合（自分自身でない）、新しい方を破棄
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 最初の1つ目を保存し、破棄されないようにする
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 渡されたコンポーネントが、これから破棄される複製の Singletons の子か
    /// 各シングルトンの Awake の最初に呼んで、true なら何もせず戻ること
    /// </summary>
    public static bool IsInDuplicate(Component self)
    {
        Singletons parent = self.GetComponentInParent<Singletons>();
        if (parent == null) return false; // Singletons の外にある場合は、判定しない
        return instance != null && instance != parent;
    }
}
