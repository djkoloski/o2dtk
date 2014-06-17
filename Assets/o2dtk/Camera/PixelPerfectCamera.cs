using UnityEngine;
using System.Collections;

public class PixelPerfectCamera : MonoBehaviour {

	// Pixels per Unit and Camera speed
	public float speed = 5.0f;
	public int PixelsPerUnit = 100;
	
	// Use this for initialization
	void Start () 
	{
		//int PixelsPerUnit = 32;
		
		if (! camera.orthographic)
			camera.orthographic = true;

		camera.orthographicSize = Screen.height / (2 * PixelsPerUnit);
		//Debug.Log(camera.orthographicSize);
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
	
