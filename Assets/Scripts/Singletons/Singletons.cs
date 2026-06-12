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
}
