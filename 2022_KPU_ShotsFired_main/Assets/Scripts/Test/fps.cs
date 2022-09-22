using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fps : MonoBehaviour
{
	float deltaTime = 0.0f;
	//[SerializeField] private int m_targetFrame;
	int _width = Screen.width;
	int _height = Screen.height;

	void Start()
	{
		//QualitySettings.vSyncCount = 0;
		//Application.targetFrameRate = 300;
		//Time.captureFramerate = m_targetFrame;
		//Screen.SetResolution(3180, 3180 / 16 * 9, true);
		//Screen.SetResolution(720, 720 / 16 * 9, true);
	}

	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps {2:0.}X{3:0.})", msec, fps, Screen.width,Screen.height);
		//string text = string.Format("{0:0.0} ms ({1:0.} fps), {2}X{3}", msec, fps, _width, _height);
		GUI.Label(rect, text, style);
	}
}