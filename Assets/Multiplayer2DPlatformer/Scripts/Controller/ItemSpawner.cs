using UnityEngine;
using System.Collections;

public class ItemSpawner : Photon.MonoBehaviour {

	[MinMaxSlider (10f, 180f)]
	public Vector2 DropInterval;

	public Transform SpawnPoint;
	public GameObject[] Items;

	// Use this for initialization
	private void Start ()
	{
		if ( !PhotonNetwork.isMasterClient ) return;
		Invoke ( "Drop", Random.Range( DropInterval.x, DropInterval.y ) );
	}
	
	private void Drop ()
	{
		photonView.RPC ("_Drop", PhotonTargets.All, Random.Range ( 0, Items.Length ) );
		Invoke ( "Drop", Random.Range( DropInterval.x, DropInterval.y ) );
	}

	[RPC]
	public void _Drop ( int index )
	{
		Instantiate ( Items[ index ], SpawnPoint.position, Quaternion.identity );
	}

	private void OnMasterClientSwitched ( PhotonPlayer newMasterClient )
	{
		if ( !PhotonNetwork.isMasterClient ) return;
		Invoke ( "Drop", Random.Range( DropInterval.x, DropInterval.y ) );
	}
}
