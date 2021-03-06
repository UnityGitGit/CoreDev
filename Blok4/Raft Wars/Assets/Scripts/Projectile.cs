﻿using UnityEngine;
using UnityEngine.Events;

public delegate void SimpleDelegate();

public class Projectile : MonoBehaviour {

	public Transform myRaft;
	public int damage;
	public event SimpleDelegate onDestroy;
	public event DamageDelegate onDamageDealt;

	private void Start(){
		Invoke ("DestroySelf", 2f);
	}

	private void DestroySelf(){
		if (onDestroy != null) {
			onDestroy ();
		}

		GameObject.Destroy (gameObject);
	}

	private void OnTriggerEnter2D(Collider2D other){
		if (myRaft != null && other.transform != myRaft && other.tag == "Player") {
			other.GetComponent<Health> ().TakeDamage (damage);

			onDamageDealt (damage);
			DestroySelf ();
		}
	}
}
