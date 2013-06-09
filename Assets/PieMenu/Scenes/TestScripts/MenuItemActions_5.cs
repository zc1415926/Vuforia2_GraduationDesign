using UnityEngine;
using System.Collections;

public class MenuItemActions_5 : MonoBehaviour 
{
	//private GameObject phoenix;
	//private GameObject chicken;
	public GameObject chicken;
	//private PhoenixCtrl phoenixCtrl;
	private ChickenCtrl chickenCtrl;
	public Vector3 originRotation;
	
	void Start()
	{
		//phoenix = GameObject.Find("Phoenix_FixY");
		//phoenixCtrl = phoenix.GetComponent<PhoenixCtrl>();
		
		//originRotation = phoenix.transform.eulerAngles;
		
		
		//chicken = GameObject.Find("Chicken(try)_13");
		//phoenixCtrl = phoenix.GetComponent<PhoenixCtrl>();
		chickenCtrl = chicken.GetComponent<ChickenCtrl>();
		originRotation = chicken.transform.eulerAngles;
	}
		
	void OnSelect(string command) 
	{
		Debug.Log("A Right Menu Command Received: " + command);	
		
		if(DefaultTrackableEventHandler.isTracked)
		{	
			switch(command)
			{
			case "Jump":
				chickenCtrl.animator.SetBool("boolIsJump", true);
				break;
			case "Walk":
				chickenCtrl.animator.SetBool("boolIsJump", false);
				break;
			case "Rooster":
				chickenCtrl.audioSource.Play();
				break;
			case "Rotate":
				iTween.RotateTo(chicken, iTween.Hash("x",originRotation.x  ,"y", originRotation.y, "z", originRotation.z, 
					"easeType", "easeInOutBack", "loopType", "none ", "delay", .2));
				break;
			}
		}
	}
	
}
