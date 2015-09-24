using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PopUpType
{
    LOGIN_FAILED,
    REGISTER_SUCCESS,
    REGISTER_FAILED,
    LOBBY_LOADING,
    MATCH_LOADING,
    LOBBY_READY,
    MATCH_READY,
    MATCH_END,
}


public class PopUpScript : MonoBehaviour {
    public LoginScript loginMenu;
    public UILabel label;
    public UIButton Button;
    UILabel ButtonLabel;
    PopUpType type;
    public void Start()
    {
        Close();
        ButtonLabel = Button.transform.FindChild("Label").GetComponent<UILabel>();
    }

    public void PopUp(PopUpType popUpType)
    {
        type = popUpType;
        GetComponent<UIWidget>().alpha = 1;
        label.text = type.ToString();
        ButtonLabel.text = "OK";

        switch (type)
        {
            case PopUpType.LOBBY_READY:
                Button.isEnabled = true;
                break;

            case PopUpType.MATCH_READY:
                
                break;

            case PopUpType.MATCH_LOADING:
                ButtonLabel.text = "Cancel";
                break;

            case PopUpType.LOBBY_LOADING:
                Button.isEnabled = false;
                break;

            case PopUpType.MATCH_END:
                UserScript user = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
                label.text = user.MyMatchResult.ToString();
                user.MyMatchResult = MatchResult.NONE;
                Button.isEnabled = false;
                ButtonLabel.text = "Wait";
                break;
        }
    }

    public void OnButtonClick()
    {
        switch (type)
        {
            case PopUpType.LOBBY_LOADING:
                GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>().MatchCancel();
                Close();
                break;

            case PopUpType.LOGIN_FAILED:
                Close();
                break;

            case PopUpType.REGISTER_SUCCESS:
                Close();
                break;

            case PopUpType.REGISTER_FAILED:
                Close();
                break;

            case PopUpType.LOBBY_READY:
                var sceneManager = GameObject.FindGameObjectWithTag("Scene").GetComponent<SceneManagerScript>();
                sceneManager.SceneChange(SceneType.LOBBY);
                break;

            case PopUpType.MATCH_END:
                break;
        }

    }

    void Close()
    {
        GetComponent<UIWidget>().alpha = 0;
    }
}
