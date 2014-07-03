﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using terasurware;



public class ReferenceAllObjectWindow : EditorWindow
{
	[MenuItem("Window/Referenced/All")]
	static void Init ()
	{
		var window = GetWindow (typeof(ReferenceAllObjectWindow));
		window.title = "all";
		window.Show ();
	}

	List<ReferenceObject> refObjectList = new List<ReferenceObject> ();
	Vector2 current = new Vector2 ();

	void OnInspectorUpdate ()
	{
		Repaint ();
	}

	List<GameObject> allObject;
	bool isHiding = false;

	void ParentShow (Transform parent)
	{
		if (parent != null) {
			parent.gameObject.hideFlags = HideFlags.None;
			ParentShow (parent.parent);
		}
	}

	void OnDestroy ()
	{
		ShowAllObject ();
	}

	void HideNoCommunication ()
	{
		isHiding = true;


		UpdateAllObject ();
		UpdateList ();
		foreach (var obj in allObject) {
			obj.hideFlags = HideFlags.HideInHierarchy;
		}
		
		foreach (var item in refObjectList) {
			ParentShow (item.rootComponent.transform);
			if( item.value is GameObject)
				ParentShow (((GameObject) item.value).transform);
			if( item.value is Component)
				ParentShow (((Component) item.value).transform);

		}
	}
	
	void ShowAllObject ()
	{
		isHiding = false;

		UpdateAllObject ();

		foreach (var obj in allObject) {
			obj.hideFlags = HideFlags.None;
		}
	}

	void Update ()
	{

		if (EditorApplication.isPaused || !EditorApplication.isPlaying) {
			UpdateAllObject ();
			UpdateList ();
		}
	}

	void UpdateAllObject ()
	{
		allObject = SceneObjectUtility.GetAllObjectsInScene (false);
	}

	void UpdateList ()
	{
		refObjectList.Clear ();

		
		foreach (var obj in allObject) {
			SceneObjectUtility.GetReferenceObject (obj, refObjectList);
		}
		refObjectList.Sort ((x,y) => {
			return x.rootComponent.GetInstanceID () - y.rootComponent.GetInstanceID (); });
	}
	
	void OnGUI ()
	{	

		if (EditorApplication.isPlaying && !EditorApplication.isPaused) {
			if (GUILayout.Button ("pause")) {
				EditorApplication.isPaused = true;
			}
			return;
		}

		if (isHiding == false && GUILayout.Button ("hide")) {
			HideNoCommunication ();
		}

		if (isHiding == true && GUILayout.Button ("show")) {
			ShowAllObject ();
		}

		GUIStyle styles = new GUIStyle ();
		styles.margin.left = 10;
		styles.margin.top = 5;
		
		current = EditorGUILayout.BeginScrollView (current);
		
		int preGameObjectID = 0;
		
		
		try {
			
			foreach (var referenceObject in refObjectList) {
				GameObject rootObject = referenceObject.rootComponent.gameObject;
				GameObject targetObject = null;
				
				if (referenceObject.value is Component)
					targetObject = ((Component)referenceObject.value).gameObject;
				if (referenceObject.value is GameObject)
					targetObject = (GameObject)referenceObject.value;
				
				if (preGameObjectID != rootObject.GetInstanceID ()) {
					preGameObjectID = rootObject.GetInstanceID ();
					EditorGUILayout.Space ();
					EditorGUILayout.ObjectField (referenceObject.rootComponent.gameObject , referenceObject.GetType());
				}
				
				string msg = string.Format ("{2}.{1} -> ({0}) {3}", 
				                            referenceObject.value.GetType ().Name, 
				                            referenceObject.memberName, 
				                            referenceObject.rootComponent.GetType ().Name,
				                            targetObject.name);
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label (EditorGUIUtility.ObjectContent (null, typeof(ReferenceObject)).image, GUILayout.Height (16), GUILayout.Width (16));
				GUILayout.Label (msg);
				GUILayout.EndHorizontal ();
			}
		} catch {
			refObjectList.Clear ();
		}

		EditorGUILayout.EndScrollView ();
	}
}
