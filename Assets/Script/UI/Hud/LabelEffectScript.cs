using UnityEngine;
using System.Collections;

public class LabelEffectScript : MonoBehaviour {

    UILabel label;
    public int standardSize = 0;
    public int deltaSize = 0;
    public int startY = 0;
    public int endY = 0;
    public int time = 0;
    
    void Awake()
    {
        label = GetComponent<UILabel>();
    }

    public void InitLabel(int value)
    {
        label.text = value.ToString();
        label.fontSize = standardSize + deltaSize * value;

        if(value < 0)
        {
            label.color = Color.red;
        }
        else
        {
            label.color = Color.green;
        }

        transform.localScale = new Vector3(1, 1, 1);
        transform.localPosition = new Vector3(0, startY, 0);

        Hashtable hash = new Hashtable();
        hash.Add("y", endY);
        hash.Add("time", time);
        hash.Add("islocal", true);
        hash.Add("oncomplete", "OnComplete");
        hash.Add("easetype", iTween.EaseType.easeOutExpo);
        iTween.MoveTo(gameObject, hash);
    }

    public void OnComplete()
    {
        GameObject.Destroy(gameObject);
    }

}
