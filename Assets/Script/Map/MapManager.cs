using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

    public GameObject HighLight;
    public GameObject Menu;

    MapScript myMap;
    MapScript otherMap;
    SocketScript network;
    Menu menuPanel;

    bool isStart = false;

	void Start () 
    {
        network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
        myMap = transform.GetChild(0).GetComponent<MapScript>();
        otherMap = transform.GetChild(1).GetComponent<MapScript>();
        menuPanel = Menu.GetComponent<Menu>();

		network.MapManager = this;
	}

    public void Ready()
    {
        myMap.FormationEnd();
        Menu.SetActive(true);
        menuPanel = Menu.GetComponent<Menu>();
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

    public void UpdateHero(HeroStateModel model, bool isMine)
    {
        if (isMine)
        {
            myMap.UpdateHero(model);
        }
        else
        {
            otherMap.UpdateHero(model);
        }
        menuPanel.UpdateMenu();
    }

    public void UpdateStatus(StateModel model, bool isMine)
    {
        if (isMine)
        {
            myMap.UpdateStatus(model);
        }
        else
        {
            otherMap.UpdateStatus(model);
        }
    }

    public void TurnStart(bool isMine)
    {
        if (isMine)
        {
            myMap.MyTurnStart();
            menuPanel.Turn(true);
        }
    }

    public void TurnOver()
    {
        myMap.MyTurnEnd();
        menuPanel.Turn(false);
        network.RequestTurnEnd();
    }

    public void CharaterSelect(MapScript map, CharacterScript selectedHero, bool isPrepositioning)
    {
        HighLight.transform.position = new Vector3(selectedHero.transform.position.x, 13.5f, selectedHero.transform.position.z);
        HighLight.transform.parent = selectedHero.transform;
        Debug.Log("Select:" + map + " " +selectedHero +" "+ isPrepositioning);
        if (isPrepositioning)
        {
            map.ChangeState(MapSelectState.MOVE_SELECT);
        }
        else
        {
            network.CharacterSelect(selectedHero.Index);
            menuPanel.SetTarget(selectedHero);
        }
    }

    public void RequestMove(CharacterScript hero, MapIndex position, bool isPrepositioning)
    {
        if(isPrepositioning)
        {
            hero.Move(position.posX, position.posY, false);
            if(myMap.IsAllCharacterOnPosition())
            {
                network.HeroPositions.Clear();
                foreach(var charac in myMap.GetCharacters())
                {
                    network.HeroPositions.Add(charac.Pos);
                }
            }
        }
        else
        {
            network.RequestMove(hero.Index, position);
        }
    }

    public void RequestAction(CharacterScript hero, MapIndex targetPos)
    {
        network.RequestSkillAction(hero.Index, targetPos, hero.CurrentSkillIdx);
    }

    public void SetHeroSkills(int heroIdx, List<SkillModel> skillModel)
    {
        myMap.GetCharacter(heroIdx).SetSkill(skillModel);
    }

    public void SetValidSkills(List<int> validSkills)
    {
        myMap.GetSelectedCharacter().SetValidSkill(validSkills);
        myMap.ChangeState(MapSelectState.MOVE_SELECT);
        menuPanel.UpdateMenu();
    }

    public void ResetHighlight()
    {
        HighLight.transform.parent = null;
    }

    public void RejectPacket()
    {
        myMap.RejectPacket();
    }

    public void ResponseRange(int heroIdx, int skillIdx, List<MapIndex> mapRange, bool isMyField)
    {
        myMap.GetCharacter(heroIdx).SetSkillRange(skillIdx, mapRange, isMyField);
    }

    public void ResponseEffect(int heroIdx, int skillIdx, List<EffectRange> effectRanges)
    {
        bool isFinalRes = myMap.GetCharacter(heroIdx).SetSkillEffect(skillIdx, effectRanges);
        if (isFinalRes)
        {
            myMap.ChangeState(MapSelectState.ACT_RESION_SELECT);
        }
    }
    
    public void ResponseSkill(SkillEffectModel model)
    {
		SkillType type = myMap.GetCharacter(model.SubjectHeroIdx).GetSkillType(model.CastingSkill);
        MapScript map = model.IsMyTurn ? myMap : otherMap;
        map.GetCharacter(model.SubjectHeroIdx).SkillAction(type);
        

        for(int i= 0; i < model.AffectedPosNum; ++i)
        {
            MapScript affactedMap = model.IsMyField[i] ? myMap : otherMap;
            affactedMap.MakeSkillEffect(model.AffectedPositions[i], type);
        }
    }

    public void OnSkillClick(int skillIdx)
    {
        var selectedHero = myMap.GetSelectedCharacter();
        selectedHero.CurrentSkillIdx = skillIdx;
        network.RequestSkillRange(selectedHero.Index, skillIdx);
    }

    public void OnChracterDead(bool isMine, int deadHeroIdx)
    {
        Debug.Log("OnCharacterDead:" + isMine + "/" + deadHeroIdx);
        MapScript map = isMine ? myMap : otherMap;
        map.OnChracterDie(deadHeroIdx);
    }

    public GameObject GetTile(bool isMine, MapIndex index)
    {
        MapScript map = isMine ? myMap : otherMap;
        return map.GetTile(index);
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
    public int CastingSkill = 0;
    public int AffectedPosNum = 0;
    public List<bool> IsMyField = new List<bool>();
    public List<MapIndex> AffectedPositions = new List<MapIndex>();
}