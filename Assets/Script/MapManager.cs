using UnityEngine;
using System.Collections;

public class MapManager : MonoBehaviour {
    MapScript myMap;
    MapScript otherMap;

	// Use this for initialization
	void Start () {
        myMap = transform.GetChild(0).GetComponent<MapScript>();
        /*
        int[] array = new int[4];
        for(int i = 0 ;i < array.Length; ++i)
        {
            array[i] = Random.Range(0, 4);
        }
        myMap.GetRandomCharacters(array);
        */
        otherMap = transform.GetChild(1).GetComponent<MapScript>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Ready()
    {
        myMap.FormationEnd();
        Camera.main.GetComponent<CameraScript>().OnCameraMove();
    }

    public void GetRandomCharacters(int[] characterTypes)
    {
        myMap.GetRandomCharacters(characterTypes);
        myMap.MakeFormation();
    }

    public void GetCharacters(HeroModel[] models, bool isMine)
    {
        if (isMine)
        {
            myMap.GetFixedHerosAndPosition(models);
        }
        else
        {
            otherMap.GetFixedHerosAndPosition(models);
        }
    }

    public void SynchronizeState(HeroStateModel model, bool isMine)
    {
        if (isMine)
        {
            myMap.SynchronizeState(model);
        }
        else
        {
            otherMap.SynchronizeState(model);
        }
    }

    public void TurnStart(bool isMine)
    {
        if (isMine)
        {
            myMap.MyTurnStart();
        }
    }

    public void RejectPacket()
    {
        myMap.RejectPacket();
    }
}
