﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    public GameObject HudPrefeb = null;
    public int InitHudNum = 10;
    
    List<GameObject> FreeHuds = new List<GameObject>();
    List<GameObject> UsingHuds = new List<GameObject>();

	// Use this for initialization
	void Start () 
    {
	    for(int i= 0 ; i < InitHudNum; ++i)
        {
            GameObject newHud = Instantiate(HudPrefeb) as GameObject;
            newHud.SetActive(false);
            FreeHuds.Add(newHud);
        }
	}
	
    public void ReleaseHud(GameObject hudObject)
    {
        GameObject releaseObject = null;
        for(int idx = 0; idx < UsingHuds.Count; ++idx)
        {
            if(UsingHuds[idx] == hudObject)
            {
                releaseObject = UsingHuds[idx];
                UsingHuds.RemoveAt(idx);
                break;
            }
        }

        if (releaseObject == null)
            return;

        releaseObject.SetActive(false);
        FreeHuds.Add(releaseObject);
    }

    public StatusHUDScript GetNewHud()
    {
        if (FreeHuds.Count <= 0)
        {
            for (int i = 0; i < UsingHuds.Count; ++i)
            {
                GameObject newHud = Instantiate(HudPrefeb) as GameObject;
                FreeHuds.Add(newHud);
            }
        }

        GameObject retHud = null;
        retHud = FreeHuds[0];
        FreeHuds.Remove(retHud);
        UsingHuds.Add(retHud);
        retHud.SetActive(true);
        return retHud.GetComponent<StatusHUDScript>();
    }
}
