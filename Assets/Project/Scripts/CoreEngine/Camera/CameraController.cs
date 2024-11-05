using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Component")]

    [SerializeField] protected Camera _camera;

    [SerializeField] protected Transform _orientation;
    [SerializeField] protected Transform _cameraHolder;
    [SerializeField] protected float _speedTilt;

    [Header("Effect")]
    [SerializeField] protected float _strenghtShake;
    [SerializeField] protected int _vibrationShake;
    [SerializeField] protected float _timeShake;

    protected float _zTilt = 0;
    protected float _currentTilt = 0;
    protected float _defaultFov;

    protected void Construct()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _defaultFov = _camera.fieldOfView;
    }
    public void RotationCamera(float xRotation,float yRotation) 
    {
       
        _cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, _zTilt);
        
        _orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        _zTilt = Mathf.Lerp(_zTilt, _currentTilt, Time.deltaTime * _speedTilt);

    }

    public virtual void SetToDefault() 
    {
        DoFov(_defaultFov);
        DoTilt(0);
    }
    public virtual void DoFov(float endValue) 
    {
        _camera.DOFieldOfView(endValue,0.25f);

    }
    public virtual void DoTilt(float zTilt) 
    {
        _zTilt = zTilt;
        StopAllCoroutines();
        _currentTilt = zTilt;
        //StartCoroutine(SmoothlyLerpZtilt(zTilt));
    }

    public virtual void ShakeCamera() 
    {
        _camera.transform.localEulerAngles = Vector3.zero;
        _camera.DOShakeRotation(0.1f, _strenghtShake, _vibrationShake, 90, true, ShakeRandomnessMode.Full);
    }
    public virtual void ShakeCamera(float strenght) 
    {
        _camera.transform.localEulerAngles = Vector3.zero;
        _camera.DOShakeRotation(0.1f, strenght, _vibrationShake, 90, true, ShakeRandomnessMode.Full);
    }


    protected IEnumerator SmoothlyLerpZtilt(float zTilt)
    {
        float time = 0;
        float difference = Mathf.Abs(zTilt - _zTilt);
        float startValue = _zTilt;

        while (time < difference)
        {
            _zTilt = Mathf.Lerp(startValue, zTilt, time / difference);
            time += Time.deltaTime * 15;

            yield return null;
        }
        _zTilt = zTilt;
    }
}
