﻿using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityStandardAssets.ImageEffects;

public class TransportManager : MBehavior {

	public TransportManager() { s_Instance = this; }
	public static TransportManager Instance { get { return s_Instance; } }
	private static TransportManager s_Instance;

	[SerializeField] ToColorEffect toColorEffect;
	[SerializeField] BloomAndFlares bloomAndFlares;
//	[SerializeField] float transportOffset = 1f;
	[SerializeField] float fadeTime = .5f;
	[SerializeField] float transportTime = 2f;
	[SerializeField] LineRenderer transportLine;
	[SerializeField] ParticleSystem transportCircle;

	[SerializeField] Transform posEnd;
	[SerializeField] Transform posCredits;
    private float height = -1f;
    private bool callOnce = true;

	[SerializeField] FinaleTrailEnable TrailLeft;
	[SerializeField] FinaleTrailEnable TrailRight;

	/// <summary>
	/// For the transport animation
	/// </summary>
	private Sequence transportSequence;
	public bool IsTransporting{
		get { 
			if ( transportSequence != null ) 
				return !transportSequence.IsComplete();
			return false;
		}
	}

	protected override void MAwake ()
	{
		base.MAwake ();
	}

	protected override void MStart ()
	{
		base.MStart ();
		toColorEffect = LogicManager.Instance.GetPlayerTransform ().GetComponentInChildren<ToColorEffect> ();
		bloomAndFlares = LogicManager.Instance.GetPlayerTransform ().GetComponentInChildren<BloomAndFlares> ();
	}

	protected override void MOnEnable ()
	{
		base.MOnEnable ();
		M_Event.inputEvents [(int)MInputType.Transport] += OnTransport;
		M_Event.inputEvents [(int)MInputType.FocusNewObject] += OnFocusNew;
		M_Event.inputEvents [(int)MInputType.OutOfFocusObject] += OnOutofFocus;
        M_Event.logicEvents[ ( int )LogicEvents.Finale ] += OnFinale;
        M_Event.logicEvents [(int)LogicEvents.End] += OnEnd;
		M_Event.logicEvents [(int)LogicEvents.Credits] += OnCredits;
	}

	protected override void MOnDisable ()
	{
		base.MOnDisable ();
		M_Event.inputEvents [(int)MInputType.Transport] -= OnTransport;
		M_Event.inputEvents [(int)MInputType.FocusNewObject] -= OnFocusNew;
		M_Event.inputEvents [(int)MInputType.OutOfFocusObject] -= OnOutofFocus;
        M_Event.logicEvents[ ( int )LogicEvents.Finale ] -= OnFinale;
        M_Event.logicEvents [(int)LogicEvents.End] -= OnEnd;
		M_Event.logicEvents [(int)LogicEvents.Credits] -= OnCredits;
	}

	PasserBy focusPasserby;
	public void OnFocusNew( InputArg arg )
	{
		PasserBy p = arg.focusObject.GetComponent<PasserBy> ();
		if (p != null && p != LogicManager.Instance.StayPasserBy) { 
			focusPasserby = p;
			if (transportLine != null) {
				Vector3 transportStart = Camera.main.transform.position;
				Vector3 transportToward = focusPasserby.GetObservePosition ();
				float length = (transportStart - transportToward).magnitude;
				transportStart.y = transportToward.y = .25f; 

				transportLine.enabled = true;
				transportLine.SetPosition (0, transportStart);
				transportLine.SetPosition (1, transportToward);
				transportLine.material.SetVector ("_Scale", new Vector4 (length * 2f, 1f, 1f, 1f));
			}

			if (transportCircle != null) {
				Vector3 transportToward = focusPasserby.GetObservePosition ();
				transportToward.y = .25f;
				transportCircle.transform.position = transportToward;
				transportCircle.gameObject.SetActive (true);
			}
		}
	}

	public void OnOutofFocus( InputArg arg )
	{
		PasserBy p = arg.focusObject.GetComponent<PasserBy> ();
		if ( focusPasserby == p) {
			if (transportLine != null) {
				transportLine.enabled = false;
			}
			if (transportCircle != null) {
				transportCircle.gameObject.SetActive (false);
			}
			focusPasserby = null;
		}
	}

	private MObject transportToObject;

