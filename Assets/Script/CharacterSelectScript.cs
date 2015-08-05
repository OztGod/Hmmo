using UnityEngine;
using System.Collections;

public class CharacterSelectScript : MonoBehaviour {

    bool IsSelected = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(IsSelected)
        {
            if(Input.GetMouseButtonUp(0))
            {
                Debug.Log("ButtonUp");
                IsSelected = false;
                return;
            }

        
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag != "Player")
                {
                    transform.position = hit.point;
                }
            }
        }
	}

    void OnMouseOver()
    {
        if(!IsSelected && Input.GetMouseButtonDown(0))
        {
            IsSelected = true;
            Debug.Log("Selected");
        }
    }
}
