using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PixelPerfectCamera : MonoBehaviour
{
	//Default Values
	public float speed = 5.0f;
	public int PixelsPerUnit = 32;
	public Vector3 CachePosition = new Vector3(0,0,0);
	public Vector3 RenderPosition = new Vector3(0,0,0);

	void OnPreRender() 
	{
		CachePosition = transform.position;
		//transform.position = RenderPosition = new Vector3(Mathf.Round(transform.position.x * PixelsPerUnit) / PixelsPerUnit, Mathf.Round(transform.position.y * PixelsPerUnit) / PixelsPerUnit, transform.position.z);
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
		GUILayout.Label("Pixels Per Unit:");
		GUILayout.FlexibleSpace();
		PixelsPerUnit = EditorGUILayout.IntField(PixelsPerUnit);
		GUILayout.EndHorizontal();
	}
	
	// Use this for initialization
	void Start () 
	{	
		if (! camera.orthographic)
			camera.orthographic = true;

		camera.orthographicSize = Screen.height / (2 * PixelsPerUnit);
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
	
