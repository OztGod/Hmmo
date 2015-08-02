using UnityEngine;
using System.Collections;

public class SceneManagerScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void SceneChange()
    {
        Application.LoadLevel("GameScene");
    }

}
