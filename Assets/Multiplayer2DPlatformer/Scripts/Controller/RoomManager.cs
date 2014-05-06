using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Player data.
/// </summary>
public class PlayerData
{
	// Photon Player
	public PhotonPlayer player;
	// Photon ID
	public int id 	  	 = -1;
	// Left life
	public int Health 	 = 100;
	// Total Score
	public int Score 	 = 0;
	// Player Name
	public string Name	 = "GUEST";
}

/// <summary>
/// Game state.
/// </summary>
public enum GameState
{
	Extension, Playing
}

/// <summary>
/// Room manager.
/// </summary>
public class RoomManager : MySingletonMonoBehaviour<RoomManager> {

	#region public variables

	// If debug mode, some player data will be showed on game view
	public bool debugMode;

	// Dictionary of network player's data
	public Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

	// Controllable player
	public GameObject playerPrefab;

	// Player will spawn at this position
	public Transform  spawnPoint;

	// Game Time [Min]
	[Range(1, 10)]
	public int GameTime;

	// Extension time after an end of the game [Sec]
	[Range(5, 60)]
	public int ExtensionTime;

	// Time wait for deploy
	[Range(1, 10)]
	public int DeployTime;

	// My data
	[HideInInspector]
	public PlayerData 
		myData;

	// Used to access the instantiated local-player object
	[HideInInspector]
	public GameObject 
		myPlayerObject;

	[HideInInspector]
	public bool
		IsDeployed = false;

	[HideInInspector]
	public string TimeLeftText
	{ 
		get { 
			string a, b;
			
			a = String.Format("{0:D2}", (int)( Mathf.CeilToInt(TimeLeft) / 60) );
			b = String.Format("{0:D2}", Mathf.CeilToInt(TimeLeft) % 60 );
			
			return a + ":" + b;
		}
	}

	[HideInInspector]
	public GameState 
		_State;

	#endregion


	#region private variables

	private float 	  GameTimeSec { get{ return (float)GameTime*60f; } }
	private float 	  TimeLeft;

	#endregion


	/// <summary>
	/// Initialize
	/// </summary>
	private void Awake()
	{
		if (!PhotonNetwork.connected)
		{
			Application.LoadLevel( "Menu" );
			return;
		}
		TimeLeft = GameTimeSec;
		_State   = GameState.Playing;
	}


	/// <summary>
	/// Start this instance.
	/// </summary>
	private void Start ()
	{
		Connected();
	}


	/// <summary>
	/// Update game state
	/// </summary>
	private void Update()
	{
		if( PhotonNetwork.isMasterClient )
		{
			UpdateTime();
		}
		if( _State == GameState.Extension && IsDeployed )
		{
			print("d");
			DestroyPlayer();
		}
	}


	/// <summary>
	/// Only master client updates the game time
	/// </summary>
	private void UpdateTime()
	{
		// Master updates time and states of the game
		TimeLeft -= Time.deltaTime;
		if(TimeLeft <= 0)
		{
			switch ( _State )
			{
			case GameState.Extension:

				_State   = GameState.Playing;
				TimeLeft = GameTimeSec;
				photonView.RPC ( "StartGame", PhotonTargets.All );
				break;

			case GameState.Playing:

				_State   = GameState.Extension;
				TimeLeft = ExtensionTime;
				photonView.RPC ( "EndGame", PhotonTargets.All );
				break;

			}
		}
	}


	/// <summary>
	/// Instantiate Photon Player
	/// </summary>
	public void SpawnPlayer()
	{
		myPlayerObject = PhotonNetwork.Instantiate ( playerPrefab.name, spawnPoint.position, Quaternion.identity, 0 ) as GameObject;
		photonView.RPC ( "SetHealth", PhotonTargets.All, 100, myData.id );
		IsDeployed = true;
	}


	/// <summary>
	/// Called when local player joined room
	/// </summary>
	private void Connected()
	{
		photonView.RPC ( "AddPlayerData", PhotonTargets.AllBuffered, PhotonNetwork.player );
	}


	/// <summary>
	/// Called when other photon player connected
	/// </summary>
	/// <param name="player">Player.</param>
	public void OnPhotonPlayerConnected ( PhotonPlayer player )
	{
		photonView.RPC ( "SetHealth", player, myData.Health, myData.id );
		photonView.RPC ( "SetScore",  player, myData.Score );
	}


	/// <summary>
	/// Called when a player disconnected from photon / left room
	/// </summary>
	/// <param name="player">Player.</param>
	public void OnPhotonPlayerDisconnected ( PhotonPlayer player )
	{
		RemovePlayerData ( player.ID );
		PhotonNetwork.DestroyPlayerObjects ( player );
	}


	/// <summary>
	/// Add PlayerData to the dictionary
	/// </summary>
	/// <param name="id">Identifier.</param>
	[RPC]
	public void AddPlayerData ( PhotonPlayer player )
	{
		// Create new data for the List
		PlayerData data = new PlayerData();

		// If this is the local player, set the instance to myData
		if( player == PhotonNetwork.player )
			myData = data;

		int id = player.ID;

		// Add and set data
		players.Add ( id, data );
		players[id].player = player;
		players[id].id 	   = id;
		players[id].Health = 100;
		players[id].Score  = 0;
		players[id].Name   = player.name;
	}


