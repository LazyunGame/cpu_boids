using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockRunner : MonoBehaviour
{
	public GameObject prefab;
	public GameObject attackTarget;
	public bool isStopAfterAttacked = true;
	GameObject[] birds;
	Boid[] boids;


	public int count = 200;

	void Start ()
	{
		boids = new Boid[count];
		birds = new GameObject[count];

		int width = 100;

		for (var i = 0; i < count; i++) {
			var boid = boids [i] = new Boid ();

			boid.setRandomPosition (width);
			boid.setRandomVelocity ();
			boid.setAvoidWalls (true);
			boid.setWorldSize (width, width, width);

			var bird = birds [i] = GameObject.Instantiate (prefab);
		}
	}

	void Update ()
	{
		for (int i = 0; i < birds.Length; i++) {
			var boid = boids [i];
			var bird = birds [i];
			boid.run (boids);
			bird.transform.position = boid.position;
			if (boid.velocity != Vector3.zero) {
				Quaternion b = Quaternion.identity;
				b.SetLookRotation (boid.velocity);
				Quaternion a = Quaternion.Lerp (bird.transform.localRotation, b, 1);
				bird.transform.localRotation = a;
			}
//			Debug.Log (boid.velocity + "  " + bird.transform.localRotation);
		}
	}

	public void OnGUI ()
	{
		if (GUI.Button (new Rect (10, 10, 100, 100), "Attack")) {
			for (int i = 0; i < birds.Length; i++) {
				var boid = boids [i];
				 
				boid.attack (attackTarget, isStopAfterAttacked);
			}
		}
	}
	 
}
