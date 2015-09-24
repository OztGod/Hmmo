using UnityEngine;
using System.Collections;

public class LoginScript : MonoBehaviour
{
    public PopUpScript popUpMenu;
    public UIInput IdInput;
    public UIInput PswInput;
    public UIButton LoginButton;
    public UIButton RegisterButton;

    SocketScript network;
    public void Start()
    {
        network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
    }

    public void OnLoginClicked()
    {
        if (IdInput.value.Length == 0 || PswInput.value.Length == 0)
            return;

        network.Login(IdInput.value, PswInput.value);
    }

    public void OnRegisterClicked()
    {
        if (IdInput.value.Length == 0 || PswInput.value.Length == 0)
            return;

        network.Register(IdInput.value, PswInput.value);
    }

    public void OnLogin(bool isSuccess)
    {
        PopUpType type = isSuccess ? PopUpType.LOBBY_LOADING : PopUpType.LOGIN_FAILED;
        popUpMenu.PopUp(type);
    }

    public void OnRegister(bool isSuccess)
    {
        PopUpType type = isSuccess ? PopUpType.REGISTER_SUCCESS : PopUpType.REGISTER_FAILED;
        popUpMenu.PopUp(type);
    }

    public void OnEnterLobby()
    {
        popUpMenu.PopUp(PopUpType.LOBBY_READY);
    }
}