	/// <summary>
	/// Remove PlayerData from the dictionary
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void RemovePlayerData ( int id )
	{
		// Remove player ID from the List
		players.Remove ( id );
	}


	/// <summary>
	/// Set network player's score
	/// </summary>
	/// <param name="score">Score.</param>
	/// <param name="info">Info.</param>
	[RPC]
	public void SetScore ( int score, PhotonMessageInfo info )
	{
		// Set new score
		players[info.sender.ID].Score = score;
	}


	/// <summary>
	/// Set network player's health
	/// </summary>
	/// <param name="newHealth">New health.</param>
	/// <param name="targetID">Target I.</param>
	/// <param name="info">Info.</param>
	[RPC]
	public void SetHealth ( int newHealth, int targetID, PhotonMessageInfo info )
	{
		// Set new health
		players[targetID].Health = newHealth;

		// If the local player was killed..
		if ( myData.Health <= 0 && IsDeployed )
		{
			PhotonNetwork.Destroy ( myPlayerObject );
			photonView.RPC ( "Die", PhotonTargets.All, info.sender );
		}
		// If the local player was attacked
		else if ( targetID == PhotonNetwork.player.ID && targetID != info.sender.ID )
		{
			// Show a hit effect
			RoomUI.Instance.HitDamage ();
		}
	}


	/// <summary>
	/// Called when a player die
	/// </summary>
	/// <param name="attacker">Attacker.</param>
	/// <param name="info">Info.</param>
	[RPC]
	public void Die ( PhotonPlayer attacker, PhotonMessageInfo info )
	{
		RoomUI.Instance.AddKillLog ( attacker.name, info.sender.name );

		// If the local player was killed by the attacker
		if ( info.sender == PhotonNetwork.player )
		{
			IsDeployed = false;

			if( _State == GameState.Extension ) 
				return;

			// Start count down for the next deploy
			RoomUI.Instance.CountDown ( DeployTime );
		}

		// If the local player killed info.sender
		if ( attacker == PhotonNetwork.player )
		{
			// Set my score
			photonView.RPC ( "SetScore", PhotonTargets.All, myData.Score + 100 );
			// Show score effect
			RoomUI.Instance.ScoreEffect ( myPlayerObject.transform.position );
		}
	}


	/// <summary>
	/// Destroy my photon player instance
	/// </summary>
	public void DestroyPlayer()
	{
		if ( IsDeployed )
		{
			PhotonNetwork.Destroy ( myPlayerObject );
		}
		IsDeployed = false;
	}


	/// <summary>
	/// Called when the game was over
	/// </summary>
	[RPC]
	public void StartGame()
	{
		// Clear score
		photonView.RPC ( "SetScore", PhotonTargets.All, 0 );
	}


	/// <summary>
	/// Called when a new game started
	/// </summary>
	[RPC]
	public void EndGame()
	{
		// Destroy local player
		DestroyPlayer ();
	}


	/// <summary>
	/// Raises the photon serialize view event.
	/// </summary>
	/// <param name="stream">Stream.</param>
	/// <param name="info">Info.</param>
	private void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
	{
		if ( !PhotonNetwork.connected ) 
			return;

		if ( stream.isWriting && PhotonNetwork.isMasterClient )
		{
			// Master sends the game time and state
			stream.SendNext ( TimeLeft );
			stream.SendNext ( (int)_State );
		}
		else if ( stream.isReading )
		{
			// Non masters receive the game time and state
			TimeLeft = ( float ) stream.ReceiveNext();
			_State   = (GameState) stream.ReceiveNext();
		}
	}


	public void OnLeftRoom()
	{
		// back to main menu        
		Application.LoadLevel( "Menu" );
	}


	/// <summary>
	/// For debugging
	/// </summary>
	private void OnGUI()
	{
		if(debugMode)
		{
			if ( !IsDeployed )
			{
				if ( GUILayout.Button("Spawn") )
					SpawnPlayer();
			}

			if ( IsDeployed )
			{
				if ( GUILayout.Button("Destroy") )
					photonView.RPC ( "SetHealth", PhotonTargets.All, 0, myData.id );
			}

			if ( GUILayout.Button("ChangeState") && PhotonNetwork.isMasterClient )
			{
				TimeLeft = 1f;
			}

			if ( GUILayout.Button("Return to menu") )
			{
				PhotonNetwork.LeaveRoom ();
			}

			foreach( PhotonPlayer pl in PhotonNetwork.playerList )
			{
				try
				{
					GUILayout.Label("ID:"+pl.ID+" Name:"+pl.name+" Life:"+players[pl.ID].Health+" Score:"+players[pl.ID].Score);
				}
				catch{}
			}

		}
	}

}
