using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class HealthPickup : Photon.MonoBehaviour
{
	public int 		 healthBonus;
	public AudioClip collect;


	private RoomManager 
		_manager { get{ return RoomManager.Instance; } }

	private Animator anim;
	private bool 	 landed;


	private void Awake ()
	{
		anim = transform.root.GetComponent<Animator>();
	}


	private void OnTriggerEnter2D (Collider2D other)
	{
		if(other.tag == "Player" )
		{
			if ( other.transform.root.GetComponent<PlayerController>().photonView.isMine )
			{
				_manager.photonView.RPC ( "SetHealth", PhotonTargets.All, Mathf.Min ( _manager.myData.Health + healthBonus, 100 ), _manager.myData.id );
				AudioSource.PlayClipAtPoint(collect,transform.position);
			}
			Destroy(transform.root.gameObject);
		}
		else if( other.tag == "ground" && !landed )
		{
			anim.SetTrigger("Land");
			transform.parent = null;
			landed			 = true;	
			gameObject.AddComponent<Rigidbody2D>();
		}
	}
}
