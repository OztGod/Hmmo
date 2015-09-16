using UnityEngine;
using System.Collections;

public class StatusHUDScript : MonoBehaviour {
    public float YGab = 0.0f;
    public Transform Target = null;
    ApBarScript ApBar = null;
    HpBarScript HpBar = null;
    DmgLabelController DmgController = null;
    StateContainer StateContainer = null;
	// Use this for initialization

	void Awake () {
        ApBar = transform.FindChild("Act_Background").GetComponent<ApBarScript>();
        HpBar = transform.FindChild("Health_Background").GetComponent<HpBarScript>();
        DmgController = transform.FindChild("DmgLabelContainer").GetComponent<DmgLabelController>();
        StateContainer = transform.FindChild("StatusContainer").GetComponent<StateContainer>();
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

    public void Reset()
    {
        SetAp(0, 0);
        SetHp(0, 0);
    }

    public void SetHp(int maxHp, int curHp)
    {
        HpBar.SetHp(maxHp, curHp);
    }

    public void SetAp(int maxAp, int curAp)
    {
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

    public void UpdateStatus(int id, StateType type, int duration)
    {
        StateContainer.UpdateStatus(id, type, duration);
    }

    public void RemoveStatus(int id)
    {
        StateContainer.RemoveStatus(id);
    }

}
