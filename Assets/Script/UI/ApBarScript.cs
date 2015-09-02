using UnityEngine;
using System.Collections;

public class ApBarScript : MonoBehaviour
{
    UIProgressBar progressBar = null;
    UILabel label = null;

    public int MaxAp = 0;
    public int CurAp = 0;

    // Use this for initialization
    void Awake()
    {
        progressBar = transform.GetComponentInChildren<UIProgressBar>();
        label = transform.GetComponentInChildren<UILabel>();
    }

    public void SetAp(int maxAp, int curAp)
    {
        MaxAp = maxAp;
        CurAp = curAp;

        label.text = CurAp.ToString() + " / " + MaxAp.ToString();
        progressBar.value = (float)CurAp / (float)MaxAp;
    }
}