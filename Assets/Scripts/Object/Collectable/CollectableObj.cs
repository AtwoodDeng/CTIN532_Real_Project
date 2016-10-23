﻿using UnityEngine;
using System.Collections;

/// <summary>
/// The object that can be select and hold by the player
/// </summary>
public class CollectableObj : MObject {
	
	[SerializeField] MeshRenderer[] outlineRenders;
	[SerializeField] protected AudioClip selectSound;
	private AudioSource selectSoundSource;
	[SerializeField] protected AudioClip unselectSound;
	private AudioSource unselectSoundSource;


	protected override void MAwake ()
	{
		base.MAwake ();
		// turn off the outline 
		SetOutline (false);

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
	/// return turn when it is successfully selected
	/// return false when it fails
	/// TODO: finish the unselectable situation
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
		if ( selectSoundSource != null )
			selectSoundSource.Play ();
		
		return true;
	}

	/// <summary>
	/// Called by SelectObjectManager when the object is unselected
	/// TODO: finish the unable to unselect situation
	/// </summary>
	/// <returns><c>true</c>, if select was uned, <c>false</c> otherwise.</returns>
	virtual public bool UnSelect()
	{
		// play the sound effect
		if ( unselectSoundSource != null )
			unselectSoundSource.Play ();
		return true;
	}

}
