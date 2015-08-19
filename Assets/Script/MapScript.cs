using UnityEngine;
using System.Collections;

public class MapScript : MonoBehaviour {

    public GameObject TilePrefab;
    public GameObject ArcherPrefab;
    public GameObject SwordPrefab;
    public GameObject MagicianPrefab;
    public GameObject SpotLight;
    public int currentSettingIndex = -1;

    GameObject[] tiles = new GameObject[9];
    GameObject[] characters = new GameObject[4];
    MapIndex[] mapCharacterIndexes = new MapIndex[4];
    Vector2 lastMouseClickedPosition = new Vector2();
    MapIndex curMouseOveredIndex = new MapIndex();


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
                int characterIndex = character.GetComponent<CharacterScript>().Index;
                SpotLight.transform.position = new Vector3(character.transform.position.x, 13.5f, character.transform.position.z);
                SpotLight.transform.parent = character.transform;

                if (ChangeSettingIndex(characterIndex))
                {
                    return;
                }
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
                CharacterMove(curMouseOveredIndex);
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
            curTile.GetComponent<TileScript>().ChangeTileState(TileScript.TileState.MOVE);

            if(Input.GetMouseButton(0))
            {
                lastMouseClickedPosition = Input.mousePosition;
                CharacterAttack(curMouseOveredIndex);
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
            if(currentSettingIndex != -1)
            {
                msg = characters[currentSettingIndex].GetComponent<CharacterScript>().GetInfoString();
            }

            float width = 150.0f;
            float height = 100.0f;
            float x = lastMouseClickedPosition.x;
            float y = lastMouseClickedPosition.y;
            GUI.Box(new Rect(x, y, width, height), msg);

            float buttonWidth = 100;
            float buttonHeight = 50;
            float buttonX = x + 25;
            float buttonY = y + 50;

            GUILayout.BeginArea(new Rect(buttonX, buttonY, buttonWidth, buttonHeight));
            if(currentSettingIndex != -1 && GUILayout.Button("Move"))
            {
                selectState = MapSelectState.MOVE_SELECT;
            }
            GUILayout.EndArea();
        }
    }

    public void MakeFormation()
    {
        selectState = MapSelectState.CHARACTER_SELECT;
    }

    public void FormationEnd()
    {
        selectState = MapSelectState.NO_SELECT;
    }

	public bool ChangeSettingIndex(int idx)
	{
		if (currentSettingIndex == idx)
			return false;

		if (currentSettingIndex == -1)
		{
			currentSettingIndex = idx;
            characters[currentSettingIndex].GetComponent<CharacterScript>().SelectHero();
			return true;
		}

		if (mapCharacterIndexes[currentSettingIndex] == null)
		{
            characters[currentSettingIndex].transform.localPosition = new Vector3(-3, 0, currentSettingIndex * 9 / 4);
		}
		else
		{
            characters[currentSettingIndex].transform.localPosition = new Vector3(mapCharacterIndexes[currentSettingIndex].posX * 3, 0,
																			 mapCharacterIndexes[currentSettingIndex].posY * 3);
		}

		currentSettingIndex = idx;
        characters[currentSettingIndex].GetComponent<CharacterScript>().SelectHero();
		return true;
	}

    public void GetRandomCharacters(int[] characterTypes)
    {
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
        currentSettingIndex = -1;
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
            characters[index].GetComponent<CharacterScript>().Initialize(heroData);
        }
    }

    public void OnMouseOverTile(int x, int y)
    {
        if (currentSettingIndex == -1)
        {
            return;
        }


    }

    public void CharacterActionEnd()
    {
        selectState = MapSelectState.CHARACTER_SELECT;
    }

    GameObject GetTile(MapIndex index)
    {
        if (index.IsValid())
            return tiles[index.posX + index.posY * 3];
        else
            return null;
    }

    void CharacterMove(MapIndex index)
    {
        Vector3 newPos = GetTile(index).transform.position;
        characters[currentSettingIndex].GetComponent<CharacterScript>().Move(newPos, index.posX, index.posY);

        if (mapCharacterIndexes[currentSettingIndex] == null)
            totalSettingNum++;

        mapCharacterIndexes[currentSettingIndex] = new MapIndex(curMouseOveredIndex);
        currentSettingIndex = -1;

        if (totalSettingNum >= numOfCharacter)
        {
            GameObject network = GameObject.FindGameObjectWithTag("Network");
            network.GetComponent<SocketScript>().heros = mapCharacterIndexes;
        }
    }

    void CharacterAttack(MapIndex index)
    {

    }


    ArrayList GetMovableIndexes()
    {
        ArrayList resultList = new ArrayList();
        if(currentSettingIndex != -1)
        {
            if(mapCharacterIndexes[currentSettingIndex] == null)
            {
                for (int xIdx = 0; xIdx < 3; ++xIdx) 
                    for (int yIdx = 0; yIdx < 3; ++yIdx)
                    {
                        resultList.Add(new MapIndex(xIdx, yIdx));
                    }
            }
            else
            {
                MapIndex stdIndex = mapCharacterIndexes[currentSettingIndex];
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
