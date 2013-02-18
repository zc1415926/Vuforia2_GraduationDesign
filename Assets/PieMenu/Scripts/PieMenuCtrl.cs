using UnityEngine;
using System.Collections;

public class PieMenuCtrl : MonoBehaviour {

	public Material OnMouseOverMat;
	public Material OnMouseExitMat;
	
	
	private MeshRenderer meshRenderer;
	
	void Start () 
	{
		meshRenderer = gameObject.GetComponent<MeshRenderer>();
		meshRenderer.material = OnMouseExitMat;
	}
	
	void OnMouseDown()
	{
		//meshRenderer = gameObject.GetComponent<MeshRenderer>();
		meshRenderer.material = OnMouseOverMat;
	}
	
	void OnMouseUpAsButton()
	{
		//meshRenderer = gameObject.GetComponent<MeshRenderer>();
		meshRenderer.material = OnMouseExitMat;
	}
	
	// Update is called once per frame
	//void Update () {
	
	//}
}
