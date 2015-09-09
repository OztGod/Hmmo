using UnityEngine;
using System.Collections;

public class DmgLabelController : MonoBehaviour {
    public GameObject dmgLabelPrefeb;
	// Use this for initialization
	
    public void MakeDamageLabel(int value)
    {
        GameObject newLabel = Instantiate(dmgLabelPrefeb) as GameObject;
        newLabel.transform.parent = transform;
        newLabel.GetComponent<LabelEffectScript>().InitLabel(value);
    }
}
