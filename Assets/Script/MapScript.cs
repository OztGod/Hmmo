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
    int currentSettingIndex = 0;

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
	void Update () {
	
	}

    public void GetRandomCharacters(int[] characterTypes)
    {
        numOfCharacter = Mathf.Min(characterTypes.Length, characters.Length);

        for (int i = 0; i < numOfCharacter; ++i)
        {
            CharacterType type = (CharacterType)(i % (int)CharacterType.MAX_NUM);
            
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

            characters[i].transform.parent = gameObject.transform;
            characters[i].transform.position = new Vector3(-3, 0, i * 9/4);
        }
        currentSettingIndex = 0;
    }

    public void OnMouseOverTile(int x, int y)
    {
        if (currentSettingIndex >= numOfCharacter)
        {
            return;
        }

        characters[currentSettingIndex].transform.position = new Vector3(x * 3, 0, y * 3);

        if(Input.GetMouseButtonDown(0))
        {
            MapIndex newIndex = new MapIndex();
            newIndex.x = x;
            newIndex.y = y;
            mapCharacterIndexes[currentSettingIndex++] = newIndex;
            if(currentSettingIndex >= numOfCharacter)
            {
                GameObject network = GameObject.FindGameObjectWithTag("Network");
                network.GetComponent<SocketScript>().AllocHeros(mapCharacterIndexes);
            }
        }
    }
}
