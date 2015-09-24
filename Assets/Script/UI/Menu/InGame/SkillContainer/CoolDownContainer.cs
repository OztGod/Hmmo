using UnityEngine;
using System.Collections;

public class CoolDownContainer : MonoBehaviour {
    UIProgressBar progressBar;
    UILabel coolLabel;
    int maxCool;
    int curCool;

	// Use this for initialization

    void Awake() 
    {
        progressBar = GetComponent<UIProgressBar>();
        coolLabel = transform.FindChild("CoolDownLabel").GetComponent<UILabel>();
	}
	
    public void SetCoolDown(int curCool, int maxCool)
    {
        if (curCool == 0)
        {
            coolLabel.text = "";
            progressBar.value = 0;
        }
        else
        {
            float coolPercent = maxCool > 0 ? (float)curCool / maxCool : 0.0f;
            progressBar.value = coolPercent;
            coolLabel.text = curCool.ToString();
        }
    }

    public void CoolOff()
    {
        coolLabel.text = "";
        progressBar.value = 1;
    }
}
