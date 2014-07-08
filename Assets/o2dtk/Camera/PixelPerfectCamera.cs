using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PixelPerfectCamera : MonoBehaviour
{
	//Default Values
	public float speed = 5.0f;
	float PixelsPerUnit;
	public float textureSize = 32f;
	public Vector3 CachePosition = new Vector3(0,0,0);
	public Vector3 RenderPosition = new Vector3(0,0,0);

	void OnPreRender() 
	{
		CachePosition = transform.position;
		RenderPosition = new Vector3(Mathf.Round(CachePosition.x * textureSize) / textureSize, Mathf.Round(CachePosition.y * textureSize) / textureSize, CachePosition.z);
		transform.position = RenderPosition;
	}

	void OnPostRender()
	{
		transform.position = CachePosition;
	}
	
	public void OnInspectorGUI()
	{
		// Pixels per Unit and Camera speed
		GUILayout.BeginHorizontal();
		GUILayout.Label("Camera Speed:");
		GUILayout.FlexibleSpace();
		speed = EditorGUILayout.FloatField(speed);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Texture Size:");
		GUILayout.FlexibleSpace();
		PixelsPerUnit = EditorGUILayout.FloatField(textureSize);
		GUILayout.EndHorizontal();
	}
	
	// Use this for initialization
	void Start () 
	{	
		if (! camera.orthographic)
			camera.orthographic = true;

		PixelsPerUnit = 1f / textureSize;
		camera.orthographicSize = (Screen.height / 2f) * PixelsPerUnit;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if(Input.GetKey(KeyCode.RightArrow))
		{
			transform.position += new Vector3(speed * Time.deltaTime,0,0);
		}
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			transform.position -= new Vector3(speed * Time.deltaTime,0,0);
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			transform.position -= new Vector3(0,speed * Time.deltaTime,0);
		}
		if(Input.GetKey(KeyCode.UpArrow))
		{
			transform.position += new Vector3(0,speed * Time.deltaTime,0);
		}
	}
}

// TODO	Allign on Pixels
	
