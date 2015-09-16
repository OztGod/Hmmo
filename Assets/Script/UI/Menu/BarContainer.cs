using UnityEngine;
using System.Collections;

public class BarContainer : MonoBehaviour {
    ApBarScript ApBar = null;
    HpBarScript HpBar = null;

    void Awake()
    {
        ApBar = transform.FindChild("Act_Background").GetComponent<ApBarScript>();
        HpBar = transform.FindChild("Health_Background").GetComponent<HpBarScript>();
    }

    public void SetHp(int maxHp, int curHp)
    {
        HpBar.SetHp(maxHp, curHp);
    }

    public void SetAp(int maxAp, int curAp)
    {
        ApBar.SetAp(maxAp, curAp);
    }

    public void Reset()
    {
        SetHp(0, 0);
        SetAp(0, 0);
    }
}
