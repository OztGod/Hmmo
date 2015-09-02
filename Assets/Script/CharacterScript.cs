using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterScript : MonoBehaviour 
{
    public float Speed = 2.0f;
    public float FixedVerticalPos = 0.5f;
    public bool isMine = false;

    Vector3 originalDirection;
    HeroClass heroClass = HeroClass.ARCHER;
    List<SkillModel> skills = new List<SkillModel>();
    public List<SkillModel> Skills { get { return skills; } }
    List<StateType> states = new List<StateType>();
    public List<StateType> States { get { return states; } }

    int index = -1;
    public int Index { get { return index; } set { index = value; } }
    int posX = 0;
    public int PosX { get { return posX; } }
    int posY = 0;
    public int PosY { get { return posY; } }
    int maxHp = 0;
    public int MaxHP { get { return maxHp; } }
    int currentHp = 0;
    public int CurrentHP { get { return currentHp; } }
    int maxAp = 0;
    public int MaxAp { get { return maxAp; } }
    int currentAp = 0;
    public int CurrentAp { get { return currentAp; } }


	// Use this for initialization
	void Start () 
	{
        originalDirection = transform.eulerAngles;
        isMine = transform.parent.GetComponent<MapScript>().isMine;
	}

    void Update()
    {
    }
    
    public string GetInfoString()
    {
        return "[" + index + "]" + heroClass.ToString() + "(" + currentHp + "/" + currentAp + ")";
    }

    public void UpdateState(HeroStateModel model)
    {
        currentHp = model.hp;
        currentAp = model.act;
    }

    public int SelectHero()
    {
        GetComponent<Animation>().Play("att01");
        GetComponent<Animation>().PlayQueued("idle", QueueMode.CompleteOthers);
        return index;
    }

    public void SetPosition(int x, int y)
    {
        posX = x;
        posY = y;
        transform.localPosition = new Vector3(posX * 3, FixedVerticalPos, posY * 3);
    }

    public void PrePositioning(int idx)
    {
        index = idx;
        transform.localRotation = transform.rotation;
        transform.localPosition = new Vector3(-3, FixedVerticalPos, index * 9 / 4);
    }

    public void Initialize(HeroModel data)
    {
        heroClass = data.heroClass;
        maxHp = data.hp;
        currentHp = data.hp;
        maxAp = data.ap;
        currentAp = data.ap;
        SetPosition(data.position.posX, data.position.posY);
        transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
    }

    public void SetSkill(List<SkillModel> skillModels)
    {
        skills.Clear();
        foreach(var skill in skillModels)
        {
            skills.Add(skill);
        }
    }

    public void Move(Vector3 position, int x, int y)
    {
        posX = x;
        posY = y;
        GetComponent<Animation>().Play("run");
        double distance = (transform.position - position).magnitude;
        Hashtable hash = new Hashtable();
        hash.Add("x", position.x);
        hash.Add("z", position.z);
        hash.Add("time", distance / Speed);
        hash.Add("orienttopath", true);
        hash.Add("oncomplete", "OnActionComplete");
        hash.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(gameObject, hash);
    }

    public void SkillAction(SkillType skill)
    {
        GetComponent<Animation>().Play("att01");
        WaitForAnimation();
    }

    void OnActionComplete()
    {
        GetComponent<Animation>().Play("idle");
        transform.eulerAngles = originalDirection;
        transform.parent.GetComponent<MapScript>().CharacterActionEnd();
    }

    IEnumerator WaitForAnimation()
    {
        yield return null;
        Animation animation = GetComponent<Animation>();
        while (animation.isPlaying)
        {
            yield return null;
        };
        OnActionComplete();
    }
}

public class HeroModel
{
    public MapIndex position = new MapIndex();
    public HeroClass heroClass;
    public int hp;
    public int ap;
}

public class HeroStateModel
{
    public int index;
    public int hp;
    public int act;
    public MapIndex position = new MapIndex();
}

public class SkillModel
{
    public SkillType type;
    public int level;
}


public enum HeroClass
{
    FIGHTER = 0,
    MAGICIAN = 1,
    ARCHER = 2,
    THIEF = 3,
    PRIEST = 4,
    MONK = 5,
    NUM = 6,
}
public enum StateType
{
    STATE_MOVE_IMMUNE = 0,
    STATE_IRON = 1,
    STATE_POSION = 2,
    STATE_ICE = 3,
    STATE_BURN = 4,
    STATE_POLYMOPH = 5,
    STATE_BUFF = 6,
    STATE_TAUNT = 7,
    STATE_SACRIFICE = 8,
    STATE_PRAY = 9,
}
public enum SkillType
{
    FIGHTER_ATTACK = 0,
    FIGHTER_CHARGE = 1,
    FIGHTER_HARD = 2,
    FIGHTER_IRON = 3,
    MAGICIAN_ICE_ARROW = 4,
    MAGICIAN_FIRE_BLAST = 5,
    MAGICIAN_THUNDER_STORM = 6,
    MAGICIAN_POLYMORPH = 7,
    ARCHER_ATTACK = 8,
    ARCHER_BACK_ATTACK = 9,
    ARCHER_PENETRATE_SHOT = 10,
    ARCHER_SNIPE = 11,
    THIEF_ATTACK = 12,
    THIEF_BACK_STEP = 13,
    THIEF_POISON = 14,
    THIEF_TAUNT = 15,
    PRIEST_HEAL = 16,
    PRIEST_ATTACK = 17,
    PRIEST_BUFF = 18,
    PRIEST_REMOVE_MAGIC = 19,
    MONK_ATTACK = 20,
    MONK_SACRIFICE = 21,
    MONK_PRAY = 22,
    MONK_KICK = 23,
}