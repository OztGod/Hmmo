using UnityEngine;
using System.Collections;

public class TurnOverButton : MonoBehaviour {
    UILabel label;
    UIButton button;
    MapManager mapManager;
	// Use this for initialization

	void Awake () 
    {
        label = transform.FindChild("Label").GetComponent<UILabel>();
        button = GetComponent<UIButton>();
        mapManager = GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>();
	}

    void Start()
    {
        Turn(false);
    }

    public void OnButtonClick()
    {
        mapManager.TurnOver();
    }
    
    public void Turn(bool isMyTurn)
    {
        if(isMyTurn)
        {
            button.isEnabled = true;
            label.text = "My Turn";
        }
        else
        {
            button.isEnabled = false;
            label.text = "Wait...";
        }
    }
    
    void OnHover(bool isOver)
    {
        if (isOver)
        {
            label.text = "Turn Over";
        }
        else
        {
            if (button.isEnabled)
                label.text = "My Turn";
            else
                label.text = "Wait...";
        }
    }


}
