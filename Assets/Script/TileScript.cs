using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    public int x;
    public int y;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetIndex(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    void OnMouseOver(){
        GameObject map = transform.parent.gameObject;
        map.GetComponent<MapScript>().OnMouseOverTile(x, y);
    }
}
