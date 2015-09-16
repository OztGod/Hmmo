using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public float TargetSize;
    public Vector3 TargetAngle;
    public Vector3 TargetPos;
    public int MovingSec = 5;
    Camera CameraObject;

	// Use this for initialization
	void Start () {
        CameraObject = GetComponent<Camera>();
	}
	
    public void OnCameraMove()
    {
        //StopAllCoroutines();

        Hashtable hash = new Hashtable();
        hash.Add("position", TargetPos);
        hash.Add("time", MovingSec);
        hash.Add("easetype", iTween.EaseType.easeInOutExpo);
        iTween.MoveTo(gameObject, hash);

        hash.Clear();
        hash.Add("rotation", TargetAngle);
        hash.Add("time", MovingSec);
        hash.Add("easetype", iTween.EaseType.easeInOutExpo);
        iTween.RotateTo(gameObject, hash);

        hash.Clear();
        hash.Add("from", CameraObject.orthographicSize);
        hash.Add("to", TargetSize);
        hash.Add("time", MovingSec);
        hash.Add("onupdate", "SizeUpdate");
        hash.Add("easetype", iTween.EaseType.easeInOutExpo);
        iTween.ValueTo(gameObject, hash);

    }

    public void SizeUpdate(float value)
    {
        CameraObject.orthographicSize = value;
    }
}
