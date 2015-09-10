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
    StatusHUDScript Hud = null;
    bool isDead = false;
    
    int currentSkillIdx = -1;
    public int CurrentSkillIdx { get { return currentSkillIdx; } set { currentSkillIdx = value; } }
    List<SkillModel> skills = new List<SkillModel>();
    public List<SkillModel> Skills { get { return skills; } }
    List<StateType> states = new List<StateType>();
    public List<StateType> States { get { return states; } }

    int index = -1;
    public int Index { get { return index; } set { index = value; } }
    int posX = -1;
    public int PosX { get { return posX; } }
    int posY = -1;
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

    public string GetInfoString()
    {
        return "[" + index + "]" + heroClass.ToString() + "(" + currentHp + "/" + currentAp + ")";
    }

    public void SetHud(StatusHUDScript hud)
    {
        Hud = hud;
        Hud.SetTarget(transform);
    }

    public void UpdateState(HeroStateModel model)
    {
        Move(model.position.posX, model.position.posY, model.isForcedMove);
        if (Hud == null)
            return;

        int dmg = model.hp - currentHp;

        currentHp = model.hp;
        currentAp = model.act;

        if(dmg != 0)
        {
            Hud.OnDamage(dmg);
        }
        Hud.SetHp(maxHp, currentHp);
        Hud.SetAp(maxAp, currentAp);
    }

    public int SelectHero()
    {
        GetComponent<Animation>().Play("att02");
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

        if (Hud == null)
            return;

        Hud.SetHp(maxHp, currentHp);
        Hud.SetAp(maxAp, currentAp);
    }

    public void SetSkill(List<SkillModel> skillModels)
    {
        skills.Clear();
        foreach(var skill in skillModels)
        {
            skills.Add(skill);
        }
		Debug.Log(index + " : skills num = " + skills.Count);
    }

	public SkillType GetSkillType(int skillidx)
	{
		Debug.Log(index + " : skills num = " + skills.Count);
		return skills[skillidx].type;
	}

    public void SetSkillRange(int skillIdx, List<MapIndex> positions, bool isOnMyField)
    {
        SkillModel skill = skills[skillIdx];
        skill.attackableRanges.Clear();
        skill.attackableEffects.Clear();

        skill.isOnMyField = isOnMyField;
        skill.attackableRanges = positions;
    }

    public bool SetSkillEffect(int skillIdx, List<EffectRange> effects)
    {
        SkillModel skill = skills[skillIdx];
        skill.attackableEffects.Add(effects);
        bool result = (skill.attackableEffects.Count == skill.attackableRanges.Count);
        return result;
    }

    public List<MapIndex> GetAttackableRanges()
    {
        return skills[currentSkillIdx].attackableRanges;
    }

    public List<MapIndex> GetAttackableEffects(MapIndex stdIndex)
    {
        List<MapIndex> resultList = new List<MapIndex>();
        SkillModel curSkill = skills[currentSkillIdx];
        int rangeIdx = -1;
        var ranges = curSkill.attackableRanges;
        Debug.Log("[DEBUG] GetAttackableEffects with" + stdIndex + " currentSkillIdx:" + currentSkillIdx);
        for (int i = 0; i < ranges.Count; ++i)
        {
            Debug.Log("current attackableRange" + ranges[i]);
            if (stdIndex.Equals(ranges[i]))
            {
                rangeIdx = i;
                break;
            }
        }


        foreach (var range in curSkill.attackableEffects[rangeIdx])
        {
            MapIndex newPos = new MapIndex();
            newPos.posX = stdIndex.posX + range.relativeX;
            newPos.posY = stdIndex.posY + range.relativeY;
            Debug.Log("Attackable Pos:" + newPos);
            resultList.Add(newPos);
        }
        return resultList;
    }

    public void Move(int x, int y, bool isForcedMove)
    {
        if(isForcedMove)
        {
            Debug.Log("OriginPos:(" + posX + "," + posY + ")NewPos:(" + x + "," + y + ")");
        }

        if (x == posX && y == posY)
            return;

        posX = x;
        posY = y;

        Vector3 position = transform.parent.TransformPoint(new Vector3(posX * 3, FixedVerticalPos, posY * 3));
        double time = 0.0;
        double distance = (transform.position - position).magnitude;

        if (isForcedMove)
        {
            time = distance / (Speed * 3);
            Debug.Log("Forced Move === Time:" + time);
            GetComponent<Animation>().Play("jump");
        }
        else
        {
            time = distance / Speed;
            GetComponent<Animation>().Play("run");
        }
        
        Hashtable hash = new Hashtable();
        hash.Add("x", position.x);
        hash.Add("z", position.z);
        hash.Add("time", time);
        hash.Add("orienttopath", true);
        hash.Add("oncomplete", "OnActionComplete");
        hash.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(gameObject, hash);
    }

    public void SkillAction(SkillType skill)
    {
        GetComponent<Animation>().Play("att01");
    }

    public void Dead()
    {
        isDead = true;
        GetComponent<Animation>().Play("die");
        Hud.Release();
        Hashtable hash = new Hashtable();
    }

    public bool CurrentSkillIsOnMyField()
    {
        return skills[currentSkillIdx].isOnMyField;
    }

    void OnDeadEnd()
    {
        Hashtable hash = new Hashtable();
        hash.Add("y", transform.position.y - 3);
        hash.Add("time", 1.5);
        hash.Add("oncomplete", "OnDeadEndComplete");
        hash.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(gameObject, hash);
    }

    void OnDeadEndComplete()
    {
        Debug.Log("Dead Complete!");
        GameObject.Destroy(gameObject);
    }

    void OnActionComplete()
    {
        Debug.Log("Action Complete!");
        GetComponent<Animation>().Play("idle");
        transform.eulerAngles = originalDirection;
        transform.parent.GetComponent<MapScript>().CharacterActionEnd();
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
    public bool isForcedMove = false;
    public int index;
    public int hp;
    public int act;
    public MapIndex position = new MapIndex();
}

public class SkillModel
{
    public SkillType type;
    public int level;
    public List<MapIndex> attackableRanges = new List<MapIndex>();
    public List<List<EffectRange>> attackableEffects = new List<List<EffectRange>>();
    public bool isOnMyField;
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
    STATE_IRON = 0,
    STATE_POISON = 1,
    STATE_ICE = 2,
    STATE_BURN = 3,
    STATE_BUFF = 4,
    STATE_TAUNT = 5,
    STATE_SACRIFICE = 6,
    STATE_PRAY = 7,
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