using UnityEngine;
using System.Collections;

public class TouchMove_Append : MonoBehaviour {
	
	//最大速度
	public float rotationMaxSpeed = 20.0F;
	//阻力系数
	public float _friction = 0.96f;	
	
	//触控坐标变化向量
	private Vector2 _touchDeltaPosition = new Vector2(0.0f,0.0f);
	//触控接触时间
	private float _touchDeltaTime = 0.0f;
	//代码所依附物体沿X轴旋转速度
	private float _speedX = 0.0f;
	//代码所依附物体沿X轴旋转速度
	private float _speedY = 0.0f;
	//Mathf.Abs(_speedX)
	private float _absSpeedX = 0.0f;
	//Mathf.Abs(_speedY)
	private float _absSpeedY = 0.0f;
	//用变量保存gameObject.transform，减少GetComponent的调用，提高效率
	private Transform _gameObjectTransform;
	
	void Awake()
	{
		_gameObjectTransform = transform;
	}
	
	void Update () {
	//	Debug.Log("MobileButtons.pause: " + MobileButtons.pause);
//		if((GameController.pause != null) & !GameController.pause)
//		{
			if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved){
				
				_touchDeltaPosition = Input.GetTouch(0).deltaPosition;
				_touchDeltaTime = Input.GetTouch(0).deltaTime;
				
				if(_touchDeltaTime!=0)
				{
					//TODO:根据DeviceOrientation调整_speedX和_speedY的公式
					//Cube旋转的初速度：v = L/t
					_speedX = _touchDeltaPosition.y / _touchDeltaTime * 0.01f;
					_speedY = -_touchDeltaPosition.x / _touchDeltaTime * 0.01f;
					
					_absSpeedX = Mathf.Abs(_speedX);
					_absSpeedY = Mathf.Abs(_speedY);
					
					if(_absSpeedX >= rotationMaxSpeed)
					{
						/* 一个变量和自己的绝对值相除可取得它是正还是负，_speedX是正数是商为1，
						 * _speedX是负数是商为-1，再和rotationMaxSpeed(最大速度)相乘，可使
						 * _speedX和符号和rotationMaxSpeed的符号相同，使速度方向保持一致	*/
						_speedX = rotationMaxSpeed * _speedX / _absSpeedX;
					}
					
					if(_absSpeedY >= rotationMaxSpeed)
					{
						_speedY = rotationMaxSpeed * _speedY / _absSpeedY;
					}
				}
			}
			
			//速度乘以阻力系数使旋转速度不断变小
			_speedX *= _friction;
			_speedY *= _friction;
			
			//实施旋转
			_gameObjectTransform.Rotate(_speedX, 0, _speedY, Space.World);
			//_gameObjectTransform.Rotate(0, 0, _speedY, Space.World);
//		}
	}
}