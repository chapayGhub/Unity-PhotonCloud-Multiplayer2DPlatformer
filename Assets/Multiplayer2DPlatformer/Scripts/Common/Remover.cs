using UnityEngine;
using System.Collections;

public class Remover : MonoBehaviour
{
	public GameObject splash;


	void OnTriggerEnter2D(Collider2D col)
	{
		// If the player hits the trigger...
		if(col.gameObject.tag == "Player")
		{
			if(PhotonNetwork.connected)
			{
				if( col.gameObject == RoomManager.Instance.myPlayerObject )
				{
					RoomManager.Instance.DestroyPlayer();
				}
			}
			else
			{
				// ... destroy the player.
				Destroy (col.gameObject);
				Invoke ( "ReloadGame", 1.0f );
			}
			// ... instantiate the splash where the player falls in.
			Instantiate(splash, col.transform.position, transform.rotation);
		}
		else
		{
			// ... instantiate the splash where the enemy falls in.
			Instantiate(splash, col.transform.position, transform.rotation);
			// Destroy the enemy.
			Destroy (col.gameObject);	
		}
	}

	void ReloadGame()
	{			
		Application.LoadLevel(Application.loadedLevel);
	}
}
