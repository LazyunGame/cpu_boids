using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{


	/// <summary>
	/// Sets the random position 
	/// </summary>
	/// <param name="width">Width.</param>
	public void setRandomPosition (int width)
	{
		mPosition = Random.insideUnitSphere * width;
	}

	public void setRandomVelocity ()
	{
		mVelocity = Random.insideUnitSphere * 2;
	}



	// fields of boid
	Vector3 _acceleration;
	float _width = 500;
	float _height = 500;
	float _depth = 200;
	Vector3 _goal;
	float _neighborhoodRadius = 100;
	float _maxSpeed = 4;
	float _maxSteerForce = 0.1f;
	bool _avoidWalls = false;
	Vector3 mVelocity;
	Vector3 mPosition;
	Vector3 mAcceleration;
	GameObject target;
	bool isStopAfterAttacked;


	public Vector3 position {
		get {
			return mPosition;
		}
		 
	}

	public Vector3 velocity {
		get {
			return mVelocity;
		}
	}

	public void setGoal (Vector3 target)
	{
		this._goal = target;
	}

	public void setAvoidWalls (bool avoid)
	{
		this._avoidWalls = avoid;
	}

	public void setWorldSize (float width, float height, float depth)
	{
		this._width = width;
		this._height = height;
		this._depth = depth;
	}

	public void run (Boid[] boids)
	{
		if (_avoidWalls) {
			var v = new Vector3 (-_width, this.position.y, this.position.z);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;

			v = new Vector3 (_width, this.position.y, this.position.z);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;

			v = new Vector3 (this.position.x, -_height, this.position.z);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;

			v = new Vector3 (this.position.x, _height, this.position.z);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;

			v = new Vector3 (this.position.x, this.position.y, -_depth);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;

			v = new Vector3 (this.position.x, this.position.y, _depth);
			v = this.avoid (v);
			v *= 5;
			_acceleration += v;
		}


		if (Random.Range (0, 1f) > 0.5) {
			this.flock (boids);
		}
		this.move ();
	}


	public void flock (Boid[] boids)
	{
		if (target != null) {
			setGoal (target.transform.position);

			_avoidWalls = false;
		}

		if (_goal != Vector3.zero) {
			_acceleration += this.reach (_goal, 0.01f);
		}

		_acceleration += this.alignment (boids);
		_acceleration += this.cohesion (boids);
		_acceleration += this.separation (boids);

	}



	public void move ()
	{


		this.mVelocity += _acceleration;
		var l = this.mVelocity.magnitude;
		if (l > _maxSpeed) {
			this.mVelocity /= l / _maxSpeed;
		}

		var d = Vector3.Distance (position, _goal);
//		Debug.Log ("d: " + d);
		if (isStopAfterAttacked && d <= 25) {
			mVelocity = Vector3.zero;
			_acceleration = Vector3.zero;
			return;
		}

		this.mPosition += this.mVelocity;
		_acceleration = Vector3.zero;
	}

	public void checkBounds ()
	{
		if (this.mPosition.x > _width)
			this.mPosition.x = -_width;
		if (this.mPosition.x < -_width)
			this.mPosition.x = _width;
		if (this.mPosition.y > _height)
			this.mPosition.y = -_height;
		if (this.mPosition.y < -_height)
			this.mPosition.y = _height;
		if (this.mPosition.z > _depth)
			this.mPosition.z = -_depth;
		if (this.mPosition.z < -_depth)
			this.mPosition.z = _depth;
	}

	public Vector3 avoid (Vector3 target)
	{
		var steer = this.mPosition - target;

		return steer / steer.sqrMagnitude;
	}

	public void repulse (Boid target)
	{
		var distance = this.mPosition - target.mPosition;
		if (distance.magnitude < 150) {
			var steer = distance;
			steer /= 0.5f / distance.magnitude;
			_acceleration += steer;
		}
	}

	public Vector3 reach (Vector3 target, float amount)
	{
		return (target - this.mPosition) * amount;
	}

	public Vector3 alignment (Boid[] boids)
	{
		var count = 0;
		var velSum = Vector3.zero;

		for (int i = 0, il = boids.Length; i < il; i++) {
			if (Random.Range (0, 1f) > 0.6f)
				continue;
			var boid = boids [i];
			var distance = (boid.mPosition - this.mPosition).magnitude;
			if (distance > 0 && distance <= _neighborhoodRadius) {
				velSum += boid.mVelocity;
				count++;
			}
		}

		if (count > 0) {
			velSum /= count;
			var l = velSum.magnitude;
			if (l > _maxSteerForce) {
				velSum /= l / _maxSteerForce;
			}
		}
		return velSum;
	}

	public Vector3 cohesion (Boid[] boids)
	{
		var count = 0;
		var posSum = Vector3.zero;
		var steer = Vector3.zero;
		for (int i = 0, il = boids.Length; i < il; i++) {
			if (Random.Range (0, 1f) > 0.6f)
				continue;
			var boid = boids [i];
			var distance = (boid.mPosition - this.mPosition).magnitude;
			if (distance > 0 && distance <= _neighborhoodRadius) {
				posSum += boid.mPosition;
				count++;
			}
		}
		if (count > 0) {
			posSum /= count;
		}
		steer = posSum - this.mPosition;
		var l = steer.magnitude;
		if (l > _maxSteerForce) {
			steer /= l / _maxSteerForce;
		}
		return steer;
	}

	public Vector3 separation (Boid[] boids)
	{
		var posSum = Vector3.zero;
		var repulse = Vector3.zero;
		for (int i = 0, il = boids.Length; i < il; i++) {
			if (Random.Range (0, 1f) > 0.6)
				continue;
			var boid = boids [i];
			var distance = (boid.mPosition - this.mPosition).magnitude;
			if (distance > 0 && distance <= _neighborhoodRadius) {
				repulse = this.mPosition - boid.mPosition;
				repulse.Normalize ();
				repulse /= distance;
				posSum += repulse;
			}
		}
		return posSum;
	}


 

	public void attack (GameObject target, bool isStopAfterAttacked = false)
	{
		this.target = target;
		this.isStopAfterAttacked = isStopAfterAttacked;
	}

}
