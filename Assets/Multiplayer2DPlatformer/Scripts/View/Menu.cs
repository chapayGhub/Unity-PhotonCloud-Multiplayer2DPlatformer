using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class Menu : Photon.MonoBehaviour {

	#region public variables
	
	[System.Serializable]
	public class Map
	{
		[SceneName]
		public string 	 LevelName;
		[Range (2, 6) ]
		public int		 Capacity;
		[HideInInspector]
		public int       _Index;
	}

	public enum MenuState
	{
		Top, RoomList, HostGame, Settings
	}

	public GUISkin skin;

	public Map[] Maps;

	#endregion


	#region private variables

	private NetworkManager 
					  _manager = new NetworkManager();
	private MenuState _State;
	private bool	  connectFailed = false;
	private Map  	  selected;
	private Vector2	  scrollPos     = Vector2.zero;
	private string    roomInput;

	#endregion


	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake()
	{
		// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
		PhotonNetwork.automaticallySyncScene = true;
		
		// the following line checks if this client was just created (and not yet online). if so, we connect
		if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
		{
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
		
		// generate a name for this player, if none is assigned yet
		if (String.IsNullOrEmpty(PhotonNetwork.playerName))
		{
			PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
		}

		roomInput = "Room" + Random.Range(1, 9999);
		
		// if you wanted more debug out, turn this on:
		// PhotonNetwork.logLevel = NetworkLogLevel.Full;

		selected = Maps[0];

		// Init index
		for (int i=0; i<Maps.Length; i++)
		{
			Maps[i]._Index = i;
		}
	}


	/// <summary>
	/// Called when the master created room
	/// </summary>
	public void OnCreatedRoom()
	{
		PhotonNetwork.LoadLevel( selected.LevelName );
	}


	/// <summary>
	/// Draw GUI
	/// </summary>
	public void OnGUI()
	{
		GUI.skin = skin;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;

		// Show ping
		GUILayout.Label ( "Ping:" + PhotonNetwork.GetPing().ToString(), GUILayout.Width(150) );
		// Draw GUIWindow
		GUILayout.Window ( 1, new Rect ( (Screen.width-500)/2f , 60f, 500f, 400f ), DrawWindow, "Game Menu" );
	}

	
	/// <summary>
	/// Score board window
	/// </summary>
	/// <param name="id">Identifier.</param>
	private void DrawWindow(int id)
	{
		GUILayout.Space(1);
		// Show connection state
		if (!PhotonNetwork.connected)
		{
			if (PhotonNetwork.connectionState == ConnectionState.Connecting)
			{
				GUILayout.Label("Connecting...");
			}
			else if(PhotonNetwork.connectionState == ConnectionState.Disconnected || this.connectFailed)
			{
				GUILayout.Label("Not connected. Check console output.");
				if ( GUILayout.Button("Re-connect") )
				{
					this.connectFailed = false;
					PhotonNetwork.ConnectUsingSettings("1.0");
				}
			}
			return;
		}

		// Main Window
		switch ( _State )
		{
			#region Top
		case MenuState.Top:

			GUILayout.Label ( "You can play game from \"room list\" or \"host game!\"" );

			GUILayout.Space(5);

			GUILayout.Label ( "Player Name" );
			PhotonNetwork.playerName = GUILayout.TextField ( PhotonNetwork.playerName, 25 );

			GUILayout.Space(5);

			if(GUILayout.Button("Room List"))
			{
				_State = MenuState.RoomList;
			}

			GUILayout.Space(5);

			if(GUILayout.Button("Host Game"))
			{
				_State = MenuState.HostGame;
			}

			GUILayout.Space(5);

			if(GUILayout.Button(" Settings "))
			{
				_State = MenuState.Settings;
			}
			break;
			#endregion


			#region RoomList
		case MenuState.RoomList:

			GUILayout.Label ( "Room List" );

			GUILayout.Space(5);

			if ( PhotonNetwork.GetRoomList().Length == 0 )
			{
				GUILayout.BeginHorizontal ( "box" );
				GUILayout.Label ( "There is no room available..." );
				GUILayout.EndHorizontal ();
			}

			foreach ( RoomInfo r in PhotonNetwork.GetRoomList() )
			{
				string[] s = _manager.GetRoomInfo ( r );
				GUILayout.BeginHorizontal ( "box" );
				{
					GUILayout.Label ( "RoomName: " + s[0] );
					GUILayout.Label ( "Players: "  + s[1] +"/"+ s[2] );
					GUILayout.Label ( "Map: "	   + Maps[ int.Parse( s[3] ) ].LevelName );
					if ( GUILayout.Button ("Join") )
					{
						if (String.IsNullOrEmpty(PhotonNetwork.playerName))
						{
							PhotonNetwork.playerName = PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
						}

						PhotonNetwork.JoinRoom ( s[0] );
					}
				}
				GUILayout.EndHorizontal ();
			}

			GUILayout.Space(10);

			if(GUILayout.Button("Return to menu"))
			{
				_State = MenuState.Top;
			}
			break;
			#endregion


			#region HostGame
		case MenuState.HostGame:

			GUILayout.Label ( "Host Game" );

			GUILayout.Space(5);

			GUILayout.BeginVertical ( "box" );

			// Name input
			GUILayout.Label ( "Room Name" );
			roomInput = GUILayout.TextField ( roomInput, 25 );

			GUILayout.Space(5);

			// Map selection
			GUILayout.Label ( "Current Map: " + selected.LevelName );
			GUILayout.BeginScrollView ( scrollPos );
			{
				GUILayout.BeginVertical ( "box" );
				foreach ( Map m in Maps )
				{
					if ( GUILayout.Button ( m.LevelName ) )
					{
						selected = m;
					}
				}
				GUILayout.EndVertical ();
			}
			GUILayout.EndScrollView ();

			GUILayout.EndVertical ();

			if(GUILayout.Button("Create a New Game"))
			{
				if (String.IsNullOrEmpty(roomInput))
				{
					roomInput = "Room" + Random.Range(1, 9999);
				}

				if (String.IsNullOrEmpty(PhotonNetwork.playerName))
				{
					PhotonNetwork.playerName = PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
				}

				_manager.CreateRoom ( roomInput, selected._Index, selected.Capacity );
			}
			
			GUILayout.Space(10);
			
			if(GUILayout.Button("Return to menu"))
			{
				_State = MenuState.Top;
			}
			break;
			#endregion


			#region Settings
		case MenuState.Settings:

			GUILayout.Label ( "Settings" );

			GUILayout.Space(5);

			GUILayout.Label ( "Quality" );
			string[] names = QualitySettings.names;
			GUILayout.BeginHorizontal( "box" );
			int i = 0;
			while (i < names.Length) {
				if (GUILayout.Button(names[i]))
					QualitySettings.SetQualityLevel(i, true);
				i++;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			
			if(GUILayout.Button("Return to menu"))
			{
				_State = MenuState.Top;
			}
			break;
			#endregion
		}
	}
}
