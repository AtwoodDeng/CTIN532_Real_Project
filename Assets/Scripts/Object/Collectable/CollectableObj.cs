﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// The object that can be select and hold by the player
/// </summary>
public class CollectableObj : MObject {
	
	[SerializeField] MeshRenderer[] outlineRenders;
	[SerializeField] protected AudioClip selectSound;
	protected AudioSource selectSoundSource;
	[SerializeField] protected AudioClip unselectSound;
	protected AudioSource unselectSoundSource;

	[SerializeField] protected AudioClip storySound;
	protected AudioSource storySoundSource;

	[SerializeField] protected Transform originalParentTransform;
	private Vector3 originalPos;
	private Quaternion originalRot;
	public bool matched = false;
	[SerializeField] float offset;
	private Material material;
	private Color color;
	[SerializeField] float outlineWidth;

	protected override void MAwake ()
	{
		base.MAwake ();

		material = new Material(Shader.Find("Outlined/Silhouette Only"));

		// turn off the outline 
		SetOutline (false);

		if (gameObject.tag == "Raise" || gameObject.tag == "TutorialRight") {
			foreach (MeshRenderer r in outlineRenders) {
				r.material = material;
				ColorUtility.TryParseHtmlString ("#FFACF9FF", out color);
				r.material.SetFloat ("_Outline", outlineWidth);
				r.material.SetVector ("_OutlineColor", color);
			}
			transform.localPosition += new Vector3(0f, 0f, offset);
		} else if (gameObject.tag == "Lower" || gameObject.tag == "TutorialLeft") {
			foreach (MeshRenderer r in outlineRenders) {
				r.material = material;
				ColorUtility.TryParseHtmlString ("#00FFFFFF", out color);
				r.material.SetFloat ("_Outline", outlineWidth);
				r.material.SetVector ("_OutlineColor", color);
			}
			transform.localPosition += new Vector3(0f, 0f, -offset);
			transform.localRotation = Quaternion.AngleAxis(180f, transform.up);
		} 

		originalParentTransform = transform.parent;
		originalPos = transform.localPosition;
		originalRot = transform.localRotation;
			
		// set up the select sound
		if (selectSound != null) {
			selectSoundSource = gameObject.AddComponent<AudioSource> ();
			selectSoundSource.playOnAwake = false;
			selectSoundSource.loop = false;
			selectSoundSource.volume = 0.5f;
			selectSoundSource.spatialBlend = 1f;
			selectSoundSource.clip = selectSound;
		}
		// set up the unselect sound
		if (unselectSound != null) {
			unselectSoundSource = gameObject.AddComponent<AudioSource> ();
			unselectSoundSource.playOnAwake = false;
			unselectSoundSource.loop = false;
			unselectSoundSource.volume = 0.5f;
			unselectSoundSource.spatialBlend = 1f;
			unselectSoundSource.clip = unselectSound;
		}
		// set up the story sound
		if (storySound != null) {
			storySoundSource = gameObject.AddComponent<AudioSource> ();
			storySoundSource.playOnAwake = false;
			storySoundSource.loop = false;
			storySoundSource.volume = 1f;
			storySoundSource.spatialBlend = 1f;
			storySoundSource.clip = storySound;
		}
	}

	public override void OnFocus ()
	{
		base.OnFocus ();
		SetOutline (true);
		if ( storySoundSource != null && !storySoundSource.isPlaying)
			storySoundSource.Play ();

	}

	public override void OnOutofFocus ()
	{
		base.OnOutofFocus ();
		SetOutline (false);
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

	/// <summary>
	/// called by SelectObjectManager when the object is selected
	/// return true when it is successfully selected
	/// should return false when it fails
	/// </summary>
	virtual public bool Select( ClickType clickType)
	{
		// set transform parent to the camera
		SelectObjectManager.AttachToCamera (transform, clickType);
		// set all the object to 'Hold' Layer
		gameObject.layer = LayerMask.NameToLayer ("Hold");
		foreach (Transform t in GetComponentsInChildren<Transform>())
			t.gameObject.layer = LayerMask.NameToLayer ("Hold");
		// play the sound effect
		if ( selectSoundSource != null)
			selectSoundSource.Play ();
		
		return true;
	}

	/// <summary>
	/// Called by SelectObjectManager when the object is unselected
	/// TODO: finish the unable to unselect situation
	/// </summary>
	/// <returns><c>true</c>, if select was uned, <c>false</c> otherwise.</returns>
	virtual public bool UnSelect( )
	{
		// play the sound effect
		if ( unselectSoundSource != null )
			unselectSoundSource.Play ();

		if ( storySoundSource != null && storySoundSource.isPlaying)
			storySoundSource.Stop ();

		// matched is set in HoleObject
		if (!matched) {
			// set all the object to 'Focus' Layer
			gameObject.layer = LayerMask.NameToLayer ("Focus");
			foreach (Transform t in GetComponentsInChildren<Transform>())
				t.gameObject.layer = LayerMask.NameToLayer ("Focus");

			transform.SetParent (originalParentTransform);
			//how do at the same time? how do rotate?
			transform.DOLocalMove (originalPos, 1f).SetEase (Ease.InCirc);
			//transform.DOLocalRotate (originalRot, 1f).SetEase (Ease.InCirc);
			transform.localRotation = Quaternion.identity;
			return true;
		} else if (matched) {
			//fires match object event on pressing trigger instead of unselect
			LogicArg logicArg = new LogicArg (this);
			logicArg.AddMessage(Global.EVENT_LOGIC_MATCH_COBJECT, this);
			M_Event.FireLogicEvent (LogicEvents.MatchObject, logicArg);
			Debug.Log (Time.timeSinceLevelLoad + "; MatchObject name: " + gameObject.name);
			//MetricManagerScript.instance.AddToMatchList (Time.timeSinceLevelLoad + "; MatchObject name: " + gameObject.name);
		}
		return false;

	}

	/// <summary>
	/// Called when the object fills in the hole
	/// </summary>
	virtual public void OnFill()
	{
		// repeat story once filled (could play both stories at this point)
		if ( storySoundSource != null && !storySoundSource.isPlaying)
			storySoundSource.Play ();
	}

}
