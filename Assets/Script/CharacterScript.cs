using UnityEngine;
using System.Collections;

public class CharacterScript : MonoBehaviour 
{
    public double speed = 1.0;
    Vector3 originalDirection;

    HeroClass heroClass = HeroClass.ARCHER;
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
	}

    void Update()
    {
    }
    
    public string GetInfoString()
    {
        string classString = "";
        switch(heroClass)
        {
            case HeroClass.FIGHTER:
                classString = "FIGHTER";
                break;
            case HeroClass.MAGICIAN:
                classString = "MAGICIAN";
                break;
            case HeroClass.ARCHER:
                classString = "ARCHER";
                break;
            case HeroClass.THIEF:
                classString = "THIEF";
                break;
            case HeroClass.PRIEST:
                classString = "PRIEST";
                break;
            case HeroClass.MONK:
                classString = "MONK";
                break;
        }

        return "[" + index + "]" + classString + "(" + currentHp + "/" + currentAp + ")";
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
        transform.localPosition = new Vector3(posX * 3, 0, posY * 3);
    }

    public void PrePositioning(int idx)
    {
        index = idx;
        transform.localRotation = transform.rotation;
        transform.localPosition = new Vector3(-3, 0, index * 9 / 4);
    }

    public void Initialize(HeroModel data)
    {
        heroClass = data.heroClass;
        maxHp = data.hp;
        currentHp = data.hp;
        maxAp = data.ap;
        currentAp = data.ap;
        SetPosition(data.position.posX, data.position.posY);
        transform.localRotation = transform.parent.rotation;
    }

    public void Move(Vector3 position, int x, int y)
    {
        posX = x;
        posY = y;
        GetComponent<Animation>().Play("run");
        double distance = (transform.position - position).magnitude;
        Hashtable hash = new Hashtable();
        hash.Add("position", position);
        hash.Add("time", distance / speed);
        hash.Add("orienttopath", true);
        hash.Add("oncomplete", "OnMoveComplete");
        hash.Add("easetype", iTween.EaseType.linear);
        iTween.MoveTo(gameObject, hash);
    }

    void OnMoveComplete()
    {
        GetComponent<Animation>().Play("idle");
        transform.eulerAngles = originalDirection;
        transform.parent.GetComponent<MapScript>().CharacterActionEnd();
    }
}
