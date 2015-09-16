using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour 
{
	SocketScript network;
	bool IsLoginPending = false;
	bool check = false;
	string IdInput = "";
	string PswInput = "";

	// Use this for initialization
	void Start () 
	{
		network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
	}

	void OnGUI()
	{
		if (check)
		{
			if (GUILayout.Button("Login Failed!"))
			{
				check = false;
			}
		}
		else if (false == IsLoginPending)
		{
			IdInput = GUILayout.TextField(IdInput);
			PswInput = GUILayout.TextField(PswInput);
			if (GUILayout.Button("Login", GUILayout.Height(30)))
			{
				IsLoginPending = true;
				network.Login(IdInput, PswInput);
			}
		}
	}

	public void loginSuccess(bool isSuccess)
	{
		if (isSuccess)
		{
			Application.LoadLevel("GameScene");
		}
		else
		{
			check = true;
			IsLoginPending = false;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
