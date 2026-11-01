using UnityEngine;

public class TaikenbanButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameProgress.Instance.LastMapId == MapId.Map1_4 && 
        GameProgress.Instance.StageProgress == MapId.Map1_5) this.gameObject.SetActive(true);
        else this.gameObject.SetActive(false);
    }
}
