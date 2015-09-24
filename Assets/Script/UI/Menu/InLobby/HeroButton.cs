using UnityEngine;
using System.Collections;

public class HeroButton : MonoBehaviour {
    public int index;
    public UI2DSprite sprite;
    public UILabel label;
    public HeroWindow window;
    public bool IsActiveButton = true;
    LobbyUI uiManager;
	// Use this for initialization
	void Start () 
    {
        uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<LobbyUI>();
        UserScript user = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
        SetBox(user.OwnHeroInfos[index].heroType, user.OwnHeroInfos[index].level);
        gameObject.GetComponent<UIButton>().isEnabled = IsActiveButton;
	}

    void SetBox(HeroClass heroClass, int level)
    {
        sprite.sprite2D = uiManager.GetClassSprite(heroClass);
        label.text = "Lv " + level + "  " + heroClass.ToString();
    }

    void OnClick()
    {
        window.OnHeroButtonClick(index);
    }

    public void SetDisable()
    {
        GetComponent<UIButton>().isEnabled = false;
    }
}
