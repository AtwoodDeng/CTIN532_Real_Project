﻿using UnityEngine;
using System.Collections;

public class PasserBy : MObject {

	[SerializeField] Transform outBody;
	[SerializeField] Transform innerWorld;
	[SerializeField] MeshRenderer[] outlineRenders;
	[SerializeField] Transform observeLocation;

	private Quaternion rotation;
	private GameObject player;

	// for the inner world
	[SerializeField] AudioClip innerWorldClip;

//	[Range(0,0.0001f)]
//	[SerializeField] float outLineWidth = 0.00005f;

	protected override void MAwake ()
	{
		base.MAwake ();

		rotation = transform.rotation;

		SetOutline (false);
		if (observeLocation == null)
			observeLocation = transform;
	}

	protected override void MOnEnable ()
	{
		base.MOnEnable ();
		player = GameObject.FindGameObjectWithTag ("Player");
		M_Event.logicEvents [(int)LogicEvents.TransportStart] += OnTransportStart;
		M_Event.logicEvents [(int)LogicEvents.TransportEnd] += OnTransportEnd;
	}

	protected override void MOnDisable ()
	{
		base.MOnDisable ();
		M_Event.logicEvents [(int)LogicEvents.TransportStart] -= OnTransportStart;
		M_Event.logicEvents [(int)LogicEvents.TransportEnd] -= OnTransportEnd;
	}

	void Update()
	{
		transform.rotation = rotation;
	}

	void OnTransportStart( LogicArg arg )
	{
		var obj = arg.GetMessage (Global.EVENT_LOGIC_TRANSPORTTO_MOBJECT);
		if (obj is PasserBy) {
			PasserBy pass = (PasserBy)obj;
			if (pass == this) {
				SetToLayer ("Focus");
			} else {
				SetToLayer ("PasserBy");
			}
		}
	}

	void OnTransportEnd( LogicArg arg )
	{
		
	}

	/// <summary>
	/// set this game object and all the body renders to a specific layer
	/// </summary>
	/// <param name="layer">Layer.</param>
	void SetToLayer( string layer )
	{
		gameObject.layer = LayerMask.NameToLayer (layer);
		foreach (Transform t in outBody.GetComponentsInChildren<Transform>() ) {
			t.gameObject.layer = LayerMask.NameToLayer (layer);
		}
		foreach (Transform t in innerWorld.GetComponentsInChildren<Transform>() ) {
			t.gameObject.layer = LayerMask.NameToLayer (layer);
		}

	}

	/// <summary>
	/// Set the outline render on or off(enable)
	/// </summary>
	/// <param name="isOn">If set to <c>true</c> is on.</param>
	void SetOutline( bool isOn )
	{
		foreach (MeshRenderer r in outlineRenders) {
			r.enabled = isOn;
		}
	}


	public override void OnFocus ()
	{
		base.OnFocus ();
	
		SetOutline (true);
	}

	public override void OnOutofFocus ()
	{
		base.OnOutofFocus ();

		SetOutline (false);
	}

	public Vector3 GetObservePosition()
	{
		Vector3 dirToPlayer = player.transform.position - observeLocation.transform.position;
		dirToPlayer = dirToPlayer.normalized;
		Vector3 pos = observeLocation.transform.position + dirToPlayer * .35f;
		print ("observe ps from passerby = " + pos);
		return pos;
	}

	public void EnterInnerWorld( Collider col )
	{
		LogicArg arg = new LogicArg (this);
		arg.AddMessage (Global.EVENT_LOGIC_ENTERINNERWORLD_CLIP, innerWorldClip);
		M_Event.FireLogicEvent (LogicEvents.EnterInnerWorld, arg);
	}

	public void ExitInnerWorld( Collider col )
	{
		LogicArg arg = new LogicArg (this);

		M_Event.FireLogicEvent (LogicEvents.ExitInnerWorld, arg);
	}
		
}
