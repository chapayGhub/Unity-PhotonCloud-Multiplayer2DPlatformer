using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : Photon.MonoBehaviour {

	// Fire type
	public enum WeaponType { Semi, Auto }

	// A internal class of weapon data
	[System.Serializable]
	public class Weapon
	{
		// Key assignment of shot
		public KeyCode keyAssign;

		public Rigidbody2D bullet;

		public WeaponType weaponType;

		// Weapons's power
		[Range(0, 100     )] 
		public int power = 20;

		// Weapon 's root transform
		public Transform weaponRoot;

		// Bullets are instantiated at this
		public Transform muzzle;

		// bullet's speed
		[Range(10   , 150 )] 
		public float speed = 50f;

		// stand by the next shot
		[Range(0.07f, 2f  )] 
		public float fireRate = 0.15f;

		public AudioClip shotSound;
	}

	#region Show Inspector¥

	// This object is flippable with mouse
	public Transform   Flippable;

	// Rotatable game objects with mouse movement
	public Transform[] RotatableObjects;

	// Player's movement force
	[Range(10, 200 )] 
	public float MoveForce = 50f;

	// Player's speed limitation
	[Range(3  , 20  )] 
	public float MaxSpeed  = 10f;

	// Player's jump force
	[Range(500, 1800)] 
	public float JumpForce = 1000f;

	public LayerMask  GroundLayer;
	public GameObject DieExplosion;
	public Weapon[] Weapons;

	#endregion


	#region Hide Inspector

	[HideInInspector] public Vector3  mouseOnScreen      = Vector3.zero;
	[HideInInspector] public Vector3  characterOnScreen  = Vector3.zero;
	[HideInInspector] public bool 	  _Right	 		 = true;
	[HideInInspector] public float 	  angle				 = .0f;
	[HideInInspector] public float 	  inputHorizontal    = .0f;
	[HideInInspector] public float 	  lenX    			 = .0f;
	[HideInInspector] public float 	  lenY				 = .0f;
	[HideInInspector] public bool 	  IsGrounded 		 = false;
	[HideInInspector] public Animator an;
	[HideInInspector] public Weapon   currentWeapon;
	private float t_weapon;
	private bool isQuitting;

	#endregion
	

	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake () 
	{
		an = GetComponent < Animator >();
		// Init weapons
		ChangeWeapon(0);
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update () 
	{
		// If this instance is Local, update the control
		if( (PhotonNetwork.connected && photonView.isMine) || !PhotonNetwork.connected )
		{
			UpdateControl();
		}
		else
		{
			Destroy ( rigidbody2D );
		}
	}


	/// <summary>
	/// Updates the control.
	/// </summary>
	private void UpdateControl()
	{
		// Update the variables
		Vector2 from	  = new Vector2 ( transform.position.x, transform.position.y );
		Vector2 to		  = new Vector2 ( from.x, from.y - 1.5f );
		IsGrounded 	      = Physics2D.Linecast( from, to, GroundLayer );
		mouseOnScreen 	  = Input.mousePosition;
		characterOnScreen = Camera.main.WorldToScreenPoint ( this.transform.position );
		inputHorizontal   = Input.GetAxis ( "Horizontal" );
		lenX			  = Mathf.Abs ( mouseOnScreen.x - characterOnScreen.x );
		lenY			  = mouseOnScreen.y - characterOnScreen.y;

		// Jump if player uses jump key
		if( Input.GetButtonDown("Jump") && IsGrounded )
		{
			Jump();
		}

		if( mouseOnScreen.x < characterOnScreen.x &&  _Right )
			Flip ();
		if( mouseOnScreen.x > characterOnScreen.x && !_Right )
			Flip ();

		// Set animator "Speed" value
		an.SetFloat("Speed", 
		            Mathf.Lerp ( 
		            	an.GetFloat("Speed"), 
		            	Mathf.Abs( rigidbody2D.velocity.x ) >= 1f ? 1 : 0,
		            	30 * Time.deltaTime
		            )
				);

		// Player movement
		rigidbody2D.velocity 
			= Vector2.Lerp ( 
			                rigidbody2D.velocity, 
			                new Vector2 ( inputHorizontal * Vector2.right.x * MaxSpeed, rigidbody2D.velocity.y ), 
			                MoveForce * Time.deltaTime 
						);

		// Rotate the rotatable objects
		foreach ( Transform t in RotatableObjects )
		{
			Quaternion rot  = t.transform.localRotation;
			angle 		    = Mathf.Atan( lenY/lenX ) * 180f/Mathf.PI;
			rot.eulerAngles = new Vector3 ( 0, 0, angle );
			t.localRotation	= rot;
		}

		// Change weapon
		for ( int i=0; i<Weapons.Length; i++ )
		{
			if ( Input.GetKeyDown ( Weapons[i].keyAssign ) && currentWeapon != Weapons[i] )
			{
				ChangeWeapon ( i );
			}
		}

		// Shoot
		if( currentWeapon.weaponType == WeaponType.Semi )
		{
			if( Input.GetButtonDown("Fire1") && Time.time - t_weapon >= currentWeapon.fireRate )
			{
				t_weapon = Time.time;
				Shoot();
			}
		}

		if( currentWeapon.weaponType == WeaponType.Auto )
		{
			if( Input.GetButton("Fire1") && Time.time - t_weapon >= currentWeapon.fireRate)
			{
				t_weapon = Time.time;
				Shoot();
			}
		}
	}


	/// <summary>
	/// Player Jumps.
	/// </summary>
	private void Jump ()
	{
		an.SetTrigger("Jump");
		rigidbody2D.AddForce(new Vector2(0f, JumpForce));

		if( GetComponent<PlayerView>() )
		{
			GetComponent<PlayerView>().Jump();
		}
	}


	/// <summary>
	/// Flip this gameObject.
	/// </summary>
	private void Flip ()
	{
		_Right 	 	 		 = !_Right;
		Vector3 newScale 	 = Flippable.localScale;
		newScale.x 	    	*= -1;
		Flippable.localScale = newScale;
	}


	/// <summary>
	/// Fire
	/// </summary>
	public void Shoot()
	{
		// ... set the animator Shoot trigger parameter
		an.SetTrigger ( "Shoot" );
		audio.PlayOneShot ( currentWeapon.shotSound );

		// If the player is facing right...
		if( _Right )
		{
			// Create a bullet instance
			Rigidbody2D bulletInstance = Instantiate ( currentWeapon.bullet, currentWeapon.muzzle.position, Quaternion.Euler ( new Vector3(0, 0, angle) ) ) as Rigidbody2D;
			bulletInstance.velocity    = new Vector2 ( lenX, lenY ).normalized * currentWeapon.speed;

			if( GetComponent<PlayerView>() ) 
			{
				GetComponent<PlayerView>().Shoot ( new Vector2 ( lenX, lenY ).normalized, angle );
			}

			bulletInstance.transform.GetComponent<Bullet>().SetData ( currentWeapon.power, true );
		}
		else
		{
			// Create a bullet instance
			Rigidbody2D bulletInstance = Instantiate ( currentWeapon.bullet, currentWeapon.muzzle.position, Quaternion.Euler ( new Vector3(0, 0, 180f-angle)) ) as Rigidbody2D;
			bulletInstance.velocity    = new Vector2 ( -lenX, lenY ).normalized * currentWeapon.speed;

			if( GetComponent<PlayerView>() ) 
			{
				GetComponent<PlayerView>().Shoot ( new Vector2 ( -lenX, lenY ).normalized, 180f-angle );
			}

			bulletInstance.transform.GetComponent<Bullet>().SetData ( currentWeapon.power, true );
		}
	}


	/// <summary>
	/// Changes the weapon.
	/// </summary>
	/// <param name="value">Value.</param>
	public void ChangeWeapon ( int value )
	{
		KillAllWeapon();
		Weapons[ value ].weaponRoot.gameObject.SetActive ( true );
		t_weapon 	  = -Weapons[ value ].fireRate;
		currentWeapon =  Weapons[ value ];

		if( GetComponent<PlayerView>() )
		{
			GetComponent<PlayerView>().ChangeWeapon ( value );
		}
	}


	/// <summary>
	/// Disable All weapons
	/// </summary>
	public void KillAllWeapon()
	{
		foreach( Weapon w in Weapons )
		{
			w.weaponRoot.gameObject.SetActive ( false );
		}
	}


	void OnApplicationQuit ()
	{
		isQuitting = true;
	}


	void OnDestroy() 
	{
		if( !isQuitting )
		{
			Instantiate ( DieExplosion, transform.position, Quaternion.identity );
		}
	}


	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		Gizmos.color = Color.red;
		Gizmos.DrawLine ( Camera.main.ScreenToWorldPoint(mouseOnScreen), Camera.main.ScreenToWorldPoint(characterOnScreen) );
#endif
	}
}
