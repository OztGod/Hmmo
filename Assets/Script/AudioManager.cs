using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour 
{
	static public AudioManager _instance = null;
	public GameObject sfx;
	public static AudioManager instance
	{
		get { return _instance; }
	}

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start () 
	{
		if (_instance == null)
			_instance = this;
	}

	public void PlaySfx(AudioClip clip)
	{
		sfx.GetComponent<AudioSource>().PlayOneShot(clip);
	}

	public void PlayBGM(AudioClip clip)
	{
		GetComponent<AudioSource>().loop = true;
		GetComponent<AudioSource>().clip = clip;
		GetComponent<AudioSource>().Play();
	}
}
