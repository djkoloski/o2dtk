using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	//Speed var to be used later
	public float speed = 5.0f;
	public int screen_height = 640;
	public int square_height = 32;
	
	// Use this for initialization
	void Start () 
	{
		int screen_height = 640;
		int square_height = 32;
		
		if (! camera.orthographic)
			camera.orthographic = true;

		camera.orthographicSize = screen_height / (2 * square_height);
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
