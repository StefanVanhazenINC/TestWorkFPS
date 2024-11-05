using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class CameraControllerWhitCinemachine : CameraController
{
    [Header("CameraControllerWhitCinemachine")]
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private InputHandler _inputHandler;

    [SerializeField] private float _sensX;
    [SerializeField] private float _sensY;


    private Vector2 _lookDirection;


    private float _xRotation;
    private float _yRotation;


    private void Start()
    {
        Construct();
    }

    private void OnEnable()
    {
        _inputHandler.OnLookChange.AddListener(LookRead);
    }
    private void OnDisable()
    {
        _inputHandler.OnLookChange.RemoveListener(LookRead);
    }
    private void LateUpdate()
    {
        _xRotation += _lookDirection.x * _sensX;
        _yRotation -= _lookDirection.y * _sensY;
        _yRotation = Mathf.Clamp(_yRotation, -80f, 80f);
        RotationCamera(_yRotation, _xRotation);

    }
    private void LookRead(Vector2 value)
    {
        _lookDirection = value;
    }
  
    public override void ShakeCamera() // переопределение на CInemachine
    {
       
    }
    public override void ShakeCamera(float strenght)// переопределение на CInemachine
    {

    }
    
}
