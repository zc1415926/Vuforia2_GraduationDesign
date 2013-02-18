using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class PieMenuManager : MonoBehaviour
{

	private List<PieMenu> display = new List<PieMenu> ();
	private static List<Matrix4x4> stack = new List<Matrix4x4> ();
	private static PieMenuManager _instance = null;

	public static PieMenuManager Instance {
		get {
			if (_instance != null)
				return _instance;
			var g = GameObject.Find ("/PieMenuManager");
			if (g == null) {
				g = new GameObject (typeof(PieMenuManager).ToString ());
				_instance = g.AddComponent<PieMenuManager> ();
				DontDestroyOnLoad (g);
			} else {
				_instance = g.GetComponent<PieMenuManager> ();
			}
			return _instance;
		}
	}

	public void Show (PieMenu menu)
	{
		if (display.Contains (menu))
			return;
		foreach (PieMenu m in display) {
			StartCoroutine (_Hide (m));
		}
		display.Add (menu);
		StartCoroutine (_Show (menu));
	}

	public void Hide (PieMenu menu)
	{
		StartCoroutine (_Hide (menu));
	}

	private IEnumerator _Hide (PieMenu menu)
	{
		while (menu.scale > 0) {
			yield return new WaitForEndOfFrame ();
			menu.scale -= Time.deltaTime * menu.speed;
			menu.angle = (1 - menu.scale) * 30f;
		}
		menu.scale = 0f;
		menu.angle = 0f;
		display.Remove (menu);
	}

	private IEnumerator _Show (PieMenu menu)
	{
		while (menu.scale < 1) {
			yield return new WaitForEndOfFrame ();
			menu.scale += Time.deltaTime * menu.speed;
			menu.angle = (1 - menu.scale) * 30f;
		}
		menu.scale = 1f;
		menu.angle = 0f;
	}

	void OnGUI ()
	{
		foreach (PieMenu menu in display.ToArray ())
			DrawMenu (menu);
	}

	private void DrawMenu (PieMenu menu)
	{
		if (menu.scale <= 0)
			return;
		
		PushGUI ();
		//Vector3 origin = Camera.main.WorldToScreenPoint (menu.transform.position);
		Vector3 origin = menu.guiCamera.WorldToScreenPoint(menu.transform.position);
		
		
		
		
		TranslateGUI (origin.x, Screen.height - origin.y);
		ScaleGUI (menu.scale);
		RotateGUI (menu.angle);
		float d = (2 * Mathf.PI) / menu.icons.Count;
		float radius = (menu.spacing * menu.icons.Count);
		if (menu.skin != null)
			GUI.skin = menu.skin;
		for (var i = 0; i < menu.icons.Count; i++) {
			float theta = (d * i);
			float ix = (Mathf.Cos (theta) * radius) - (menu.iconSize / 2);
			float iy = (Mathf.Sin (theta) * radius) - (menu.iconSize / 2);
			if (GUI.Button (new Rect (ix, iy, menu.iconSize, menu.iconSize), menu.icons[i])) {
				StartCoroutine (_Hide (menu));
				string cmd = "PieMenuCommandIsMissing";
				try {
					cmd = menu.commands[i];
				} catch(ArgumentOutOfRangeException) {
					Debug.LogWarning("PieMenu commands have not been correctly set up.");
				}
				
				menu.gameObject.SendMessage ("OnSelect", cmd, SendMessageOptions.DontRequireReceiver);
			}
		}
		PopGUI ();
		
	}

	private static void PushGUI ()
	{
		stack.Add (GUI.matrix);
	}

	private static void PopGUI ()
	{
		GUI.matrix = stack[stack.Count - 1];
		stack.RemoveAt (stack.Count - 1);
	}

	private static void TranslateGUI (float x, float y)
	{
		Matrix4x4 m = new Matrix4x4 ();
		m.SetTRS (new Vector3 (x, y, 0), Quaternion.identity, Vector3.one);
		GUI.matrix *= m;
	}

	private static void RotateGUI (float a)
	{
		Matrix4x4 m = new Matrix4x4 ();
		m.SetTRS (Vector3.zero, Quaternion.Euler (0, 0, a), Vector3.one);
		GUI.matrix *= m;
	}

	private static void ScaleGUI (float s)
	{
		Matrix4x4 m = new Matrix4x4 ();
		m.SetTRS (Vector3.zero, Quaternion.identity, Vector3.one * s);
		GUI.matrix *= m;
	}
	
}
