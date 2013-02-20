using UnityEngine;
using System.Collections;

public class MenuItemActions_5 : MonoBehaviour 
{
	private GameObject phoenix;
	private PhoenixCtrl phoenixCtrl;
	public Vector3 originRotation;
	
	void Start()
	{
		phoenix = GameObject.Find("Phoenix_FixY");
		phoenixCtrl = phoenix.GetComponent<PhoenixCtrl>();
		
		originRotation = phoenix.transform.eulerAngles;
	}
		
	void OnSelect(string command) 
	{
		Debug.Log("A Right Menu Command Received: " + command);	
		
		if(DefaultTrackableEventHandler.isTracked)
		{	
			switch(command)
			{
			case "Fly":
				phoenixCtrl.animator.SetBool("boolIsFly", true);
				break;
			case "Idle":
				phoenixCtrl.animator.SetBool("boolIsFly", false);
				break;
			case "Twitter":
				phoenixCtrl.audioSource.Play();
				break;
			case "Rotate":
				iTween.RotateTo(phoenix, iTween.Hash("x",originRotation.x  ,"y", originRotation.y, "z", originRotation.z, 
					"easeType", "easeInOutBack", "loopType", "none ", "delay", .2));
				break;
			}
		}
	}
	
}
