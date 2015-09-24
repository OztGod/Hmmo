using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class HeroWindow : MonoBehaviour {
    public List<GameObject> HeroModels;
    public List<HeroButton> HeroButtons;
    public int SelectedIdx = -1;
    UserScript user;
	// Use this for initialization
	void Start () 
    {
        user = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
	}

    void SelectHero(int selectedIdx)
    {
        ShowModel(user.OwnHeroInfos[selectedIdx].heroType);
        SelectedIdx = selectedIdx;
    }

    void ShowModel(HeroClass heroType)
    {
        foreach(var model in HeroModels)
        {
            model.SetActive(false);
        }

        HeroModels[(int)heroType].SetActive(true);
    }

    public void OnHeroButtonClick(int selectedIdx)
    {
        SelectHero(selectedIdx);
    }

    public void DisableButton(int index)
    {
        HeroButtons[index].SetDisable();
    }
}
