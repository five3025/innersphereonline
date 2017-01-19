﻿using System;
using UnityEngine;
using Zenject;

public class CameraHandler : ITickable {

	[Inject]
	readonly Settings _settings;

	private float _cameraDistance = 0f;
	private bool _gliding = false;
	private Vector3 _startMarker;
    private Vector3 _endMarker;
    private float _glideSpeed = 0F;
    private float _startTime;
    private float _journeyLength;
	
	readonly Camera _camera;

	// Allow camera movement to be enabled and disabled
	public bool CameraMovementEnabled = false;

	public CameraHandler( [Inject(Id = "MainCamera")] Camera camera )
	{
		_camera = camera;
	}

	public void StartGlide ( Vector3 destination )
	{
		_gliding = true;
		_startTime = Time.time;
		_startMarker = _camera.transform.position;
		_endMarker = destination;
        _journeyLength = Vector3.Distance(_startMarker, _endMarker);
		_glideSpeed = _journeyLength / _settings.GlideSpeedBase;
	}

	// TODO: Abstract away direct references to Input
	public void Tick () 
	{
		if ( !CameraMovementEnabled ) return;

		if ( _gliding )
		{
			float distCovered = (Time.time - _startTime) * _glideSpeed;
        	float fracJourney = distCovered / _journeyLength;
        	_camera.transform.position = Vector3.Lerp(_startMarker, _endMarker, Mathf.SmoothStep(0.0F, 1.0F, fracJourney));
			
			if (_camera.transform.position == _endMarker) _gliding = false;

			return;
		}

		float speed = 80.0F;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			speed *= 3;

		float moveV = Input.GetAxis("Vertical") * speed * Time.deltaTime;
		float moveH = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
		_camera.transform.Translate(_camera.transform.InverseTransformVector(new Vector3(moveH, 0, moveV)));

		float scrolling = Input.GetAxis("Mouse ScrollWheel");
		if ( scrolling != 0 )
		{
			float cameraChange = scrolling * _settings.PanSpeed;
			_cameraDistance += cameraChange;
			if ( _cameraDistance < _settings.CameraDistanceMax && _cameraDistance > _settings.CameraDistanceMin )
				_camera.transform.Translate(new Vector3(0, 0, cameraChange));
			else _cameraDistance -= cameraChange;
		}
	}

	[Serializable]
	public class Settings
	{
		public float CameraDistanceMax = 50f;
		public float CameraDistanceMin = -5000f;
		public float PanSpeed = 300f;
		public float GlideSpeedBase = 1.3F;
	}
	
}