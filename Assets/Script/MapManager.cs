using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {
    public GameObject turnButton;
    MapScript myMap = null;
    MapScript otherMap = null;

    bool isStart = false;
	// Use this for initialization
	void Start () {
        myMap = transform.GetChild(0).GetComponent<MapScript>();
        otherMap = transform.GetChild(1).GetComponent<MapScript>();
        turnButton.GetComponent<UIButton>().isEnabled = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
// 	    if(Input.GetMouseButtonDown(0))
//         {
//             if(false == isStart)
//             {
//                 isStart = true;
//                 int[] array = new int[4];
//                 for (int i = 0; i < array.Length; ++i)
//                 {
//                     array[i] = Random.Range(0, 4);
//                 }
//                 myMap.GetRandomCharacters(array);
//                 myMap.MakeFormation();
//             }
//         }
	}

    public void Ready()
    {
        myMap.FormationEnd();
        Camera.main.GetComponent<CameraScript>().OnCameraMove();
        turnButton.SetActive(true);
        turnButton.GetComponent<UIButton>().isEnabled = false;
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
            turnButton.GetComponent<UIButton>().isEnabled = true;
            myMap.MyTurnStart();
        }
    }

    public void TurnOver()
    {
        myMap.MyTurnEnd();
        turnButton.GetComponent<UIButton>().isEnabled = false;
    }

    public void SetHeroSkills(int heroIdx, List<SkillModel> skillModel)
    {
        myMap.SetHeroSkills(heroIdx, skillModel);
    }

    public void SetValidSkills(List<int> validSkills)
    {
        myMap.ConfirmHeroSkills(validSkills);
    }

    public void RejectPacket()
    {
        myMap.RejectPacket();
    }

    public void ResponseRange(int heroIdx, int skillIdx, List<MapIndex> mapRange, bool isMyField)
    {
        myMap.OnSkillRangeReponse(heroIdx, skillIdx, mapRange, isMyField);
    }

    public void ResponseEffect(int heroIdx, int skillIdx, List<EffectRange> effectRanges)
    {
        myMap.OnSkillEffectResponse(heroIdx, skillIdx, effectRanges);
    }
    
    public void ResponseSkill(SkillEffectModel model)
    {
        if(model.IsMyTurn)
        {
            myMap.OnHeroSkillResponse(model.SubjectHeroIdx, model.CastingSkill);
        }
        else
        {
            otherMap.OnHeroSkillResponse(model.SubjectHeroIdx, model.CastingSkill);
        }

        for(int i= 0; i < model.AffectedPosNum; ++i)
        {
            if(model.IsMyField[i])
            {
                myMap.OnSkillEffect(model.AffectedPositions[i], model.CastingSkill);
            }
            else
            {
                otherMap.OnSkillEffect(model.AffectedPositions[i], model.CastingSkill);
            }
        }
    }

    public void OnChracterDead(bool isMine, int deadHeroIdx)
    {
        Debug.Log("OnCharacterDead:" + isMine + "/" + deadHeroIdx);
        if (isMine)
        {
            myMap.OnChracterDie(deadHeroIdx);
        }
        else
        {
            otherMap.OnChracterDie(deadHeroIdx);
        }
    }

    public GameObject GetTile(bool isMine, MapIndex index)
    {
        if(isMine)
        {
            return myMap.GetTile(index);
        }
        else
        {
            return otherMap.GetTile(index);
        }
    }

    public void ClearAllTile()
    {
        myMap.ClearTile();
        otherMap.ClearTile();
    }
}

public class SkillEffectModel
{
    public bool IsMyTurn = true;
    public int SubjectHeroIdx = 0;
    public SkillType CastingSkill = new SkillType();
    public int AffectedPosNum = 0;
    public List<bool> IsMyField = new List<bool>();
    public List<MapIndex> AffectedPositions = new List<MapIndex>();
}