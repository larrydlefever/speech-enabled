using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnitySpeechToText.Utilities;
using UniWebServer;


public class testScript : MonoBehaviour, IWebResource {

	bool recording;

	// Use this for initialization
	void Start () {

		EmbeddedWebServerComponent srvr = GetComponent<EmbeddedWebServerComponent>();
		srvr.AddResource("/authresponse", this);

		Debug.Log("app starting ...");
		Application.OpenURL("http://unity3d.com/");

		recording = false;
		AudioSource aud = GetComponent<AudioSource>();
		aud.clip = null;
		aud.loop = false;

		foreach (string device in Microphone.devices) {
			Debug.Log("Name: " + device);
		}
	}

	public void HandleRequest(Request request, Response response) 
	{
		Debug.Log("got authresponse: " + request.uri.PathAndQuery);
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

			//FIXME: for Alexa, this needs to be: 16000 sample-rate; 16-bit "linear PCM"
			// per: https://developer.amazon.com/public/solutions/alexa/alexa-voice-service/reference/speechrecognizer
			// maybe less difficult on server-side (in Java):
			// per: http://stackoverflow.com/questions/29905225/java-converting-mp3-to-wav-16-bit-mono
			/*
				import javax.sound.sampled.*;

				public void changeBitrate(File source,File output){
				  AudioFormat format=new AudioFormat(44100,16,1,true,true);
				  AudioInputStream in=AudioSystem.getAudioInputStream(source);
				  AudioInputStream convert=AudioSystem.getAudioInputStream(format,in);
				  AudioSystem.write(convert,AudioFileFormat.Type.WAVE,output);
				}
				
				NOTE: use "mdls" at cmdln, to check the attributes of a wav-file
				      Audacity can be misleading: you load a 16-bit audio-file, but it indicates as 32-bit
			*/
			AudioClip voiceInputClip = AudioClip.Create("playRecordClip", clipSamples.Length, 1, 44100, false);
			aud.clip = voiceInputClip;
			aud.clip.SetData(clipSamples, 0);

			aud.Play();
			recording = false;
			Debug.Log("playing back ...");

			//List<IMultipartFormSection> sections = new List<IMultipartFormSection>();
			//MultipartFormFileSection fileSection = new MultipartFormFileSection("audio");
			//fileSection.sectionName = "audio";

			// HOWEVER, see the following regarding saving files on Android (first intended targeted platform:
			//    http://stackoverflow.com/questions/10712520/android-saving-the-generated-qr-code-image-to-a-sd-card
			//    "Environment.getExternalStorageDirectory()"
			//    and
			//    "<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE"/>"
			// might need to use such code, or similar

			// public static string Save(string fileName, AudioClip clip, bool overwiteIfExists = false)
			string wavFilePath = SavWav.Save("voiceInput.wav", voiceInputClip, true);
			Debug.Log("stored voiceInputClip as: " + wavFilePath);

			byte[] voiceInputFileBytes = System.IO.File.ReadAllBytes(wavFilePath);
			Debug.Log("got voiceInputFileBytes: " + voiceInputFileBytes.Length);

			//var byteArray = new byte[clipSamples.Length * 4];
			//Buffer.BlockCopy(clipSamples, 0, byteArray, 0, byteArray.Length);

			//fileSection.sectionData = voiceInputFileBytes;
			//MultipartFormFileSection fileSection = new MultipartFormFileSection(voiceInputFileBytes);
			//sections.Add(fileSection);

			//Debug.Log("uploading ...");
			//UnityWebRequest.Post("http://localhost:4567/audio", sections);
			//Debug.Log("uploaded.");

			WWWForm form = new WWWForm();
			form.AddBinaryData("voiceInputUpload", voiceInputFileBytes, "voiceInput.wav", null);
			WWW w = new WWW("http://localhost:4567/audio", form);
			//yield return w;
			if (!string.IsNullOrEmpty(w.error)) {
				print(w.error);
			}
			else {
				print("Finished Uploading audio-file");
			}
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
