using UnityEngine;
using System.Collections;

public class CostContainer : MonoBehaviour {
    UILabel label;
	// Use this for initialization
    void Awake() 
    {
        label = transform.FindChild("Label").GetComponent<UILabel>();
	}

    public void SetCost(int cost)
    {
        if (cost == 0)
        {
            label.text = "";
        }
        else
        {
            label.text = cost.ToString();
        }
    }
}
