using UnityEngine;
using System.Collections;

public class DebugScript : MonoBehaviour {
    UILabel Label = null;
	// Use this for initialization
	void Start () {
        Label = transform.GetChild(0).GetComponent<UILabel>();
	}
	
    public void SetLabelString(string msg)
    {
        Label.text = msg;
    }
}
