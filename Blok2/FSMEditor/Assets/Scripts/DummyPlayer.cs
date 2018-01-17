using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour, IDamageable {
	private Rigidbody2D rb;
	public float speed = 1f;

	public int health = 8;

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}

	void FixedUpdate () {
		rb.AddForce (transform.right * Input.GetAxis ("Horizontal") * speed);
	}

	public void ApplyDamage (int dmg) {
		health -= dmg;
		if (health <= 0) {
			onDie ();
			Destroy (gameObject);
		}
	}

	public event DefaultEvent onDie;
}
