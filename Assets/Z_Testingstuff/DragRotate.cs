using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragRotate : MonoBehaviour
{

	
	private float _sensitivity;
	private Vector3 _mouseReference;
	private Vector3 _mouseOffset;
	private Vector3 _rotation;
	private bool _isRotating;
	public GameObject cube;
	public bool y;

	void Start()
	{
		_sensitivity = 0.4f;
		_rotation = Vector3.zero;
	}

	void Update()
	{
		if (_isRotating)
		{
			// offset
			_mouseOffset = (Input.mousePosition - _mouseReference);

			// apply rotation
			if (y)
			{
				_rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;
			}
            else
            {
				_rotation.z = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;
			}

			// rotate
			cube.transform.Rotate(_rotation);

			// store mouse
			_mouseReference = Input.mousePosition;
		}
	}

	void OnMouseDown()
	{
		// rotating flag
		_isRotating = true;

		// store mouse
		_mouseReference = Input.mousePosition;
	}

	void OnMouseUp()
	{
		// rotating flag
		_isRotating = false;
	}

}

