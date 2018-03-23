using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public float levelStartDelay = 2f;
	public float turnDelay = 0.1f;
	public static GameManager instance = null;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	private Text levelText;
	private GameObject levelImage;
	private int level = 1;
	private List<Enemy> enemies;
	private bool enemiesMoving;
	private bool doingSetup;
	private bool skipLoading = true;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
		enemies = new List<Enemy>();
		boardScript = GetComponent<BoardManager>();
		InitGame();
	}

	//This is called each time a scene is loaded.
	//This should be deprecated in favor of the following
	//three methods (OnLevelFinishedLoading, OnEnable, OnDisable)
	//but the delegate appears to fire too many times.
	/*
	void OnLevelWasLoaded(int index)
	{
		//Add one to our level number.
		level++;
		//Call InitGame to initialize our level.
		InitGame();
	}
	*/
	
	//This is called each time a scene is loaded
	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
		if (skipLoading) {
			skipLoading = false;
			return;
		}
		//Increment level number
		level++;
		//Initialize level
		InitGame();
	}

	void OnEnable()
	{
		//Tell our 'OnLevelFinishedLoading' function to start listening for a scene change
		//event as soon as this script is enabled;
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		//Tell our OnLevelFinishedLoading fcn to stop listening for a scene change event as soon as
		//this script is disabled. Unsubscribe from every delegate you subscribe too!
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	void InitGame() {
		doingSetup = true;
		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		levelText.text = "Day " + level;
		levelImage.SetActive(true);
		Invoke("HideLevelImage", levelStartDelay);

		enemies.Clear();
		boardScript.SetupScene(level);
	}

	private void HideLevelImage() {
		levelImage.SetActive(false);
		doingSetup = false;
	}

	public void GameOver() {
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive(true);
		enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if (playersTurn || enemiesMoving || doingSetup) {
			return;
		}
		StartCoroutine(MoveEnemies());
	}

	public void AddEnemyToList(Enemy script) {
		enemies.Add(script);
	}

	IEnumerator MoveEnemies() {
		enemiesMoving = true;
		yield return new WaitForSeconds(turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds(turnDelay);
		}
		for (int ndx = 0; ndx < enemies.Count; ndx++) {
			enemies[ndx].MoveEnemy();
			yield return new WaitForSeconds(enemies[ndx].moveTime);
		}
		playersTurn = true;
		enemiesMoving = false;
	}
}
