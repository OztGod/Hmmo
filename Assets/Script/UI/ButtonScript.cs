using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonScript : MonoBehaviour {
    public string LabelString;
    PanelScript ParentPanel = null;
    ButtonType Type;

	// Use this for initialization
	void Start () {
        transform.GetChild(0).GetComponent<UILabel>().text = LabelString;
        ParentPanel = transform.parent.GetComponent<PanelScript>();
        UIEventListener.Get(gameObject).onClick += OnButtonClick;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetButton(ButtonSetting set)
    {
        Type = set.Type;
        LabelString = set.Name;
        transform.GetChild(0).GetComponent<UILabel>().text = LabelString;
        gameObject.GetComponent<UIButton>().isEnabled = set.IsEnable;
    }

    public void OnButtonClick(GameObject sender)
    {
        ParentPanel.OnButtonClick(Type);
    }
}
