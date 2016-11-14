﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryObjManagerCharacters : MBehavior {

	private int count = 0;
	[SerializeField] List<GameObject> storyObjA;
	[SerializeField] List<GameObject> storyObjB;
	[SerializeField] List<GameObject> storyObjC;
	[SerializeField] List<GameObject> levelSpecificObjects;

	private List<GameObject> currentStory = new List<GameObject>();

	protected override void MAwake ()
	{
		base.MAwake ();
		currentStory = storyObjA;

		if (levelSpecificObjects.Count > 0) {
			for (int i = 0; i < levelSpecificObjects.Count; i++) {
				levelSpecificObjects [i].SetActive (false);
			}
		}
	}

	protected override void MOnEnable(){

		base.MOnEnable ();
		M_Event.logicEvents [(int)LogicEvents.EnterStory] += OnEnterStory;
		M_Event.logicEvents [(int)LogicEvents.ExitStory] += OnExitStory;
		M_Event.logicEvents [(int)LogicEvents.Characters] += OnCharacters;
        M_Event.logicEvents [(int)LogicEvents.End] += OnEnd;
        M_Event.logicEvents[ ( int )LogicEvents.Credits ] += OnCredits;
    }

	protected override void MOnDisable(){

		base.MOnDisable ();
		M_Event.logicEvents [(int)LogicEvents.EnterStory] -= OnEnterStory;
		M_Event.logicEvents [(int)LogicEvents.ExitStory] -= OnExitStory;
		M_Event.logicEvents [(int)LogicEvents.Characters] -= OnCharacters;
        M_Event.logicEvents [(int)LogicEvents.End] -= OnEnd;
        M_Event.logicEvents[ ( int )LogicEvents.Credits ] += OnCredits;
    }

	void OnEnterStory(LogicArg arg){
		
		currentStory.Clear ();
		currentStory = GetStory ();
		if (currentStory != null) {
			for (int i = 0; i < currentStory.Count; i++) {
				currentStory [i].SetActive (true);
			}
		}
	}

	//exit last story before entering new one 
	void OnExitStory(LogicArg arg){
		//disable remaining objects in mother
		//Debug.Log("length of current story obj = " + currentStory.Count);
		for (int i=currentStory.Count-1; i>=0; i--) {
			if (currentStory [i].layer == 16) { // Focus is layer 16
                Debug.Log( "in StoryManCharacters exit story deactivating " + currentStory[ i ].name );
				currentStory [i].SetActive (false);
			}
		}

		//iterate count and enter next story upon exiting last one 
		count++;
		if (GetStory () != null) {
            Debug.Log( "in StoryManCharacters enter next story " );
            LogicArg logicArg = new LogicArg (this);
			M_Event.FireLogicEvent (LogicEvents.EnterStory, logicArg);
		} 
	}

	//returns the next batch of story obj
	List<GameObject> GetStory(){
		switch (count) {
		case 0:
			if (storyObjA.Count > 0) {
				return storyObjA;
			} else {
				return null;
			}
		case 1:
			if (storyObjB.Count > 0) {
				return storyObjB;
			} else {
				return null;
			}
		case 2:
			if (storyObjC.Count > 0) {
                LogicArg logicArg = new LogicArg( this );
                M_Event.FireLogicEvent( LogicEvents.Finale, logicArg );
                return storyObjC;
			} else {
				return null;
			}
		default:
			return null;
		}
	}

	void OnCharacters( LogicArg arg ){
		for (int i = 0; i < levelSpecificObjects.Count; i++) {
			levelSpecificObjects [i].SetActive (true);
		}
	}
		
	void OnEnd( LogicArg arg ){
		for (int i = levelSpecificObjects.Count - 1; i >=0; i--) {
            levelSpecificObjects [i].SetActive (false);
		}
	}

    void OnCredits( LogicArg arg )
    {
        //disable the trails
        for ( int i = currentStory.Count - 1; i >= 0; i-- )
        {
            currentStory[ i ].SetActive( false );
        }
    }

    		
}