	public void OnTransport ( InputArg arg )
	{
		if (InputManager.Instance.FocusedObject != null && InputManager.Instance.FocusedObject is PasserBy ) {

			// do not make a mutiple transport
			if (IsTransporting)
				return;

			transportToObject = InputManager.Instance.FocusedObject;

			// do not transport to myself
			if ( transportToObject == LogicManager.Instance.StayPasserBy)
				return;

			PasserBy p = transportToObject.GetComponent<PasserBy> ();
			if (p == null)
				return;
			
			// fire the transport start event
			LogicArg logicArg = new LogicArg (this);
			logicArg.AddMessage (Global.EVENT_LOGIC_TRANSPORTTO_MOBJECT, transportToObject);
			M_Event.FireLogicEvent ( LogicEvents.TransportStart, logicArg);

			// set up the animation sequence
			transportSequence = DOTween.Sequence ();
			// add the vfx if there is the image effect in the camera
			if (toColorEffect != null && bloomAndFlares != null) {
				transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 1f, fadeTime));
				transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 8f, fadeTime));
			}
				
			// set up the transport varible
			//Transform myTrans = LogicManager.Instance.GetPlayerTransform ();
			Vector3 target = p.GetObservePosition();
			//print ("my target.y is = " + target.y); //make sure world coord
			// if on bridge, set higher
			if (target.y > posEnd.position.y - 2f) {
				target.y = posEnd.position.y + .132f;
			} else {
				target.y = .246f;
			}

			//Debug.Log (Time.timeSinceLevelLoad + "; TransportStart to: " + target);
			//target.y = transform.position.y; /// + 0.68f; /// TODO: fix where pc y is set after transport

			transportSequence.Append (LogicManager.Instance.GetPlayerTransform ().DOMove (target , transportTime));
			// add the vfx if there is the image effect in the camera
			if (toColorEffect != null && bloomAndFlares != null) {
				transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 0f, fadeTime));
				transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 0f, fadeTime));
			}

			transportSequence.OnComplete (OnTransportCOmplete);
		}
	}

	void OnTransportCOmplete()
	{
		// fire the transport end event
		if (transportToObject != null) {
			LogicArg arg = new LogicArg (this);
			arg.AddMessage (Global.EVENT_LOGIC_TRANSPORTTO_MOBJECT, transportToObject);
			M_Event.FireLogicEvent ( LogicEvents.TransportEnd, arg);
		}
		transportSequence = null;
		transportToObject = null;
	}

    protected override void MUpdate( )
    {
        base.MUpdate( );

        if ( height > 0f && height < posEnd.position.y / 3f )
        {
			float distance = (TrailLeft.GetDistance () + TrailRight.GetDistance ()) / 300f;
			print (distance + " = distance in transport");
			Vector3 target = new Vector3 (transform.position.x, transform.position.y + distance, transform.position.z);
		
			transform.position = Vector3.Lerp (transform.position, target, 1f);
			height = transform.position.y;
        } 
		else if (height >= posEnd.position.y / 3f && callOnce )
        {
            LogicArg logicArg = new LogicArg( this );
            M_Event.FireLogicEvent( LogicEvents.End, logicArg );
            callOnce = false;
        }
    }

    void OnFinale( LogicArg arg )
    {
        Disable.Instance.DisableClidren( ); // Disable text instructions
        height = LogicManager.Instance.GetPlayerTransform( ).position.y;
        
    }

	void OnEnd( LogicArg arg ){

		//Debug.Log ("on end from transport manager");
		// fire the transport start event
		LogicArg logicArg = new LogicArg (this);
		logicArg.AddMessage (Global.EVENT_LOGIC_TRANSPORTTO_MOBJECT, transportToObject);
		M_Event.FireLogicEvent ( LogicEvents.TransportStart, logicArg);

		// set up the animation sequence
		transportSequence = DOTween.Sequence ();
		// add the vfx if there is the image effect in the camera
		if (toColorEffect != null && bloomAndFlares != null) {
			transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 1f, fadeTime));
			transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 8f, fadeTime));
		}

		//Transform myTrans = LogicManager.Instance.GetPlayerTransform ();
		Vector3 target = posEnd.position;

		transportSequence.Append (LogicManager.Instance.GetPlayerTransform ().DOMove (target , transportTime));
		// add the vfx if there is the image effect in the camera
		if (toColorEffect != null && bloomAndFlares != null) {
			transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 0f, fadeTime));
			transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 0f, fadeTime));
		}

		transportSequence.OnComplete (OnTransportCOmplete);
	}

	void OnCredits( LogicArg arg ){

		//Debug.Log ("on end from transport manager");
		// fire the transport start event
		LogicArg logicArg = new LogicArg (this);
		logicArg.AddMessage (Global.EVENT_LOGIC_TRANSPORTTO_MOBJECT, transportToObject);
		M_Event.FireLogicEvent ( LogicEvents.TransportStart, logicArg);

		// set up the animation sequence
		transportSequence = DOTween.Sequence ();
		// add the vfx if there is the image effect in the camera
		if (toColorEffect != null && bloomAndFlares != null) {
			transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 1f, fadeTime));
			transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 8f, fadeTime));
		}

		//Transform myTrans = LogicManager.Instance.GetPlayerTransform ();
		Vector3 target = posCredits.position;

		transportSequence.Append (LogicManager.Instance.GetPlayerTransform ().DOMove (target , transportTime));
		// add the vfx if there is the image effect in the camera
		if (toColorEffect != null && bloomAndFlares != null) {
			transportSequence.Append (DOTween.To (() => toColorEffect.rate, (x) => toColorEffect.rate = x, 0f, fadeTime));
			transportSequence.Join (DOTween.To (() => bloomAndFlares.bloomIntensity, (x) => bloomAndFlares.bloomIntensity = x, 0f, fadeTime));
		}

		transportSequence.OnComplete (OnTransportCOmplete);
	}
}
