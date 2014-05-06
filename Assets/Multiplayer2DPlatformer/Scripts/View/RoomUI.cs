using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RoomUI : MySingletonMonoBehaviour<RoomUI> {

	#region public variables

	// GUI skin
	public GUISkin skin;

	// Time left
	public GUIText TimeLeft;
	public GUIText TimeLeftShadow;

	// Count down text for the respawn
	public GUIText CountText;
	public GUIText CountTextShadow;

	// Return to menu button
	public TextAnchor ReturnMenuAllignment;
	public string  ReturnMenuText = "Return to menu";
	public Vector2 ReturnMenuOffset;

	// Score count
	public GUIText ScoreText;
	public GUIText ScoreTextShadow;
	public Color   ScoreTextHighlight;

	// Kill log text
	public TextAnchor KillLogAllignment;
	public Vector2 KillLogOffset;

	[Range(1f, 10f)]
	public float KillLogPersistence;

	[Range(10, 50)]
	public int KillLogFontSize;

	// Score Effect
	public GameObject scoreEffect;

	// Hit Effect
	public GameObject HitEffect;

	// Hit Sounds
	public AudioClip[] HitSounds;

	#endregion


	#region private variables

	// Refer RoomManager instance
	private RoomManager _manager { get{ return RoomManager.Instance; } }
	private string 		time 	 { get{ return _manager.TimeLeftText; } }
	private float  		scW  	 { get{ return Screen.width; } }
	private float  		scH   	 { get{ return Screen.height; } }
	private float  		countLeft;
	private bool   		isCounting;
	private int 		score    { get{ return _manager.myData.Score; } }
	private Color	    scoreTextDefault;
	private float		scoreUI = .0f;
	private	int			scoreTextFont;

	// Kill log list
	private List<string> KillLog = new List<string>();

	#endregion


	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
		Init();
	}


	/// <summary>
	/// Initialize
	/// </summary>
	private void Init()
	{
		countLeft 		 = .0f;
		isCounting 		 = false;
		scoreTextDefault = ScoreText.color;
		ScoreText.text   = "Score:0";
		scoreTextFont    = ScoreText.fontSize;
		EndCount();
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update()
	{
		countLeft = Mathf.Max ( 0f, countLeft-Time.deltaTime );
		scoreUI   = Mathf.Min ( ( float )score, scoreUI + 100*Time.deltaTime );
	}


	private void OnGUI()
	{
		GUI.skin = skin;

		// Update left time text
		TimeLeft.text = TimeLeftShadow.text = time;

		// Show pause button
		skin.button.alignment = ReturnMenuAllignment;
		GUILayout.BeginArea ( new Rect( ReturnMenuOffset.x, ReturnMenuOffset.y, scW, scH ) );
		if ( GUILayout.Button ( ReturnMenuText, GUILayout.ExpandWidth(false) ) )
		{
			PhotonNetwork.LeaveRoom ();
		}
		GUILayout.EndArea ();

		// Update Kill Log text
		skin.label.fontSize  = KillLogFontSize;
		skin.label.alignment = KillLogAllignment;
		GUILayout.BeginArea ( new Rect( KillLogOffset.x, KillLogOffset.y, scW, scH ) );
		foreach ( string s in KillLog )
		{
			GUILayout.Label ( s );
		}
		GUILayout.EndArea ();

		// Update score text
		ScoreText.text = ScoreTextShadow.text  = "Score:" + ((int)scoreUI).ToString();
		if ( score != (int)scoreUI )
		{
			ScoreText.color 		 = ScoreTextHighlight;
			ScoreText.fontSize 		 = ( int )( scoreTextFont*1.5f );
			ScoreTextShadow.fontSize = ( int )( scoreTextFont*1.5f );
		}
		else
		{
			ScoreText.color 		 = scoreTextDefault;
			ScoreText.fontSize 		 = scoreTextFont;
			ScoreTextShadow.fontSize = scoreTextFont;
		}

		// If game state is "playing" continue 
		if( _manager._State == GameState.Extension )
		{
			ShowScoreBoard ();
			return;
		}

		// If player is waiting for the respawn 
		if( isCounting )
		{
			if(countLeft > 0f)
			{
				CountText.text = CountTextShadow.text = Mathf.CeilToInt(countLeft).ToString();
			}
		}
		else if ( !_manager.IsDeployed )
		{
			GUILayout.BeginArea( new Rect(scW/2-100, scH/2-20, 200, 40) );
			skin.button.alignment = TextAnchor.MiddleCenter;
			if( GUILayout.Button ("Deploy", GUILayout.Width(200), GUILayout.Height(40) ))
			{
				_manager.SpawnPlayer ();
			}
			GUILayout.EndArea();
		}

	}


	/// <summary>
	/// Shows the score board.
	/// </summary>
	private void ShowScoreBoard ()
	{
		GUI.skin.label.alignment  = TextAnchor.UpperCenter;
		GUI.skin.window.alignment = TextAnchor.UpperCenter;
		GUILayout.Window ( 1, new Rect ( (scW-250)/2f , 130f, 250f, 350f ), DrawWindow, "Game Over !" );
	}


	/// <summary>
	/// Score board window
	/// </summary>
	/// <param name="id">Identifier.</param>
	private void DrawWindow ( int id )
	{
		List<KeyValuePair<int, PlayerData>> 
			list = new List<KeyValuePair<int, PlayerData>>( _manager.players );
		
		list.Sort ( intCmp );

		int i = 0;

		GUILayout.Label ( "Ranking" );
		foreach ( KeyValuePair<int, PlayerData> pl in list )
		{
			i ++;
			PlayerData data = pl.Value;
			GUILayout.BeginHorizontal ();
			{
				string s = 
					i.ToString() + ". "
						+ data.Name + " : "
						+ data.Score.ToString();
				GUILayout.Box ( s );
			}
			GUILayout.EndHorizontal ();
		}
		GUILayout.Space (15);
		GUILayout.BeginHorizontal ();
		{
			if ( GUILayout.Button("Return to Menu") )
			{
				PhotonNetwork.LeaveRoom ();
			}
		}
		GUILayout.EndHorizontal ();
	}


	/// <summary>
	/// Compares the two score elements
	/// </summary>
	/// <param name="kvp1">Kvp1.</param>
	/// <param name="kvp2">Kvp2.</param>
	private int intCmp( KeyValuePair<int, PlayerData> kvp1, KeyValuePair<int, PlayerData> kvp2)
	{
		return kvp2.Value.Score - kvp1.Value.Score;
	}


	/// <summary>
	/// Start count down for the respawn
	/// </summary>
	/// <param name="time">Time.</param>
	public void CountDown( int time )
	{
		isCounting = true;
		countLeft  = (float)time-.001f;
		CountText.transform.gameObject.SetActive ( true );
		CountTextShadow.transform.gameObject.SetActive ( true );
		Invoke ( "EndCount", time );
	}


	/// <summary>
	/// Ends the count down.
	/// </summary>
	private void EndCount()
	{
		isCounting = false;
		CountText.transform.gameObject.SetActive ( false );
		CountTextShadow.transform.gameObject.SetActive ( false );
	}


	/// <summary>
	/// Adds the kill log.
	/// </summary>
	/// <param name="attacker">Attacker.</param>
	/// <param name="dead">Dead.</param>
	public void AddKillLog ( string attacker, string dead )
	{
		string txt = "<color=red>" + attacker + "</color>"+ " " + "killed" + " " + "<color=blue>" + dead + "</color>"; 
		KillLog.Add ( txt );
		Invoke ( "RemoveKillLog", KillLogPersistence );
	}


	/// <summary>
	/// Removes the kill log.
	/// </summary>
	private void RemoveKillLog ()
	{
		KillLog.RemoveAt ( 0 );
	}


	/// <summary>
	/// Show the score effect
	/// </summary>
	public void ScoreEffect ( Vector3 p )
	{
		Instantiate ( scoreEffect, p, Quaternion.identity );
	}


	/// <summary>
	/// Show the hit effect
	/// </summary>
	public void HitDamage ()
	{
		HitEffect.SetActive ( true );
		audio.PlayOneShot ( HitSounds [ Random.Range( 0, HitSounds.Length ) ] );
		Invoke ( "EndHitDamage", .01f );
	}


	/// <summary>
	/// Ends the hit damage.
	/// </summary>
	public void EndHitDamage ()
	{
		HitEffect.SetActive ( false );
	}
}
