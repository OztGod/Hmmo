using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateContainer : MonoBehaviour {
	// Use this for initialization
    public GameObject IconPrefeb;
    UIGrid grid;
    UIManager uiManager;
    List<StateIcon> icons = new List<StateIcon>();
    List<int> delList = new List<int>();

    void Awake()
    {
        uiManager = GameObject.FindWithTag("UI").GetComponent<UIManager>();
        grid = gameObject.GetComponent<UIGrid>();
    }

    public void UpdateStatus(int id, StateType type, int duration)
    {
        var icon = GetIcon(id);
        if(icon == null)
        {
            icon = NGUITools.AddChild(gameObject, IconPrefeb).GetComponent<StateIcon>();
            icon.InitState(type, duration, id);
            icons.Add(icon);
            grid.Reposition();
        }
        else
        {
            icon.UpdateState(duration);
        }
    }

    public void RemoveStatus(int id)
    {
        var icon = GetIcon(id);
        if (icon == null)
            return;

        NGUITools.Destroy(icon.gameObject);
    }

    StateIcon GetIcon(int id)
    {
        foreach(var icon in icons)
        {
            if (icon.id == id)
                return icon;
        }
        return null;
    }

}
