using UnityEngine;
using System.Collections;

public class UserInfo : MonoBehaviour {
    public UILabel IdContainer;
    public UILabel WinRateContainer;
	// Use this for initialization
	void Start () {
        UserScript user = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
        IdContainer.text = user.UserId;
        WinRateContainer.text = user.WinCount + " / " + user.LoseCount;
	}
	
    public void OnMatchButtonClick()
    {
        GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>().MatchRequest();
    }
}
