using UnityEngine;
using System.Collections;

public class MenuItemActions_5 : MonoBehaviour 
{
	private GameObject phoenix;
	private PhoenixCtrl phoenixCtrl;
	//private Animator phoenixAnimator;
	
	void Start()
	{
	//	phoenix = GameObject.Find("Phoenix_FixY");
		//phoenixAnimator = GameObject.Find("Phoenix_FixY").GetComponent<Animator>();
		phoenixCtrl = GameObject.Find("Phoenix_FixY").GetComponent<PhoenixCtrl>();
		//if(phoenixCtrl)
		//{
		//	Debug.Log("haha");
		//}
	}
		
	void OnSelect(string command) 
	{
		Debug.Log("A Right Menu Command Received: " + command);	
		//phoenixCtrl = GameObject.Find("Phoenix_FixY").GetComponent<PhoenixCtrl>();
		
		switch(command)
		{
		case "Fly":
			phoenixCtrl.animator.SetBool("boolIsFly", true);
			break;
		case "Idle":
			phoenixCtrl.animator.SetBool("boolIsFly", false);
			break;
		case "Twitter":
			
			//if(phoenixCtrl.enabled)
			//{
			phoenixCtrl.audioSource.Play();
			//}
			break;
		}
	}
	
}
