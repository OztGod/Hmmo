using UnityEngine;
using System.Collections;

public class MapScript : MonoBehaviour {

    public GameObject TilePrefab;
    public GameObject ArcherPrefab;
    public GameObject SwordPrefab;
    public GameObject MagicianPrefab;
    public GameObject SpotLight;
    public int selectedHeroIdx = -1;

    GameObject[] tiles = new GameObject[9];
    GameObject[] characters = new GameObject[4];
    MapIndex[] mapCharacterIndexes = new MapIndex[4];
    Vector2 lastMouseClickedPosition = new Vector2();
    MapIndex curMouseOveredIndex = new MapIndex();
    bool isPrePositioning = false;
    bool isMine = false;

    public enum CharacterType
    {
        ARCHER,
        SWORD_MAN,
        MAGICIAN,
        MAX_NUM,
    }

    enum MapSelectState
    {
        NO_SELECT,
        CHARACTER_SELECT,
        MOVE_SELECT,
        ATTACK_SELECT,
    }

    int numOfCharacter = 0;
	int totalSettingNum = 0;
    MapSelectState selectState = MapSelectState.NO_SELECT;

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

        lastMouseClickedPosition.x = Screen.width / 2;
        lastMouseClickedPosition.y = Screen.height / 5;

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
            case MapSelectState.ATTACK_SELECT:
                OnAttackSelect();
                break;
        }
	}

    void OnCharacterSelect()
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<TileScript>().ChangeTileState(TileScript.TileState.NORMAL);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 10))
            {
                lastMouseClickedPosition = Input.mousePosition;
                var character = hit.collider.gameObject;
                selectedHeroIdx = character.GetComponent<CharacterScript>().SelectHero();
                SpotLight.transform.position = new Vector3(character.transform.position.x, 13.5f, character.transform.position.z);
                SpotLight.transform.parent = character.transform;
            }
        }
        return;
    }

    void OnMoveSelect()
    {
        foreach(MapIndex index in GetMovableIndexes())
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
            curMouseOveredIndex.posX = tileHit.collider.gameObject.GetComponent<TileScript>().x;
            curMouseOveredIndex.posY = tileHit.collider.gameObject.GetComponent<TileScript>().y;

            GetTile(curMouseOveredIndex).GetComponent<TileScript>().ChangeTileState(TileScript.TileState.MOVE);

            if (Input.GetMouseButtonDown(0))
            {
                lastMouseClickedPosition = Input.mousePosition;
                CharacterMoveRequest(curMouseOveredIndex);
                selectState = MapSelectState.NO_SELECT;
            }

        }
        return;
    }

    void OnAttackSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit tileHit;

        if (Physics.Raycast(ray, out tileHit, Mathf.Infinity, 1 << 11))
        {
            curMouseOveredIndex.posX = tileHit.collider.gameObject.GetComponent<TileScript>().x;
            curMouseOveredIndex.posY = tileHit.collider.gameObject.GetComponent<TileScript>().y;

            GameObject curTile = GetTile(curMouseOveredIndex);
            curTile.GetComponent<TileScript>().ChangeTileState(TileScript.TileState.ATTACK);

            if(Input.GetMouseButton(0))
            {
                lastMouseClickedPosition = Input.mousePosition;
                CharacterActionRequest(curMouseOveredIndex);
                selectState = MapSelectState.NO_SELECT;
            }
        }
        return;
    }

    void OnGUI()
    {
        if(selectState == MapSelectState.CHARACTER_SELECT)
        {
            string msg = "==No Select==";
            if(selectedHeroIdx != -1)
            {
                msg = characters[selectedHeroIdx].GetComponent<CharacterScript>().GetInfoString();
            }

            float width = 150.0f;
            float height = 100.0f;
            float x = 20 + width/2;
            float y = 20 + height/2;
            GUI.Box(new Rect(x, y, width, height), msg);

            float buttonWidth = 100;
            float buttonHeight = 50;
            float buttonX = x + 25;
            float buttonY = y + 50;

            GUILayout.BeginArea(new Rect(buttonX, buttonY, buttonWidth, buttonHeight));
            if(selectedHeroIdx != -1 && GUILayout.Button("MOVE"))
            {
                selectState = MapSelectState.MOVE_SELECT;
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(buttonX, buttonY + 55, buttonWidth, buttonHeight));
            if (GUILayout.Button("END TURN"))
            {
                MyTurnEnd();
            }
            GUILayout.EndArea();

        }
    }

    public void SynchronizeState(HeroStateModel model)
    {
        GameObject hero = characters[model.index];
        Vector3 newPos = GetTile(model.position).transform.position;
        hero.GetComponent<CharacterScript>().Move(newPos, model.position.posX, model.position.posY);
        hero.GetComponent<CharacterScript>().UpdateState(model);
    }

    public void MakeFormation()
    {
        selectedHeroIdx = -1;
        isPrePositioning = true;
        selectState = MapSelectState.CHARACTER_SELECT;
    }

    public void FormationEnd()
    {
        isPrePositioning = false;
        selectState = MapSelectState.NO_SELECT;
    }

    public void MyTurnStart()
    {
        selectedHeroIdx = -1;
        selectState = MapSelectState.CHARACTER_SELECT;
    }

    public void MyTurnEnd()
    {
        selectState = MapSelectState.NO_SELECT;
        GameObject network = GameObject.FindGameObjectWithTag("Network");
        network.GetComponent<SocketScript>().RequestTurnEnd();
    }

