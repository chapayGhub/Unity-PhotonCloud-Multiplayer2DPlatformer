using UnityEngine;
using System.Collections;

public class NetworkManager {

	/// <summary>
	/// Gets the room info.
	/// </summary>
	/// <returns>The room info.</returns>
	/// <param name="info">Info.</param>
	public string[] GetRoomInfo ( RoomInfo info )
	{
		string[] s = new string[4];
		s[0] = info.name;
		s[1] = info.playerCount.ToString();
		s[2] = info.maxPlayers.ToString();
		s[3] = ( info.customProperties["index"] ).ToString();
		return s;
	}

	/// <summary>
	/// Creates the room.
	/// </summary>
	/// <param name="roomName">Room name.</param>
	public void CreateRoom ( string roomName, int mapIndex, int maxPlayer )
	{
		ExitGames.Client.Photon.Hashtable toSet = new ExitGames.Client.Photon.Hashtable();
		toSet.Add("index", mapIndex);
		
		string[] forLobby = new string[1];
		forLobby[0] = "index";
		
		PhotonNetwork.CreateRoom ( roomName, true, true, maxPlayer, toSet, forLobby);
	}

	/// <summary>
	/// Joins the room.
	/// </summary>
	/// <param name="roomName">Room name.</param>
	public void JoinRoom ( string roomName )
	{
		PhotonNetwork.JoinRoom ( roomName, true );
	}

	/// <summary>
	/// Joins the room.
	/// </summary>
	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom ( "TEST", true );
	}
}
