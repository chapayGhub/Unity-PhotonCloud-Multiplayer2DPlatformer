using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerView : Photon.MonoBehaviour {

	public float syncSmooth = 10f;

	private PlayerController controller;
	private PhotonView pv { get{ return photonView; } }
	private Vector3 syncPos;
	private Vector3 syncScale;
	private float syncAngle;
	private float syncInputHorizontal;


	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
		if( !PhotonNetwork.connected )
			Destroy( this );

		controller = GetComponent <PlayerController>();
	}

	private void Update()
	{
		// if this is not the local player, return
		if( pv.isMine ) return;
		transform.position 	 = Vector3.Lerp ( transform.position, syncPos, syncSmooth * Time.deltaTime );
		controller.Flippable.localScale = syncScale;
		foreach ( Transform t in controller.RotatableObjects )
		{
			Quaternion rot  = t.transform.localRotation;
			rot.eulerAngles = new Vector3 ( 0, 0, syncAngle );
			t.localRotation	= Quaternion.Lerp ( t.localRotation, rot, syncSmooth * Time.deltaTime );
		}
		controller.an.SetFloat("Speed", Mathf.Abs( syncInputHorizontal ));
	}
	

	/// <summary>
	/// Call RPC "_Jump"
	/// </summary>
	public void Jump()
	{
		photonView.RPC ( "_Jump", PhotonTargets.Others );
	}

	/// <summary>
	/// RPC of Jump
	/// </summary>
	[RPC]
	public void _Jump()
	{
		controller.an.SetTrigger("Jump");
	}

	
	/// <summary>
	/// Call RPC "_Shoot"
	/// </summary>
	public void Shoot( Vector2 direction, float angle )
	{
		photonView.RPC ( "_Shoot", PhotonTargets.Others, direction, angle );
	}

	/// <summary>
	/// RPC of Shoot
	/// </summary>
	[RPC]
	public void _Shoot( Vector2 direction, float angle )
	{
		// ... set the animator Shoot trigger parameter
		controller.an.SetTrigger ( "Shoot" );
		controller.audio.PlayOneShot ( controller.currentWeapon.shotSound );
		Rigidbody2D bulletInstance = Instantiate ( controller.currentWeapon.bullet, controller.currentWeapon.muzzle.position, Quaternion.Euler ( new Vector3(0, 0, angle) ) ) as Rigidbody2D;
		bulletInstance.velocity    = direction * controller.currentWeapon.speed;
		bulletInstance.transform.GetComponent<Bullet>().SetData ( 0, false );
	}


	/// <summary>
	/// Call RPC "ChangeWeapon"
	/// </summary>
	/// <param name="i">The index.</param>
	public void ChangeWeapon( int value )
	{
		photonView.RPC ( "_ChangeWeapon", PhotonTargets.Others, value );
	}

	/// <summary>
	/// RPC of Change Weapon
	/// </summary>
	/// <param name="i">The index.</param>
	[RPC]
	public void _ChangeWeapon( int value )
	{
		controller.KillAllWeapon();
		controller.Weapons[ value ].weaponRoot.gameObject.SetActive ( true );
		controller.currentWeapon = controller.Weapons[ value ];
	}


	/// <summary>
	/// Raises the photon serialize view event.
	/// </summary>
	/// <param name="stream">Stream.</param>
	/// <param name="info">Info.</param>
	private void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
	{
		if(!PhotonNetwork.connected) return;
		if (stream.isWriting)
		{
			stream.SendNext ( transform.position );
			stream.SendNext ( controller.Flippable.localScale );
			stream.SendNext ( controller.angle );
			stream.SendNext ( controller.inputHorizontal );
		}
		else
		{
			syncPos   			= ( Vector3 ) stream.ReceiveNext();
			syncScale 			= ( Vector3 ) stream.ReceiveNext();
			syncAngle 			= (  float  ) stream.ReceiveNext();
			syncInputHorizontal = (  float  ) stream.ReceiveNext();
		}
	}
}
