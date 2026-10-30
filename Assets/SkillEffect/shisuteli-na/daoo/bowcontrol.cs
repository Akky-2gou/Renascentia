using UnityEngine;

public class bowcontrol : MonoBehaviour
{
    public bool b = false;
    public int t = 1;
    int i = 0;
    public float x = 0, xw = 0;   // double Å® float
    public float y = 0, yw = 0;   // double Å® float
    public GameObject obj;

    void Start()
    {
    }

    void Update()
    {
        i += 1;
        if (b && t <= i)
        {
            float p = Random.Range(0f, 1f), q = Random.Range(0f, 1f);
            Instantiate(obj, new Vector3(x + xw*p, y + yw*q, 0), Quaternion.identity);
            i = 0;
        }
    }
}