// 	public bool ChangeSettingIndex(int idx)
// 	{
// 		if (selectedHeroIdx == idx)
// 			return false;
// 
// 		if (selectedHeroIdx == -1)
// 		{
// 			selectedHeroIdx = idx;
//             characters[selectedHeroIdx].GetComponent<CharacterScript>().SelectHero();
// 			return true;
// 		}
// 
// 		if (mapCharacterIndexes[selectedHeroIdx] == null)
// 		{
//             characters[selectedHeroIdx].transform.localPosition = new Vector3(-3, 0, selectedHeroIdx * 9 / 4);
// 		}
// 		else
// 		{
//             characters[selectedHeroIdx].transform.localPosition = new Vector3(mapCharacterIndexes[selectedHeroIdx].posX * 3, 0,
// 																			 mapCharacterIndexes[selectedHeroIdx].posY * 3);
// 		}
// 
// 		selectedHeroIdx = idx;
//         characters[selectedHeroIdx].GetComponent<CharacterScript>().SelectHero();
// 		return true;
// 	}

    public void GetRandomCharacters(int[] characterTypes)
    {
        isMine = true;

        for (int index = 0; index < 4; ++index)
        {
            Debug.Log(characterTypes[index]);
        }

        numOfCharacter = characterTypes.Length;
		totalSettingNum = 0;

        for (int index = 0; index < numOfCharacter; ++index)
        {
            Debug.Log("index:" + index);
			mapCharacterIndexes[index] = null;
            if (characters[index] != null)
			{
                Object.Destroy(characters[index]);
			}

            CharacterType type = (CharacterType)(characterTypes[index] % (int)CharacterType.MAX_NUM);
            Debug.Log("type:" + type);
            
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
            characters[index].GetComponent<CharacterScript>().Initialize(heroData);
        }
    }

    public void CharacterActionEnd()
    {
        if (isMine)
        {
            selectState = MapSelectState.CHARACTER_SELECT;
        }
    }

    GameObject GetTile(MapIndex index)
    {
        if (index.IsValid())
            return tiles[index.posX + index.posY * 3];
        else
            return null;
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
                GameObject network = GameObject.FindGameObjectWithTag("Network");
                network.GetComponent<SocketScript>().heros = mapCharacterIndexes;
            }
        }
        else
        {
            GameObject network = GameObject.FindGameObjectWithTag("Network");
            network.GetComponent<SocketScript>().RequestMove(selectedHeroIdx, index);
        }
    }

    void CharacterActionRequest(MapIndex index)
    {
        //TODO
    }


    ArrayList GetMovableIndexes()
    {
        ArrayList resultList = new ArrayList();
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
}
