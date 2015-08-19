using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {

    public int x;
    public int y;

    public Material AttackMaterial;
    public Material AttackableMaterial;
    public Material MovableMaterial;
    public Material MoveMaterial;
    public Material NormalMaterial;


    public enum TileState
    {
        NORMAL,
        MOVABLE,
        MOVE,
        ATTACKABLE,
        ATTACK,
    }

    GameObject cover;
    TileState state = TileState.NORMAL;

	// Use this for initialization
	void Start () {
        cover = transform.GetChild(0).gameObject;
        cover.GetComponent<Renderer>().material = NormalMaterial;
        ChangeTileState(TileState.NORMAL);
	}

    public void SetIndex(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public MapIndex GetIndex()
    {
        MapIndex result = new MapIndex(x, y);
        return result;
    }

    public void ChangeTileState(TileState newState)
    {
        if (state == newState)
            return;
        Debug.Log("Tile StateChange" + newState);

        switch(newState)
        {
            case TileState.NORMAL:
                cover.GetComponent<Renderer>().material = NormalMaterial;
                state = newState;
                break;
            case TileState.MOVABLE:
                if (state == TileState.NORMAL || state == TileState.MOVE)
                {
                    cover.GetComponent<Renderer>().material = MovableMaterial;
                    state = newState;
                }
                break;
            case TileState.MOVE:
                if (state == TileState.MOVABLE)
                {
                    cover.GetComponent<Renderer>().material = MoveMaterial;
                    state = newState;
                }
                break;

            case TileState.ATTACKABLE:
                if (state == TileState.NORMAL || state == TileState.ATTACK)
                {
                    cover.GetComponent<Renderer>().material = AttackableMaterial;
                    state = newState;
                }
                break;

            case TileState.ATTACK:
                if (state == TileState.ATTACKABLE)
                {
                    cover.GetComponent<Renderer>().material = AttackMaterial;
                    state = newState;
                }
                break;
            default:
                break;
        }
    }

    IEnumerator WaitForSecAndGoNormal()
    {
        yield return null;
        double accTime = 0.0;
        while (accTime < 1)
        {
            accTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        ChangeTileState(TileState.NORMAL);
    }
}
