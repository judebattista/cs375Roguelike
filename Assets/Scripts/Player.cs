using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Player : MovingObject
{
	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;
	private int food;
	private Vector2 touchOrigin = -Vector2.one;
	private int previousXmove = 0;
	private int previousYmove = 0;
	private bool charging = false;
	private int damageToEnemy = 50;

	// Use this for initialization
	protected override void Start()
	{
		animator = GetComponent<Animator>();
		food = GameManager.instance.playerFoodPoints;
		foodText.text = "Food: " + food;
		base.Start();
	}

	private void OnDisable()
	{
		GameManager.instance.playerFoodPoints = food;
	}

	// Update is called once per frame
	void Update()
	{
		if (!GameManager.instance.playersTurn)
		{
			return;
		}
		int horizontal = 0;
		int vertical = 0;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		//Keyboard driven
		horizontal = (int)Input.GetAxisRaw("Horizontal");
		vertical = (int)Input.GetAxisRaw("Vertical");

		//No diagonal movement
		if (horizontal != 0)
		{
			vertical = 0;
		}

#else
		//Swipe driven
		if (Input.touchCount > 0) {
			Touch myTouch = Input.touches[0];
			if (myTouch.phase == TouchPhase.Began)
			{
				touchOrigin = myTouch.position;
			}
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;
				if (Mathf.Abs(x) > Mathf.Abs(y))
				{
					horizontal = x > 0 ? 1 : -1;
				}
				else {
					vertical = y > 0 ? 1 : -1;
				}
			}
		}
#endif
		if (horizontal + vertical != 0)
		{
			//Because this is a player moving, it expects to possibly interact with a wall
			AttemptMove<MonoBehaviour>(horizontal, vertical);
		}
	}

	protected override void AttemptMove<T>(int xDir, int yDir)
	{
		//Movement takes food
		food--;
		foodText.text = "Food: " + food;
		//If we're trying to move the exact same direction we moved last time, we are officially charging
		//If we stood still (xDir = yDir = 0), we are not charging.
		if (xDir == this.previousXmove && yDir == this.previousYmove && xDir + yDir > 0)
		{
			charging = true;
			Debug.Log("Charging lazers...");
		}
		else {
			charging = false;
		}
		base.AttemptMove<T>(xDir, yDir);
		RaycastHit2D hit;
		if (Move(xDir, yDir, out hit))
		{
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
			//Update the historical move direction
			this.previousXmove = xDir;
			this.previousYmove = yDir;
		}
		CheckIfGameOver();
		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Exit")
		{
			Invoke("Restart", restartLevelDelay);
			enabled = false;
		}
		else if (other.tag == "Food")
		{
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food; " + food;
			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
			Destroy(other.gameObject);
		}
		else if (other.tag == "Soda")
		{
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food; " + food;
			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
			Destroy(other.gameObject);
		}
		else
		{
			Debug.Log("2D collision detected");
		}
	}

	protected override void OnCantMove<T>(T component)
	{
		this.previousXmove = 0;
		this.previousYmove = 0;
		Debug.Log("Player can't move");
		//Check to see what's blocking the move
		//If it's a wall, try to break it
		if (component.CompareTag("Wall")) {
			Debug.Log("Player tried to enter a space blocked by " + component.tag);
			Wall hitWall = component as Wall;
			hitWall.DamageWall(wallDamage);
			animator.SetTrigger("PlayerChop");
		}
		//If it's an enemy, attack it.
		if (component.CompareTag("Enemy"))
		{
			Debug.Log("Player tried to enter a space blocked by " + component.tag);
			Enemy hitEnemy = component as Enemy;
			//baseline damage
			int damageDealt = GameManager.instance.playerDamageToEnemy;
			//if the player is charging, they deal double damage
			if (charging) {
				damageDealt *= 2;
			}
			Debug.Log("Dealing " + damageDealt + " to an enemy.");
			//If the player kills the enemy they get a damage boost.
			if (hitEnemy.TakeDamage(damageDealt))
			{
				GameManager.instance.playerDamageToEnemy++;
			}
			animator.SetTrigger("PlayerChop");
		}
		else {
			Debug.Log("Player tried to enter a space blocked by " + component.tag);
		}
		this.charging = false;
	}

	private void Restart()
	{
		//Application.LoadLevel(Application.loadedLevel);
		//Deprecated in favor of the following
		SceneManager.LoadScene(0);
	}

	public void LoseFood(int loss)
	{
		animator.SetTrigger("PlayerHit");
		food -= loss;
		foodText.text = "-" + loss + " Food; " + food;
		CheckIfGameOver();
	}

	private void CheckIfGameOver()
	{
		if (food <= 0)
		{
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();
			GameManager.instance.GameOver();
		}
	}
}
