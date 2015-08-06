using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    public int x;
    public int y;
	// Use this for initialization
	void Start () {
	}

    public void SetIndex(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
