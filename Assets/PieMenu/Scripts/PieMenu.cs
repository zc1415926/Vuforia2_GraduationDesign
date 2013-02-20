using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PieMenu : MonoBehaviour
{
	public Camera guiCamera;
	
	public List<string> commands;
	public List<Texture> icons;
	
	public float iconSize = 64f;
	public float spacing = 12f;
	public float speed = 8f;
	public GUISkin skin;
	
	[HideInInspector]
	public float scale;
	[HideInInspector]
	public float angle;
	[HideInInspector]
	public PieMenuManager manager;
	
	void Awake ()
	{
		manager = PieMenuManager.Instance;
	}
	
	void OnMouseUp() 
	{	
		if(manager.getShown())
		{
			manager.Hide(this);
			manager.setShown(false);
		}
		else
		{
			manager.Show(this);
			manager.setShown(true);
		}
	}
}