using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapScript : MonoBehaviour {

    public GameObject ArcherPrefab;
    public GameObject SwordPrefab;
    public GameObject MagicianPrefab;
    public GameObject ThiefPrefeb;
    public GameObject PriestPrefeb;
    public GameObject MonkPrefeb;

    public GameObject DefaultEffect;
	public GameObject FireEffect;
	public GameObject HealEffect;
	public GameObject ThunderEffect;
    public int selectedHeroIdx = -1;
    public bool isMine = false;
    public GameObject[] tiles = new GameObject[9];

    MapManager mapManager = null;
    
    GameObject[] characters = new GameObject[4];

    MapIndex curMouseOveredIndex = new MapIndex();
    MapIndex targetMouseOveredIndex = new MapIndex();
    Vector2 lastMouseClickedPosition = new Vector2();

    MapSelectState selectState = MapSelectState.NO_SELECT;
    bool isPrePositioning = false;
    bool isMyTurn = false;
    bool isMovable = false;
    int numOfCharacter = 0;
    int totalSettingNum = 0;

	// Use this for initialization
	void Start () {
        mapManager = GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        switch(selectState)
        {
            case MapSelectState.NO_SELECT:
                break;
            case MapSelectState.CHARACTER_SELECT:
                OnCharacterSelect();
                break;
            case MapSelectState.MOVE_SELECT:
                OnMoveSelect();
                break;
            case MapSelectState.ACT_RESION_SELECT:
                OnActResionSelect();
                break;
            case MapSelectState.ACT_PLAYING:
                break;
        }
	}

    void OnCharacterSelect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 10))
            {
                var character = hit.collider.gameObject;
                if (!character.GetComponent<CharacterScript>().isMine)
                    return;

                CharacterScript script = character.GetComponent<CharacterScript>();
                selectedHeroIdx = script.SelectHero();
                mapManager.CharaterSelect(this, script, isPrePositioning);
            }
        }
        return;
    }

    void OnMoveSelect()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ChangeState(MapSelectState.CHARACTER_SELECT);
        }

        if (isMovable)
        {
            var movableIndexes = GetMovableIndexes();
            foreach (MapIndex index in movableIndexes)
            {
                GameObject tile = GetTile(index);
                if (tile != null)
                {
                    var tileScript = tile.GetComponent<TileScript>();
                    if (tileScript.GetIndex() != curMouseOveredIndex)
                    {
                        tileScript.ChangeTileState(TileScript.TileState.MOVABLE);
                    }
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit tileHit;

            if (Physics.Raycast(ray, out tileHit, Mathf.Infinity, 1 << 11))
            {

                TileScript tile = tileHit.collider.gameObject.GetComponent<TileScript>();
                if (!tile.isMine)
                    return;

                curMouseOveredIndex.posX = tile.x;
                curMouseOveredIndex.posY = tile.y;

                bool isMovableIndex = false;
                foreach (var index in movableIndexes)
                {
                    if (index.Equals(curMouseOveredIndex))
                    {
                        isMovableIndex = true;
                        break;
                    }
                }

                if (!isMovableIndex)
                    return;
      
                tile.ChangeTileState(TileScript.TileState.MOVE);
                if (Input.GetMouseButtonDown(0))
                {
                    lastMouseClickedPosition = Input.mousePosition;
                    mapManager.RequestMove(GetSelectedCharacter(), curMouseOveredIndex, isPrePositioning);
                    ChangeState(MapSelectState.NO_SELECT);
                }

            }
        }
        return;
    }

    void OnActResionSelect()
    {
        if(Input.GetMouseButtonDown(1))
        {
            ChangeState(MapSelectState.MOVE_SELECT);
        }

        ClearAllTile();

        CharacterScript curHero = GetSelectedCharacter();

        foreach (MapIndex index in curHero.GetAttackableRanges())
        {
            GameObject tile = mapManager.GetTile(curHero.CurrentSkillIsOnMyField(), index);
            if (tile != null)
            {
                var tileScript = tile.GetComponent<TileScript>();
                if (tileScript.GetIndex() != targetMouseOveredIndex)
                {
                    tileScript.ChangeTileState(TileScript.TileState.ATTACKABLE);
                }
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit tileHit;

        if (Physics.Raycast(ray, out tileHit, Mathf.Infinity, 1 << 11))
        {
            TileScript tile = tileHit.collider.gameObject.GetComponent<TileScript>();
            if (tile.isMine != curHero.CurrentSkillIsOnMyField())
                return;

            targetMouseOveredIndex.posX = tile.x;
            targetMouseOveredIndex.posY = tile.y;

            bool isAttackableIndex = false;
            foreach (var index in curHero.GetAttackableRanges())
            {
                if (index.Equals(targetMouseOveredIndex))
                {
                    isAttackableIndex = true;
                    break;
                }
            }

            if (!isAttackableIndex)
            {
                return;
            }

            foreach(var pos in curHero.GetAttackableEffects(targetMouseOveredIndex))
            {
                mapManager.GetTile(curHero.CurrentSkillIsOnMyField(), pos).GetComponent<TileScript>().ChangeTileState(TileScript.TileState.ATTACK);
            }

            if (Input.GetMouseButtonDown(0))
            {
                mapManager.RequestAction(GetSelectedCharacter(), targetMouseOveredIndex);
            }
        }
        return;
    }

    public void ChangeState(MapSelectState newState)
    {
        if (newState == selectState)
            return;

        Debug.Log("State Change:" + selectState + "->" + newState);

        OnStateEnd(selectState);
        OnStateStart(newState);
        selectState = newState;
    }

    void OnStateEnd(MapSelectState exitState)
    {
        switch (exitState)
        {
            case MapSelectState.NO_SELECT:
                return;

            case MapSelectState.CHARACTER_SELECT:
                return;

            case MapSelectState.MOVE_SELECT:
                ClearAllTile();
                isMovable = false;
                return;


            case MapSelectState.ACT_RESION_SELECT:
                ClearAllTile();
                return;

            case MapSelectState.ACT_PLAYING:
                return;

            default:
                return;
        }
    }

    void OnStateStart(MapSelectState startState)
    {
        switch (startState)
        {
            case MapSelectState.NO_SELECT:
                ClearAllTile();
                return;
            
            case MapSelectState.CHARACTER_SELECT:
                ClearAllTile();
                return;
            
            case MapSelectState.MOVE_SELECT:
                isMovable = true;
                return;

            case MapSelectState.ACT_RESION_SELECT:
                return;

            case MapSelectState.ACT_PLAYING:
                return;

            default:
                return;
        }
    }

    public void SetIsMine(bool mine)
    {
        isMine = mine;
        foreach(var tile in tiles)
        {
            tile.GetComponent<TileScript>().isMine = mine;
        }
    }

    public void UpdateHero(HeroStateModel model)
    {
        var hero = GetCharacter(model.index);
        if (hero == null)
            return;

        hero.UpdateState(model);
    }

    public void UpdateStatus(StateModel model)
    {
        var hero = GetCharacter(model.heroIdx);
        if (hero == null)
            return;

        hero.UpdateStatus(model);
    }

    public void MakeFormation()
    {
        selectedHeroIdx = -1;
        isPrePositioning = true;
        ChangeState(MapSelectState.CHARACTER_SELECT);
    }

    public void FormationEnd()
    {
        isPrePositioning = false;
        ChangeState(MapSelectState.NO_SELECT);
    }

    public void MyTurnStart()
    {
        isMyTurn = true;
        selectedHeroIdx = -1;
        ChangeState(MapSelectState.CHARACTER_SELECT);
    }

    public void MyTurnEnd()
    {
        isMyTurn = false;
        ChangeState(MapSelectState.NO_SELECT);
    }

    public void RejectPacket()
    {
        ChangeState(MapSelectState.CHARACTER_SELECT);
    }

    public void GetRandomCharacters(int[] characterTypes)
    {
        SetIsMine(true);
        numOfCharacter = characterTypes.Length;
		totalSettingNum = 0;

        for (int index = 0; index < numOfCharacter; ++index)
        {
            if (characters[index] != null)
			{
                mapManager.ResetHighlight();
                Object.Destroy(characters[index]);
			}

            HeroClass heroType = (HeroClass)(characterTypes[index] % (int)HeroClass.NUM);
            
            switch (heroType)
            {
                case HeroClass.ARCHER:
                    characters[index] = Instantiate(ArcherPrefab) as GameObject;
                    break;
                case HeroClass.FIGHTER:
                    characters[index] = Instantiate(SwordPrefab) as GameObject;
                    break;
                case HeroClass.MAGICIAN:
                    characters[index] = Instantiate(MagicianPrefab) as GameObject;
                    break;
                case HeroClass.THIEF:
                    characters[index] = Instantiate(ThiefPrefeb) as GameObject;
                    break;
                case HeroClass.PRIEST:
                    characters[index] = Instantiate(PriestPrefeb) as GameObject;
                    break;
                case HeroClass.MONK:
                    characters[index] = Instantiate(MonkPrefeb) as GameObject;
                    break;
                default:
                    break;
            };
            characters[index].transform.parent = transform;
            GetCharacter(index).PrePositioning(index);
        }
        selectedHeroIdx = -1;
    }

    public void GetFixedHerosAndPosition(HeroModel[] heroModels)
    {
        numOfCharacter = Mathf.Min(heroModels.Length, characters.Length);
        totalSettingNum = 0;
        for (int index = 0; index < numOfCharacter; ++index)
        {
            HeroModel heroData = heroModels[index];
            if (characters[index] != null)
            {
                mapManager.ResetHighlight();
                Object.Destroy(characters[index]);
            }

            HeroClass heroType = (HeroClass)((int)heroData.heroClass % (int)HeroClass.NUM);
            switch (heroType)
            {
                case HeroClass.ARCHER:
                    characters[index] = Instantiate(ArcherPrefab) as GameObject;
                    break;
                case HeroClass.FIGHTER:
                    characters[index] = Instantiate(SwordPrefab) as GameObject;
                    break;
                case HeroClass.MAGICIAN:
                    characters[index] = Instantiate(MagicianPrefab) as GameObject;
                    break;
                case HeroClass.THIEF:
                    characters[index] = Instantiate(ThiefPrefeb) as GameObject;
                    break;
                case HeroClass.PRIEST:
                    characters[index] = Instantiate(PriestPrefeb) as GameObject;
                    break;
                case HeroClass.MONK:
                    characters[index] = Instantiate(MonkPrefeb) as GameObject;
                    break;
                default:
                    break;
            };

            characters[index].transform.parent = gameObject.transform;
            GetCharacter(index).Index = index;
            GetCharacter(index).SetHud(GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>().GetNewHud());
            GetCharacter(index).Initialize(heroData);
        }
    }


    public void CharacterActionEnd()
    {
        if (isMyTurn)
        {
            mapManager.CharaterSelect(this, GetSelectedCharacter(), isPrePositioning);
        }
        else if(isPrePositioning)
        {
            ChangeState(MapSelectState.CHARACTER_SELECT);
        }
    }

    public void CharacterTurnOver()
    {
        if (isMyTurn)
        {
            selectedHeroIdx = -1;
            ChangeState(MapSelectState.CHARACTER_SELECT);
        }
    }

    public void OnChracterDie(int characterIdx)
    {
        selectedHeroIdx = -1;
        mapManager.ResetHighlight();
        GetCharacter(characterIdx).Dead();
        characters[characterIdx] = null;
    }

	public void MakeSkillEffect(MapIndex position, SkillType type)
    {
        GameObject effect;

		//일단 피격 사운드는 디폴트로
		AudioManager.instance.PlaySfx(mapManager.defaultHit);

		switch (type)
		{
			case SkillType.PRIEST_HEAL:
				effect = Instantiate(HealEffect) as GameObject;
				break;
			case SkillType.MAGICIAN_FIRE_BLAST:
				effect = Instantiate(FireEffect) as GameObject;
				break;
			case SkillType.MAGICIAN_THUNDER_STORM:
				effect = Instantiate(ThunderEffect) as GameObject;
				break;
			default:
				effect = Instantiate(DefaultEffect) as GameObject;
				break;
		}
        effect.transform.parent = transform;
        Vector3 effectPos = GetTile(position).transform.localPosition;
        effect.transform.localPosition = new Vector3(effectPos.x, 2.0f, effectPos.z);
    }

    public GameObject GetTile(MapIndex index)
    {
        if (index.IsValid())
            return tiles[index.posX + index.posY * 3];
        else
            return null;
    }

    public CharacterScript GetCharacter(int heroIdx)
    {
        if (characters[heroIdx] == null)
            return null;

        return characters[heroIdx].GetComponent<CharacterScript>();
    }

    public CharacterScript GetSelectedCharacter()
    {
        if(selectedHeroIdx == -1) 
            return null;

        return characters[selectedHeroIdx].GetComponent<CharacterScript>();
    }

    public List<CharacterScript> GetCharacters()
    {
        List<CharacterScript> result = new List<CharacterScript>();
        foreach(var charac in characters)
        {
            result.Add(charac.GetComponent<CharacterScript>());
        }
        return result;
    }

    public void ClearTile()
    {
        foreach (var tile in tiles)
        {
            tile.GetComponent<TileScript>().ChangeTileState(TileScript.TileState.NORMAL);
        }
    }

    void ClearAllTile()
    {
        mapManager.ClearAllTile();
    }


    public bool IsAllCharacterOnPosition()
    {
        foreach(var hero in characters)
        {
            if (!hero.GetComponent<CharacterScript>().isOnPosition)
                return false;
        }
        return true;
    }

    List<MapIndex> GetMovableIndexes()
    {
        List<MapIndex> resultList = new List<MapIndex>();
        if(selectedHeroIdx != -1)
        {
            if (isPrePositioning)
            {
                for (int xIdx = 0; xIdx < 3; ++xIdx) 
                    for (int yIdx = 0; yIdx < 3; ++yIdx)
                    {
                        resultList.Add(new MapIndex(xIdx, yIdx));
                    }
            }
            else
            {
                MapIndex stdIndex = GetCharacter(selectedHeroIdx).Pos;
                resultList.Add(new MapIndex(stdIndex.posX + 1, stdIndex.posY));
                resultList.Add(new MapIndex(stdIndex.posX - 1, stdIndex.posY));
                resultList.Add(new MapIndex(stdIndex.posX, stdIndex.posY + 1));
                resultList.Add(new MapIndex(stdIndex.posX, stdIndex.posY - 1));
                resultList.Add(stdIndex);
            }
        }
        return resultList;
    }
}

public class MapIndex
{
    public MapIndex()
    {
        posX = 0;
        posY = 0;
    }

    public MapIndex(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public MapIndex(MapIndex other)
    {
        posX = other.posX;
        posY = other.posY;
    }

    private int maxX = 3;
    private int maxY = 3;

    public int posX = 1;
    public int posY = 1;
    
    public override string ToString() 
    {
        return "(" + posX + "," + posY + ")";
    }

    public bool IsValid()
    {
        return posX >= 0 && posX < maxX && posY >= 0 && posY < maxY;
    }

    public bool Equals(MapIndex other)
    {
        return posX == other.posX && posY == other.posY;
    }
}

public class EffectRange
{
    public EffectRange(int x, int y)
    {
        relativeX = x;
        relativeY = y;
    }

    public int relativeX = 0;
    public int relativeY = 0;
}

public enum MapSelectState
{
    NO_SELECT,
    CHARACTER_SELECT,
    MOVE_SELECT,
    ACT_RESION_SELECT,
    ACT_PLAYING,
}