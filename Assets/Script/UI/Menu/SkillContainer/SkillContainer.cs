using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillContainer : MonoBehaviour {

    List<SkillUIScript> skillUIs = new List<SkillUIScript>();
	// Use this for initialization
    void Awake() 
    {
	    for(int i =0 ;i < 4;++i)
        {
            skillUIs.Add(transform.FindChild("Skill" + i).GetComponent<SkillUIScript>());
            skillUIs[i].skillIdx = i;
        }
	}
	
    public void Reset()
    {
        foreach(var skillUI in skillUIs)
        {
            skillUI.Reset();
        }
    }

	public void SetSkillUIs(List<SkillModel> models)
    {
        Reset();
        for(int i = 0 ; i < models.Count; ++i)
        {
            skillUIs[i].SetSkillUI(models[i]);
        }
    }
}
