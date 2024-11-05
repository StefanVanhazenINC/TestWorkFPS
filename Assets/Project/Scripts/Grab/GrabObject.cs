using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GrabObject : MonoBehaviour
{
    [SerializeField] private Transform _pointGrab;
    [SerializeField] private Transform _pointGrabSearch;
    [SerializeField] private float _distanceGrab;
    [SerializeField] private LayerMask _grabLayer;

    private bool _isGrabing;
    private Rigidbody _object;

    private void Start()
    {
        InputHandler inputHandler = GetComponent<InputHandler>();
        inputHandler.OnInteractChange.AddListener(GrabCheck);
    }
    private void GrabCheck(bool value) 
    {
        if (value && !_isGrabing)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(_pointGrabSearch.position, _pointGrabSearch.forward, out hitInfo, _distanceGrab, _grabLayer))
            {
                _object = hitInfo.collider.GetComponent<Rigidbody>();
                _object.GetComponent<Collider>().enabled = false;
                _isGrabing = true;
                _object.transform.SetParent(_pointGrab);
                _object.transform.localPosition = Vector3.zero;
                _object.transform.localEulerAngles = Vector3.zero;
                _object.useGravity = false;
                _object.isKinematic = true;

            }
        }
        else if (!value)
        {
            if (_isGrabing)
            {
                _object.isKinematic = false;
                _object.GetComponent<Collider>().enabled = true;

                _isGrabing = false;
                _object.transform.SetParent(null);
                _object.useGravity = true;
                _object = null;
            }
        }
    }
   


}
