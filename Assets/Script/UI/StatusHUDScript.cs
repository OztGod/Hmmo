using UnityEngine;
using System.Collections;

public class StatusHUDScript : MonoBehaviour {
    public float YGab = 0.0f;
    public Transform Target = null;
    ApBarScript ApBar = null;
    HpBarScript HpBar = null;
    DmgLabelController DmgController = null;
	// Use this for initialization

	void Awake () {
        ApBar = transform.FindChild("Act_Background").GetComponent<ApBarScript>();
        HpBar = transform.FindChild("Health_Background").GetComponent<HpBarScript>();
        DmgController = transform.FindChild("DmgLabelContainer").GetComponent<DmgLabelController>();
	}
	
	// Update is called once per frame
	void Update () {

        if (Target == null)
            return;

        var wantedPos = Camera.main.WorldToScreenPoint(Target.position);
        transform.localPosition = new Vector3(wantedPos.x - Screen.width / 2, wantedPos.y + YGab - Screen.height / 2, 0);
	}

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    public void SetHp(int maxHp, int curHp)
    {
        Debug.Log("SetHp:" + curHp + "/" + maxHp);
        HpBar.SetHp(maxHp, curHp);
    }

    public void SetAp(int maxAp, int curAp)
    {
        Debug.Log("SetAp:" + curAp + "/" + maxAp);
        ApBar.SetAp(maxAp, curAp);
    }

    public void OnDamage(int dmg)
    {
        DmgController.MakeDamageLabel(dmg);
    }

    public void Release()
    {
        ApBar.SetAp(0, 0);
        HpBar.SetHp(0, 0);
        Target = null;
        GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().ReleaseHud(gameObject);
    }
}
