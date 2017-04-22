using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour {

	bool recording;


	// Use this for initialization
	void Start () {

		recording = false;
		AudioSource aud = GetComponent<AudioSource>();
		aud.clip = null;
		aud.loop = false;

		foreach (string device in Microphone.devices) {
			Debug.Log("Name: " + device);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown()
	{
		Debug.Log ("I'm a cube.");

		if (!recording) 
		{
			AudioSource aud = GetComponent<AudioSource>();
			aud.clip = Microphone.Start("Built-in Microphone", false, 30, 44100);
			recording = true;
			Debug.Log("recording ...");
		} 
		else 
		{
			
			AudioSource aud = GetComponent<AudioSource>();
			int lastSampleIdx = Microphone.GetPosition(null);

			Microphone.End(null);

			float[] samples = new float[aud.clip.samples];
			aud.clip.GetData(samples, 0);
			float[] clipSamples = new float[lastSampleIdx];
			System.Array.Copy(samples, clipSamples, clipSamples.Length);

			aud.clip = AudioClip.Create("playRecordClip", clipSamples.Length, 1, 44100, false);
			aud.clip.SetData(clipSamples, 0);

			aud.Play();
			recording = false;
			Debug.Log("playing back ...");
		}

		// use Microphone.Start() to create a clip and set it on an AudioSource (?)
		// if toggling to not-recording
		// - for dev/debugging, use Play(), to hear what was just recorded
		// - use AudioSource.GetData()
		// - port JS to C# (to the extent needed) for sending in WAV format
		//   - need to ensure have raw data (not compressed)
		// use networking-API to send to server-side
		// on server-side:
		// - accept file-attachment
		// - store it as WAV-file
		// - use Audacity to check it
	}
}
