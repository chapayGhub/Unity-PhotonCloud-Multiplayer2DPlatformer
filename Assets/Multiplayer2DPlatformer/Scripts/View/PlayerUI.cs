using UnityEngine;
using System.Collections;

public class PlayerUI : Photon.MonoBehaviour {

	#region public variables

	// A skin for GUI description
	public GUISkin skin;

	// Local player's Name-UI color
	public Color nameColor_Local  = Color.red;

	// Other player's Name-UI color
	public Color nameColor_Player = Color.cyan;

	// Player's health bar
	public SpriteRenderer healthBar;

	// Player name will be described at this position
	public Transform namePoint;

	#endregion


	#region private variables

	private RoomManager manager { get{ return RoomManager.Instance; } }
	private PlayerData player 	{ get{ return manager.players[ photonView.owner.ID ]; } }
	private Vector3 healthScale;
	private string playerName   { get{ return photonView.owner.name; } }
	private Vector3 charaPos    { get{ return Camera.main.WorldToScreenPoint(namePoint.position); } }

	#endregion


	/// <summary>
	/// Initialization
	/// </summary>
	private void Start()
	{
		healthScale = healthBar.transform.localScale;
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update()
	{
		UpdateHealthBar ();
	}


	/// <summary>
	/// Updates the health bar.
	/// </summary>
	private void UpdateHealthBar()
	{
		int h = Mathf.Max ( 0, player.Health );

		// Set the health bar's colour to proportion of the way between green and red based on the player's health.
		healthBar.material.color = Color.Lerp(Color.green, Color.red, 1 - h * 0.01f);

		// Set the scale of the health bar to be proportional to the player's health.
		healthBar.transform.localScale = new Vector3(healthScale.x * h * 0.01f, 1, 1);

	}


	private void OnGUI()
	{
		// Apply our skin
		GUI.skin = skin;

		string newLabel;
		Color  newColor;

		// Set name and color 
		if( player.id == PhotonNetwork.player.ID )
		{
			newLabel = "You";
			newColor = nameColor_Local;
		}
		else
		{
			newLabel = playerName;
			newColor = nameColor_Player;
		}

		// Shadow
		GUI.color = Color.black;
		GUI.Label ( new Rect(charaPos.x + 1f, Screen.height - charaPos.y - 90f+1f, 300, 50), newLabel);

		// Describe player name
		GUI.color = newColor;
		GUI.Label ( new Rect(charaPos.x, Screen.height - charaPos.y - 90f, 300, 50), newLabel);
	}

}
