using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour {
    public List<Sprite> SkillSprites = new List<Sprite>();
    public List<Sprite> ClassSprites = new List<Sprite>();
    public List<string> SkillDescs = new List<string>();

    public Sprite GetSkillSprite(SkillType type)
    {
        return SkillSprites[(int)type];
    }

    public string GetSkillDesc(SkillType type)
    {
        return SkillDescs[(int)type];
    }

    public Sprite GetClassSprite(HeroClass type)
    {
        return ClassSprites[(int)type];
    }

    
}
