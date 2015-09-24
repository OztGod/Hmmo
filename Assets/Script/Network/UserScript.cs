using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UserState
{
    START,
    LOGIN,
    LOBBY,
    PICK,
    GAME,
}

public class SimpleSkillModel
{
    public SkillType type;
    public int level;
}

public class OwnHeroModel
{
    public HeroClass heroType;
    public int level;
    public int maxHp;
    public int maxAct;
    public List<SimpleSkillModel> skills = new List<SimpleSkillModel>();    
}

public enum MatchResult
{
    NONE,
    WIN,
    LOSE,
    DRAW,
}

public class UserScript : MonoBehaviour 
{
    public UserState State = UserState.START;
    public string UserId = "";
    public int WinCount = 0;
    public int LoseCount = 0;
    public int HeroNum = 0;
    public int PickNum = 0;
    public bool IsMyPick = false;
    public MatchResult MyMatchResult = MatchResult.NONE;
    public List<OwnHeroModel> OwnHeroInfos = new List<OwnHeroModel>();
    public List<int> PickedHeroIdxes = new List<int>();

    public List<HeroClass> GetPickedHeros()
    {
        List<HeroClass> result = new List<HeroClass>();
        foreach(int index in PickedHeroIdxes)
        {
            result.Add(OwnHeroInfos[index].heroType);
        }
        return result;
    }

    public void EnterLobby()
    {
        Debug.Log("EnterLobby" + State);

        switch(State)
        {
            case UserState.LOGIN:
                var loginMenu = GameObject.FindGameObjectWithTag("UI").GetComponent<TitleUI>().LoginMenu;
                loginMenu.OnEnterLobby();
                break;
            case UserState.GAME:
                GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>().PopUpMenu.PopUp(PopUpType.LOBBY_READY);
                break;
        }
    }

}
