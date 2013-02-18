using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class PhoenixCtrl : MonoBehaviour {
	
	protected Animator animator;

	// Use this for initialization
	void Start () {
		
		animator = GetComponent<Animator>();
		animator.SetBool("boolIsFly", false);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
}
