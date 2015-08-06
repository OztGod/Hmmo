using UnityEngine;
using System.Collections;

public class MapScript : MonoBehaviour {

    public GameObject TilePrefab;
    public GameObject ArcherPrefab;
    public GameObject SwordPrefab;
    public GameObject MagicianPrefab;

    GameObject[] tiles = null;
    GameObject[] characters = null;
    MapIndex[] mapCharacterIndexes = null;

    int numOfCharacter = 0;
    public int currentSettingIndex = -1;
	int totalSettingNum = 0;

    enum CharacterType
    {
        ARCHER,
        SWORD_MAN,
        MAGICIAN,
        MAX_NUM,
    }

	// Use this for initialization
	void Start () {

        tiles = new GameObject[9];
        characters = new GameObject[4];
        mapCharacterIndexes = new MapIndex[4];

        for(int x = 0; x < 3; ++x)
        {
            for(int y = 0; y < 3; ++y)
            {
                GameObject tile = Instantiate(TilePrefab) as GameObject;
                tiles[x + y*3] = tile;
                tile.transform.parent = transform;
                tile.GetComponent<TileScript>().SetIndex(x, y);
                tile.transform.position = new Vector3(x * 3, 0, y * 3);
            }
        }
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameObject network = GameObject.FindGameObjectWithTag("Network");
		if (network.GetComponent<SocketScript>().IsReady)
		{
			//ready 박으면 땡
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit tileHit;

		RaycastHit hit;

		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 10))
			{
				int idx = hit.collider.gameObject.GetComponent<CharacterSelectScript>().idx;
				if (ChangeSettingIndex(idx))
				{
					return;
				}
			}
		}

		if (Physics.Raycast(ray, out tileHit, Mathf.Infinity, 1 << 11))
		{
			OnMouseOverTile(tileHit.collider.gameObject.GetComponent<TileScript>().x, tileHit.collider.gameObject.GetComponent<TileScript>().y);
		}
		
	}

	public bool ChangeSettingIndex(int idx)
	{
		if (currentSettingIndex == idx)
			return false;

		if (currentSettingIndex == -1)
		{
			currentSettingIndex = idx;
			characters[currentSettingIndex].GetComponent<Animation>().Play("att01");
			characters[currentSettingIndex].GetComponent<Animation>().PlayQueued("idle", QueueMode.CompleteOthers);
			return true;
		}

		if (mapCharacterIndexes[currentSettingIndex] == null)
		{
			characters[currentSettingIndex].transform.position = new Vector3(-3, 0, currentSettingIndex * 9 / 4);
		}
		else
		{
			characters[currentSettingIndex].transform.position = new Vector3(mapCharacterIndexes[currentSettingIndex].x * 3, 0,
																			 mapCharacterIndexes[currentSettingIndex].y * 3);
		}

		currentSettingIndex = idx;
		characters[currentSettingIndex].GetComponent<Animation>().Play("att01");
		characters[currentSettingIndex].GetComponent<Animation>().PlayQueued("idle", QueueMode.CompleteOthers);

		return true;
	}

    public void GetRandomCharacters(int[] characterTypes)
    {
        numOfCharacter = Mathf.Min(characterTypes.Length, characters.Length);
		totalSettingNum = 0;

        for (int i = 0; i < numOfCharacter; ++i)
        {
			mapCharacterIndexes[i] = null;
			if (characters[i] != null)
			{
				Object.Destroy(characters[i]);
			}

            CharacterType type = (CharacterType)(characterTypes[i] % (int)CharacterType.MAX_NUM);
            
            switch (type)
            {
                case CharacterType.ARCHER:
                    characters[i] = Instantiate(ArcherPrefab) as GameObject;
                    break;
                case CharacterType.SWORD_MAN:
                    characters[i] = Instantiate(SwordPrefab) as GameObject;
                    break;
                case CharacterType.MAGICIAN:
                    characters[i] = Instantiate(MagicianPrefab) as GameObject;
                    break;
                default:
                    break;
            };

			characters[i].GetComponent<CharacterSelectScript>().idx = i;

            characters[i].transform.parent = gameObject.transform;
            characters[i].transform.position = new Vector3(-3, 0, i * 9/4);
        }
        currentSettingIndex = -1;
    }

    public void OnMouseOverTile(int x, int y)
    {
        if (currentSettingIndex == -1)
        {
            return;
        }

        characters[currentSettingIndex].transform.position = new Vector3(x * 3, 0, y * 3);

        if(Input.GetMouseButtonDown(0))
        {
            MapIndex newIndex = new MapIndex();
            newIndex.x = x;
            newIndex.y = y;

			if (mapCharacterIndexes[currentSettingIndex] == null)
				totalSettingNum++;

            mapCharacterIndexes[currentSettingIndex] = newIndex;
			currentSettingIndex = -1;

            if(totalSettingNum >= numOfCharacter)
            {
                GameObject network = GameObject.FindGameObjectWithTag("Network");
                network.GetComponent<SocketScript>().heros = mapCharacterIndexes;
            }
        }
    }
}
