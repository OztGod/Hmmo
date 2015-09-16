using UnityEngine;
using System.Collections;

public class SkillLevelContainer : MonoBehaviour {
    public Sprite OnSprite;
    public Sprite OffSprite;

    UI2DSprite[] levelSigns = new UI2DSprite[3];
	// Use this for initialization
    void Awake()
    {
        for(int i= 0;i < levelSigns.Length; ++i)
        {
            levelSigns[i] = transform.FindChild("levelSign" + i).GetComponent<UI2DSprite>();
            levelSigns[i].sprite2D = OffSprite;
        }
	}

    public void SetLevel(int level)
    {
        int idx = 0;
        for(;idx < level; ++idx)
        {
            levelSigns[idx].sprite2D = OnSprite;
        }

        for(;idx < levelSigns.Length;++idx)
        {
            levelSigns[idx].sprite2D = OffSprite;
        }
    }
}
