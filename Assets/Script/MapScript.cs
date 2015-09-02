using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapScript : MonoBehaviour {

    public GameObject TilePrefab;
    public GameObject ArcherPrefab;
    public GameObject SwordPrefab;
    public GameObject MagicianPrefab;
    public GameObject Anchor;
    public GameObject BombEffect;
    public int selectedHeroIdx = -1;
    public bool isMine = false;

    SocketScript network = null;
    MapManager mapManager = null;
    UIManager uiManager = null;
    GameObject[] tiles = new GameObject[9];
    GameObject[] characters = new GameObject[4];
    MapIndex[] mapCharacterIndexes = new MapIndex[4];
    List<MapIndex> attackableIndexes = new List<MapIndex>();
    List<EffectRange> attackableRange = new List<EffectRange>();
    MapIndex curMouseOveredIndex = new MapIndex();
    MapIndex targetMouseOveredIndex = new MapIndex();
    Vector2 lastMouseClickedPosition = new Vector2();
    
    bool isPrePositioning = false;
    bool isMyTurn = false;
    bool isMovable = false;
    bool isOnMyField = false;

    public enum CharacterType
    {
        SWORD_MAN,
        MAGICIAN,
        ARCHER,
        MAX_NUM,
    }

    public enum MapSelectState
    {
        NO_SELECT,
        CHARACTER_SELECT,
        MOVE_SELECT,
        ACT_SELECT,
        ACT_RESION_SELECT,
        ACT_PLAYING,
    }

    int numOfCharacter = 0;
	int totalSettingNum = 0;
    int selectedSkillIdx = -1;
    MapSelectState selectState = MapSelectState.NO_SELECT;
    PanelScript menuPanel = null;

	// Use this for initialization
	void Start () {
        for(int x = 0; x < 3; ++x)
        {
            for(int y = 0; y < 3; ++y)
            {
                GameObject tile = Instantiate(TilePrefab) as GameObject;
                tiles[x + y*3] = tile;
                tile.GetComponent<TileScript>().SetIndex(x, y);
                tile.transform.parent = transform;
                tile.transform.localPosition = new Vector3(x * 3, 0, y * 3);
            }
        }

        network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
        mapManager = GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>();

        lastMouseClickedPosition.x = Screen.width / 2;
        lastMouseClickedPosition.y = Screen.height / 5;

        menuPanel = GameObject.FindGameObjectWithTag("UI").transform.FindChild("Menu").GetComponent<PanelScript>();
        uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        menuPanel.SetMyMap(this);
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
            case MapSelectState.ACT_SELECT:
                OnActSelect();
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

                selectedHeroIdx = character.GetComponent<CharacterScript>().SelectHero();
                Anchor.transform.position = new Vector3(character.transform.position.x, 13.5f, character.transform.position.z);
                Anchor.transform.parent = character.transform;
                menuPanel.SetActionButtons(character.GetComponent<CharacterScript>().Skills);

                if (isPrePositioning)
                {
                    ConfirmHeroSkills(null);
                }
                else
                {
                    network.SelectHero(selectedHeroIdx);
                }
            }
        }
        return;
    }

    void OnMoveSelect()
    {
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
                        CharacterMoveRequest(curMouseOveredIndex);
                        isMovable = false;
                }

            }
        }
        return;
    }

    void OnActSelect()
    {
        OnCharacterSelect();
        return;
    }

    void OnActResionSelect()
    {
        if(Input.GetMouseButtonDown(1))
        {
            ChangeState(MapSelectState.MOVE_SELECT);
        }

        foreach (MapIndex index in attackableIndexes)
        {
            GameObject tile = mapManager.GetTile(isOnMyField, index);
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
            if (tile.isMine != isOnMyField)
                return;

            targetMouseOveredIndex.posX = tile.x;
            targetMouseOveredIndex.posY = tile.y;

            bool isAttackableIndex = false;
            foreach (var index in attackableIndexes)
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

            foreach(var pos in GetAttackableRanges(targetMouseOveredIndex))
            {
                mapManager.GetTile(isOnMyField, pos).GetComponent<TileScript>().ChangeTileState(TileScript.TileState.ATTACK);
            }

            if (Input.GetMouseButtonDown(0))
            {
                network.RequestSkillAction(selectedHeroIdx, targetMouseOveredIndex, selectedSkillIdx);
                Debug.Log("Req Skill:" + "pos=" + targetMouseOveredIndex.posX + "," + targetMouseOveredIndex.posY + " skillIdx=" + selectedSkillIdx);
                ChangeState(MapSelectState.ACT_PLAYING);
            }
        }
        return;
    }

    void ChangeState(MapSelectState newState)
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

            case MapSelectState.ACT_SELECT:
                if (!isPrePositioning)
                    menuPanel.CloseMenu();
                return;

            case MapSelectState.MOVE_SELECT:
                ClearTile();
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
                return;
            
            case MapSelectState.CHARACTER_SELECT:
                return;
            
            case MapSelectState.MOVE_SELECT:
                if (!isPrePositioning)
                    menuPanel.OpenMenu();
                isMovable = true;
                return;
            
            case MapSelectState.ACT_SELECT:
                if (!isPrePositioning)
                    menuPanel.OpenMenu();
                attackableIndexes.Clear();
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

    public void SynchronizeState(HeroStateModel model)
    {
        GameObject hero = characters[model.index];
        mapCharacterIndexes[model.index] = model.position;
        Vector3 newPos = GetTile(model.position).transform.position;
        hero.GetComponent<CharacterScript>().Move(newPos, model.position.posX, model.position.posY);
        hero.GetComponent<CharacterScript>().UpdateState(model);
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
        network.RequestTurnEnd();
        menuPanel.CloseMenu();
    }

    public void RejectPacket()
    {
        ChangeState(MapSelectState.CHARACTER_SELECT);
    }

    public void ConfirmHeroSkills(List<int> validIdx)
    {
        if (validIdx != null)
        {
            menuPanel.SetActiveButtons(validIdx);
        }
        ChangeState(MapSelectState.MOVE_SELECT);
    }

    public void GetRandomCharacters(int[] characterTypes)
    {
        SetIsMine(true);
        numOfCharacter = characterTypes.Length;
		totalSettingNum = 0;

        for (int index = 0; index < numOfCharacter; ++index)
        {
			mapCharacterIndexes[index] = null;
            if (characters[index] != null)
			{
                Anchor.transform.parent = null;
                Object.Destroy(characters[index]);
			}

            CharacterType type = (CharacterType)(characterTypes[index] % (int)CharacterType.MAX_NUM);
            
            switch (type)
            {
                case CharacterType.ARCHER:
                    characters[index] = Instantiate(ArcherPrefab) as GameObject;
                    break;
                case CharacterType.SWORD_MAN:
                    characters[index] = Instantiate(SwordPrefab) as GameObject;
                    break;
                case CharacterType.MAGICIAN:
                    characters[index] = Instantiate(MagicianPrefab) as GameObject;
                    break;
                default:
                    break;
            };
            characters[index].transform.parent = transform;
            characters[index].GetComponent<CharacterScript>().PrePositioning(index);
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
            mapCharacterIndexes[index] = heroData.position;
            if (characters[index] != null)
            {
                Anchor.transform.parent = null;
                Object.Destroy(characters[index]);
            }

            CharacterType type = (CharacterType)((int)heroData.heroClass % (int)CharacterType.MAX_NUM);

            switch (type)
            {
                case CharacterType.ARCHER:
                    characters[index] = Instantiate(ArcherPrefab) as GameObject;
                    break;
                case CharacterType.SWORD_MAN:
                    characters[index] = Instantiate(SwordPrefab) as GameObject;
                    break;
                case CharacterType.MAGICIAN:
                    characters[index] = Instantiate(MagicianPrefab) as GameObject;
                    break;
                default:
                    break;
            };
            characters[index].transform.parent = gameObject.transform;
            characters[index].GetComponent<CharacterScript>().Index = index;
            characters[index].GetComponent<CharacterScript>().SetHud(uiManager.GetNewHud());
            characters[index].GetComponent<CharacterScript>().Initialize(heroData);
        }
    }

    public void SetHeroSkills(int heroIdx, List<SkillModel> skillModels)
    {
        characters[heroIdx].GetComponent<CharacterScript>().SetSkill(skillModels);
    }

    public void CharacterActionEnd()
    {
        if (isMyTurn)
        {
            ChangeState(MapSelectState.ACT_SELECT);
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

    public GameObject GetTile(MapIndex index)
    {
        if (index.IsValid())
            return tiles[index.posX + index.posY * 3];
        else
            return null;
    }

    public void OnSkillClicked(int skillIndex)
    {
        selectedSkillIdx = skillIndex;
        network.RequestSkillRange(selectedHeroIdx, selectedSkillIdx);
        ChangeState(MapSelectState.ACT_RESION_SELECT);
    }

    public void OnSkillRangeReponse(List<MapIndex> mapRange, List<EffectRange> effectRange, bool isMyField)
    {
        Debug.Log("SkillRangeRes| isMyField:" + isMyField);
        attackableIndexes.Clear();
        Debug.Log("SkillRangeRes| mapRange");
        foreach(var index in mapRange)
        {
            Debug.Log("SkillRangeRes| isMyField:" + isMyField);
            attackableIndexes.Add(index);
        }

        attackableRange.Clear();
        foreach(var range in effectRange)
        {
            attackableRange.Add(range);
        }
        ChangeState(MapSelectState.ACT_RESION_SELECT);
        isOnMyField = isMyField;
    }

    public void OnHeroSkillResponse(int heroIdx, SkillType skillType)
    {
        characters[heroIdx].GetComponent<CharacterScript>().SkillAction(skillType);
    }

    public void OnSkillEffect(MapIndex position, SkillType skillType)
    {
        Debug.Log("Res Skill Effect: " + "pos=" + position.posX + "," + position.posY + " skillIdx=" + (int)skillType);

        GameObject effect = Instantiate(BombEffect) as GameObject;
        effect.transform.parent = transform;
        Vector3 effectPos = GetTile(position).transform.localPosition;
        effect.transform.localPosition = new Vector3(effectPos.x, 2.0f, effectPos.z);
    }

    public void ClearTile()
    {
        foreach (var tile in tiles)
        {
            tile.GetComponent<TileScript>().ChangeTileState(TileScript.TileState.NORMAL);
        }
        attackableIndexes.Clear();
        attackableRange.Clear();
    }

    void ClearAllTile()
    {
        mapManager.ClearAllTile();
    }

    void CharacterMoveRequest(MapIndex index)
    {
        if (isPrePositioning)
        {
            Vector3 newPos = GetTile(index).transform.position;
            characters[selectedHeroIdx].GetComponent<CharacterScript>().Move(newPos, index.posX, index.posY);

            if (mapCharacterIndexes[selectedHeroIdx] == null)
                totalSettingNum++;

            mapCharacterIndexes[selectedHeroIdx] = new MapIndex(curMouseOveredIndex);
            selectedHeroIdx = -1;

            if (totalSettingNum >= numOfCharacter)
            {
                network.heros = mapCharacterIndexes;
            }
        }
        else
        {
            network.RequestMove(selectedHeroIdx, index);
        }
    }


    List<MapIndex> GetMovableIndexes()
    {
        List<MapIndex> resultList = new List<MapIndex>();
        if(selectedHeroIdx != -1)
        {
            if(mapCharacterIndexes[selectedHeroIdx] == null)
            {
                for (int xIdx = 0; xIdx < 3; ++xIdx) 
                    for (int yIdx = 0; yIdx < 3; ++yIdx)
                    {
                        resultList.Add(new MapIndex(xIdx, yIdx));
                    }
            }
            else
            {
                MapIndex stdIndex = mapCharacterIndexes[selectedHeroIdx];
                resultList.Add(new MapIndex(stdIndex.posX + 1, stdIndex.posY));
                resultList.Add(new MapIndex(stdIndex.posX - 1, stdIndex.posY));
                resultList.Add(new MapIndex(stdIndex.posX, stdIndex.posY + 1));
                resultList.Add(new MapIndex(stdIndex.posX, stdIndex.posY - 1));
                resultList.Add(stdIndex);
            }
        }
        return resultList;
    }

    List<MapIndex> GetAttackableRanges(MapIndex stdIndex)
    {
        List<MapIndex> resultList = new List<MapIndex>();
        //Debug.Log("[DEBUG] attackRange");
        foreach (var range in attackableRange)
        {
            MapIndex newPos = new MapIndex();
            newPos.posX = stdIndex.posX + range.relativeX;
            newPos.posY = stdIndex.posY + range.relativeY;
            resultList.Add(newPos);
            //Debug.Log("(" + newPos.posX + "," + newPos.posY + ")");
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