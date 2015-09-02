using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

    public float TargetSize;
    public Vector3 TargetAngle;
    public Vector3 TargetPos;
    public float MovingSec = 1.0f;
    Camera CameraObject;

	// Use this for initialization
	void Start () {
        CameraObject = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnCameraMove()
    {
        StopAllCoroutines();
        StartCoroutine(CameraMoveToTarget());
    }

    //무조건 1초만 돌린다
    IEnumerator CameraMoveToTarget()
    {
        yield return null;
        double accTime = 0.0;
        Vector3 deltaAngle = TargetAngle - transform.eulerAngles;
        Vector3 deltaPos = TargetPos - transform.position;
        float deltaSize = TargetSize - CameraObject.orthographicSize;
        while(accTime < MovingSec)
        {
            accTime += Time.deltaTime;
            transform.eulerAngles += deltaAngle * (Time.deltaTime / MovingSec);
            transform.position += deltaPos * (Time.deltaTime / MovingSec);
            CameraObject.orthographicSize += deltaSize * (Time.deltaTime / MovingSec);
            yield return new WaitForEndOfFrame();
        }
    }
}
