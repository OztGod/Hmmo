using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PickWindow : MonoBehaviour {
    public List<PickHeroBox> PickBoxes;
	int PickCount = 0;
    public void PickHero(HeroClass heroType)
    {
        PickBoxes[PickCount++].SetBox(heroType);
    }
}
