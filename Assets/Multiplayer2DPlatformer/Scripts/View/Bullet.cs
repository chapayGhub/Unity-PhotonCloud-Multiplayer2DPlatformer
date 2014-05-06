using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public GameObject explosion;		// Prefab of explosion effect.

	private int  _power;
	private bool _isLocal;


	private void Start () 
	{
		Destroy ( gameObject, 3f );
	}

	public void SetData ( int power,  bool isLocal)
	{
		_power = power;
		_isLocal = isLocal;
	}
	
	private void Explode ()
	{
		Quaternion rndRot = Quaternion.Euler( 0f, 0f, Random.Range(0f, 360f) );
		Instantiate (explosion, transform.position, rndRot);
	}


	private void OnTriggerEnter2D ( Collider2D col ) 
	{
		if(col.tag == "Player")
		{
			if( _isLocal && PhotonNetwork.connected )
			{
				PlayerData enemy = RoomManager.Instance.players[ col.transform.root.GetComponent<PhotonView>().owner.ID ];
				if(enemy.id != PhotonNetwork.player.ID)
				{
					RoomManager.Instance.photonView.RPC ( "SetHealth", PhotonTargets.All, enemy.Health-_power, enemy.id );
				}
			}

			Explode();
			Destroy (gameObject);
		}
		else
		{
			Explode();
			Destroy (gameObject);
		}
	}
}
