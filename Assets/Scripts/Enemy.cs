﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int playerDamage;
	public int hitPoints;
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;

	public AudioClip chopSound1;
	public AudioClip chopSound2;
	private Animator animator;
	private Transform target;
	private bool skipMove;

	protected override void Start () {
		GameManager.instance.AddEnemyToList(this);
		animator = GetComponent<Animator>();
		target = GameObject.FindGameObjectWithTag("Player").transform;
		base.Start();
	}

	protected override void AttemptMove<T>(int xDir, int yDir)
	{
		//Enemies only move every other turn
		if (skipMove) {
			skipMove = false;
			return;
		}
		base.AttemptMove<T>(xDir, yDir);
		skipMove = true;
	}

	public void MoveEnemy() {
		int xDir = 0;
		int yDir = 0;
		double xDistance = Mathf.Abs(target.position.x - transform.position.x);
		double yDistance = Mathf.Abs(target.position.y - transform.position.y);
		//If they're in the same row
		/*
		if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
		{
			yDir = target.position.y > transform.position.y ? 1 : -1;
		}
		else {
			xDir = target.position.x > transform.position.x ? 1 : -1;
		}
		*/
		if (xDistance < yDistance)
		{
			yDir = target.position.y > transform.position.y ? 1 : -1;
		}
		else
		{
			xDir = target.position.x > transform.position.x ? 1 : -1;
		}
		AttemptMove<Player>(xDir, yDir);
	}

	protected override void OnCantMove<T>(T component) {
		Player hitPlayer = component as Player;
		animator.SetTrigger("EnemyAttack");
		hitPlayer.LoseFood(playerDamage);
		SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
	}

	public bool TakeDamage(int damage) {
		bool killed = false;
		hitPoints -= damage;
		SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
		if (hitPoints <= 0)
		{
			killed = true;
			//gameObject.SetActive(false);
			Destroy(this.gameObject);
		}
		return killed;
	}
}
