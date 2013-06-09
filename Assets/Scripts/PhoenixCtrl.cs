using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class PhoenixCtrl : MonoBehaviour {
	
	public Animator animator;
	public AudioSource audioSource;
	public bool isFly = false;

	// Use this for initialization
	void Start () {
		
		animator = GetComponent<Animator>();
		animator.SetBool("boolIsFly", isFly);
	
	}
}
