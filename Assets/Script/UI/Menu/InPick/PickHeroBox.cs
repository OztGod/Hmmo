using UnityEngine;
using System.Collections;

public class PickHeroBox : MonoBehaviour {
    public UI2DSprite sprite;
    public UILabel label;
    public int index;
	
    public void SetBox(HeroClass heroType)
    {
        var uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<LobbyUI>();
        sprite.sprite2D = uiManager.GetClassSprite(heroType);
        label.text =heroType.ToString();
    }
	
}
