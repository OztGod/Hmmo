using UnityEngine;
using System.Collections;

public class StateIcon : MonoBehaviour {
    public int id = -1;
    UI2DSprite sprite;
    UIManager uiManager;
    UILabel label;
    int curDuration;
    
	// Use this for initialization
	void Awake () {
        sprite = GetComponent<UI2DSprite>();
        uiManager = GameObject.FindWithTag("UI").GetComponent<UIManager>();
        label = transform.FindChild("Label").GetComponent<UILabel>();
	}
	
    public void InitState(StateType type, int duration, int stateId)
    {
        id = stateId;
        sprite.sprite2D = uiManager.GetStateSprite(type);
        curDuration = duration;
        label.text = curDuration.ToString();        
    }

    public void UpdateState(int duration)
    {
        curDuration = duration;
        label.text = curDuration.ToString();
    }

}
