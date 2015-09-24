using UnityEngine;
using System.Collections;

public enum SceneType
{
    TITLE,
    LOBBY,
    PICK,
    GAME
}

public class SceneManagerScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        DontDestroyOnLoad(gameObject);
	}
	
    public void SceneChange(SceneType type)
    {
        UserScript user = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
        string sceneName = "";
        switch (type)
        {
            case SceneType.GAME:
                sceneName = "GameScene";
                user.State = UserState.GAME;
                break;
            case SceneType.TITLE:
                sceneName = "TitleScene";
                user.State = UserState.START;
                break;
            case SceneType.LOBBY:
                sceneName = "LobbyScene";
                user.State = UserState.LOBBY;
                break;
            case SceneType.PICK:
                sceneName = "PickScene";
                user.State = UserState.PICK;
                break;
            default:
                break;
        }
        Application.LoadLevel(sceneName);
    }

}
