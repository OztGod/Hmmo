using UnityEngine;
using System.Collections;

public class HpBarScript : MonoBehaviour {
    UIProgressBar progressBar = null;
    UILabel label = null;

    public int MaxHp = 0;
    public int CurHp = 0;

	// Use this for initialization
	void Awake () {
        progressBar = transform.GetComponentInChildren<UIProgressBar>();
        label = transform.GetComponentInChildren<UILabel>();
	}
	
    public void SetHp(int maxHp, int curHp)
    {
        MaxHp = maxHp;
        CurHp = curHp;
        
        if (maxHp == 0)
        {
            label.text = "";
            progressBar.value = 0.0f;
        }
        else
        {
            label.text = CurHp.ToString() + " / " + MaxHp.ToString();
            progressBar.value = (float)CurHp / (float)MaxHp;
        }
    }
}
