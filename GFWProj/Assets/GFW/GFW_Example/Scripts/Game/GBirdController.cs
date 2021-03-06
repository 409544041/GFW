﻿using UnityEngine;
using System.Collections;
using GFW;

public class GBirdController : MonoBehaviour
{
	private GMoveScript moveScript_;

	public delegate void GEventDied ();

	public delegate void GEventScore ();

	public GEventDied eventDied;
	public GEventScore eventScore;

	void Awake ()
	{
		moveScript_ = GetComponent<GMoveScript> ();
	}

	void Start ()
	{
		moveScript_.enabled = false;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.name != "middle") {
			GEventMgr.GetInstance ().TriggerEvent ((int)GEventType.kEvent_GameOver, null);
		}
	}

	Collider2D preCollider = null;

	void OnTriggerExit2D (Collider2D other)
	{
		if (other.gameObject.name == "middle") {
			if (eventScore != null && other != preCollider) {
				Vector3 otherPos = other.gameObject.transform.position;
				Vector3 birdPos = transform.position;

				if (other.gameObject.transform.position.x + 28.0f < transform.position.x) {
					GLogUtility.LogInfo ("other pos = " + otherPos.ToString ());
					GLogUtility.LogInfo ("bird  pos = " + birdPos.ToString ());

					preCollider = other;
					eventScore ();
				}
			}
		}

	}

	void OnCollisionEnter2D (Collision2D coll)
	{
		if (coll.gameObject.name == "GameObject_btmColidder") {
			StopBirdFly ();
			GEventMgr.GetInstance ().TriggerEvent ((int)GEventType.kEvent_GameOver, null);	
		}
	}

	void StopBirdFly ()
	{
		GLogUtility.LogDebug ("StopBirdFly");
		moveScript_.CurForceOrSpeed = 0.0f;
		moveScript_.IsEnableMove = false;
		GetComponent<Animator> ().PlayOrStopAnimator (false);
	}
}