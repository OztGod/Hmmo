using UnityEngine;
using System.Collections;

public class MapManager : MonoBehaviour {
    MapScript myMap;
    MapScript otherMap;

	// Use this for initialization
	void Start () {
        myMap = transform.GetChild(0).GetComponent<MapScript>();
        myMap.SetPositioning(true);
        otherMap = transform.GetChild(1).GetComponent<MapScript>();
        otherMap.SetPositioning(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Ready()
    {
        myMap.SetPositioning(false);
        Camera.main.GetComponent<CameraScript>().OnCameraMove();
    }

    public void GetRandomCharacters(int[] characterTypes)
    {
        myMap.GetRandomCharacters(characterTypes);
    }

    public void GetOtherCharacters(HeroData[] datas)
    {
        otherMap.GetFixedHerosAndPosition(datas);
    }
}
