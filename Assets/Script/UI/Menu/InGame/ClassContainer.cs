using UnityEngine;
using System.Collections;

public class ClassContainer : MonoBehaviour {
    UILabel label;
    UI2DSprite sprite;
    UIManager uiManager;
	// Use this for initialization
	void Awake () 
    {
        label = transform.FindChild("Label").GetComponent<UILabel>();
        uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        sprite = GetComponent<UI2DSprite>();
	}
	
    public void Reset()
    {
        sprite.sprite2D = uiManager.GetClassSprite(HeroClass.NUM);
        label.text = "Lv 0    Nobody";
    }

	public void SetClass(HeroClass type, int level)
    {
        sprite.sprite2D = uiManager.GetClassSprite(type);
        label.text = "Lv " + level.ToString() + "    " + type.ToString();
    }
}
