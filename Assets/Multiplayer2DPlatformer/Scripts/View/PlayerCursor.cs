using UnityEngine;
using System.Collections;

public class PlayerCursor : MonoBehaviour {

	[SerializeField]
	private float rotationSmooth;

	private Vector3 mouse { get{ return Input.mousePosition; } }
	private Vector3 pos   { get{ return Camera.main.ScreenToWorldPoint ( mouse ); } }

	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () 
	{
		transform.position 		= new Vector3 ( pos.x, pos.y, 1 );
		Quaternion newRot 		= transform.localRotation;
		newRot.eulerAngles 	    = new Vector3 ( 0, 0, Time.time * rotationSmooth );
		transform.localRotation = newRot;
	}
}
