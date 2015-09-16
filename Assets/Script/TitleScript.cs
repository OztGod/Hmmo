using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour 
{
	SocketScript network;
	bool IsLoginPending = false;
	bool register = false;
	bool registerCheck = false;
	bool loginCheck = false;
	string IdInput = "";
	string PswInput = "";

	// Use this for initialization
	void Start () 
	{
		network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
	}

	void OnGUI()
	{
		if (registerCheck)
		{
			float width = 300.0f;
			float height = 50.0f;
			float x = (Screen.width - width) / 2;
			float y = (Screen.height - height) / 2;

			Rect rect = new Rect(x, y, width, height);

			if (register)
			{
				if (GUI.Button(rect, "Register Success!"))
				{
					registerCheck = false;
				}
			}
			else
			{
				if (GUI.Button(rect, "Register Failed!"))
				{
					registerCheck = false;
				}
			}
		}
		else if (loginCheck)
		{
			float width = 300.0f;
			float height = 50.0f;
			float x = (Screen.width - width) / 2;
			float y = (Screen.height - height) / 2;

			Rect rect = new Rect(x, y, width, height);

			if (GUI.Button(rect, "Login Failed!"))
			{
				loginCheck = false;
			}
		}
		else if (false == IsLoginPending)
		{
			float width = 120.0f;
			float x = (Screen.width - width) / 2;
			float y = Screen.height / 2;

			Rect idRect = new Rect(x, y - 100, width, 30);
			Rect passwordRect = new Rect(x, y - 60, width, 30);
			Rect loginRect = new Rect(x + width / 2 + 10, y - 20, width / 2, 50);
			Rect registerRect = new Rect(x, y - 20, width / 2, 50);

			IdInput = GUI.TextField(idRect, IdInput);
			PswInput = GUI.PasswordField(passwordRect, PswInput, '*');
			if (GUI.Button(loginRect, "Login"))
			{
				IsLoginPending = true;
				network.Login(IdInput, PswInput);
			}

			if (GUI.Button(registerRect, "Register"))
			{
				IsLoginPending = true;
				network.Register(IdInput, PswInput);
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
			loginCheck = true;
			IsLoginPending = false;
			IdInput = "";
			PswInput = "";
		}
	}

	public void registerSuccess(bool isSuccess)
	{
		register = isSuccess;
		registerCheck = true;
		IsLoginPending = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
