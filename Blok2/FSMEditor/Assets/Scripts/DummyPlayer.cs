using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour, IDamageable {
	
	public float speed = 1f;

	public int health = 8;
	public UnityEngine.UI.Text healthText;
	public float healthYOffset = 0.5f;

	void Start () {
		ApplyDamage (0);
		onDie += DestroyMe;
	}

	void Update () {
		transform.Translate (transform.right * Input.GetAxis ("Horizontal") * speed * Time.deltaTime);
		healthText.transform.position = HPTextPos ();
	}

	Vector3 HPTextPos (){
		Vector3 pos = transform.position;
		pos.y += healthYOffset;
		return pos;
	}

	public void ApplyDamage (int dmg) {
		health -= dmg;
		healthText.text = health.ToString();
		if (health <= 0) {
			onDie ();
		}
	}

	public event DefaultEvent onDie;

	void DestroyMe(){
		Destroy (healthText.gameObject);
		Destroy (gameObject);
	}
}
