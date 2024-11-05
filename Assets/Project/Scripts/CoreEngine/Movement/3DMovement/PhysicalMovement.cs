using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PhysicalMovement : MonoBehaviour
{

    private readonly string OrientationName = "Orientation";
    #region Serialized variable
    [SerializeField] private Transform orientation;

    [Header("Parametrs ")]
    //то что будет вынесено в SO data
    [SerializeField] private LayerMask groudLayer;
    [SerializeField] private float modifireHeightGorundCheck = 1;

    [SerializeField] private float baseDrag = 5;


    [SerializeField] private float maxTouchGrass = 0.5f;//максимальное врем€ соприкосновени€ с землей до замедлени€ 
    private float _touchGrass = 0;




    [Header("Setting")]

    [SerializeField,Tooltip("Ѕазова€ высота")] 
    private float baseHeight = 2;
    [SerializeField, Tooltip("—ила дав€ща€ вниз при приседании")] 
    private float downForce = 25;//прижимающа€ сила 

    [SerializeField, Tooltip("ћаксимальна€ скорость в воздухе")] 
    private float maxAirSpeed = 36;//скорость дл€ падени€ 
    [SerializeField, Tooltip("ћаксимальна€ скорость по Y(¬ скольжении)")] 
    private float maxYSpeed = 25;//скорость дл€ сколжени€

    [SerializeField,Tooltip("максимальный угол на котором игрок стоит с трением, больше этого трение отсутвует")] 
    private float maxSlopeAngleWithFriction = 35;//максимальный угол на котором игрок стоит с трением, больше этого трение отсутвует
    [SerializeField, Tooltip("максимальный угол на который может зайти, перед тем как перейти именно в скольжение")] 
    private float maxAngleBeforeSlide = 40;//максимальный угол на который может зайти, перед тем как перейти именно в скольжение 

    [SerializeField, Tooltip("ћодификатор двжини€ в воздухе")]
    private float airMultiplier = 1;//можно перенатроить 


    #endregion

    #region private variable
    private Rigidbody _rb;

    private RaycastHit _slopeHit;


    private Vector3 _moveDirection;

    private float _currentMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private float _desireMoveSpeed;
    private float _speedChangeFactor;

    private bool _keepMomentum;
    private bool _touchGrassLong = true;
    private bool _exitingSlope;
    private bool _grounded;

    private float _moveSpeed = 10;

    private float _speedIncreaseMultiplier = 1.5f;
    private float _speedIncreaseMultiplierInSlow = 25;//ускоренное замедление , если распрыжка не нужна или закончилась 
    private float _speedIncreaseMultiplierInSlope = 2.5f;
    #endregion

    #region Getter Setter

    public bool KeepMomentum { get => _keepMomentum; }
    public bool ExitingSlope { get => _exitingSlope; }
    public float MoveSpeed { get => _moveSpeed;  }
    public Vector3 MoveDirection { get => _moveDirection; }
    public bool Grounded { get => _grounded; }
    public float GroundDrag { get => baseDrag;  }
    public Transform Orientation { get => orientation; }

    #endregion

    public void Construct() 
    {
        _rb ??= GetComponent<Rigidbody>();
        baseHeight = transform.localScale.y;
        GetOrientation();
        ChangeDrag(baseDrag);
    }
    //private void OnEnable()
    //{
    //    Construct();
    //}
    private void GetOrientation() 
    {
        Transform[] t_transformChild = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < t_transformChild.Length; i++)
        {
            if (t_transformChild[i].name == OrientationName)
            {
                orientation = t_transformChild[i];
                return;
            }
        }

        GameObject transformNew = new GameObject();
        orientation = Instantiate(transformNew, transform).transform;
        orientation.name = OrientationName;

        if (!orientation) 
        {
            Debug.LogWarning("ќбъекта Orientation нету ");
        }
    }
    #region SpeedControl
    private void SpeedControl()
    {
        //slopeCheck
        if (OnSlope() && !_exitingSlope)
        {
            if (_rb.velocity.magnitude > _moveSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * _moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            if (flatVel.magnitude > _moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * _moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }
        if (maxYSpeed != 0 && _rb.velocity.y > maxYSpeed)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, maxYSpeed, _rb.velocity.z);
        }

        //тут вызываетс€ проверка дл€ куратины 

        bool desiredMoveSpeedHasChanged = _desireMoveSpeed != _lastDesiredMoveSpeed;

        if (desiredMoveSpeedHasChanged)
        {
            if (_keepMomentum || (Mathf.Abs(_desireMoveSpeed - _lastDesiredMoveSpeed) > 4f && _moveSpeed != 0))
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                _moveSpeed = _desireMoveSpeed;
            }
        }

        _lastDesiredMoveSpeed = _desireMoveSpeed;


    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(_desireMoveSpeed - _currentMoveSpeed);
        float startValue = _currentMoveSpeed;

        float boostFactor = _speedChangeFactor;
        while (time < difference)
        {
            _currentMoveSpeed = Mathf.Lerp(startValue, _desireMoveSpeed, time / difference);

            if (OnSlope() || OnSlideSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * _speedIncreaseMultiplier * _speedIncreaseMultiplierInSlope * slopeAngleIncrease;
            }
            else
            {
                if (_keepMomentum)
                {
                    time += Time.deltaTime * boostFactor;
                }
                else
                {
                    if (!_touchGrassLong)
                    {
                        time += Time.deltaTime * _speedIncreaseMultiplier;
                    }
                    else
                    {
                        time += Time.deltaTime * _speedIncreaseMultiplierInSlow;
                    }
                }
            }

            yield return null;
        }
        _currentMoveSpeed = _desireMoveSpeed;
        _speedChangeFactor = 1f;
        _keepMomentum = false;
    }
    #endregion
    #region CharacterMovment public function

    #region Move Function
    private Vector3 direction = new Vector3();
    public void AddVelocityToOrientation(Vector3 value, bool useAirMultiplier = true) 
    {
        direction = orientation.forward * value.z + orientation.right * value.x;
        AddVelocity(direction, useAirMultiplier);
    }
    public void AddVelocityToOrientationImpulse(Vector3 value, bool useAirMultiplier = false)
    {
        direction = orientation.forward * value.z + orientation.right * value.x;
        AddVelocityImpulse(value, useAirMultiplier);
    }
    public void AddVelocity(Vector3 value, bool useAirMultiplier = true)
    {
        AddVelocity(value, ForceMode.Force, useAirMultiplier);
    }
    public void AddVelocityImpulse(Vector3 value, bool useAirMultiplier = false)
    {
        AddVelocity(value, ForceMode.Impulse, useAirMultiplier);
    }
    private void AddVelocity(Vector3 value, ForceMode forceMode, bool useAirMultiplier = false) 
    {
        if (!_grounded && useAirMultiplier)
        {
            value *= airMultiplier;
        }
        _rb.AddForce(value, forceMode);
        if ((OnSlideSlope() || OnSlope()) && !_exitingSlope)//slope
        {
            
            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 160, ForceMode.Force);
            }
           
        }
        if (!Grounded) 
        {
            if (_rb.velocity.y > maxAirSpeed)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, maxAirSpeed, _rb.velocity.z);
            }
        }
        SpeedControl();
    }

    #endregion

    #region Check Function 
    public bool GroundCheck()
    {
        return _grounded = Physics.Raycast(transform.position, Vector3.down, (baseHeight+modifireHeightGorundCheck)* 0.5f + 0.2f, groudLayer);
    }
    public void CheckTouchGrass() 
    {
        if (!_touchGrassLong) 
        {
            if (_touchGrass < maxTouchGrass)
            {
                _touchGrass += Time.deltaTime;
            }
            else
            {
                ChangeTouchGrass(true);
            }
        }
    }
    public void ResetTouchGrass() 
    {
        _touchGrass = 0;
        ChangeTouchGrass( false);
    }
    public void ChangeTouchGrass(bool value) 
    {
        _touchGrassLong = value;
    }
    #endregion

    #region Slope Function
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, baseHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngleWithFriction && angle != 0;
        }
        return false;
    }
    public bool OnSlideSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, baseHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            if (angle != 0)
            {
                if (angle > maxAngleBeforeSlide)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public Vector3 GetSlopeDirection()
    {
        return GetSlopeDirection(_moveDirection);
    }
    public Vector3 GetSlopeDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }
    public Vector3 GetDirectionSlopePlane()
    {
        OnSlope();
        Vector3 temp = Vector3.Cross(_slopeHit.normal, Vector3.down);
        Vector3 direction = Vector3.Cross(temp, _slopeHit.normal);
        return direction;
    }


    #endregion

    #region Change Fuction
    public void ChangeDrag(float value) 
    {
        _rb.drag = value;
    }
    public void ChangeToBaseDrag() 
    {
        _rb.drag = baseDrag;
    }
    public void ChangeSpeedFactory(float newSpeedFactory) 
    {
        _speedChangeFactor = newSpeedFactory;

    }
    public void ChangeMoveSpeed(float value) 
    {
        _moveSpeed = value;
    }
    public void ChangeDesireMoveSpeed(float newDesireSpeed,float lastDesireSpeed = 0,bool changeLastDesireSpeed = false) 
    {
        _desireMoveSpeed = newDesireSpeed;
        if (changeLastDesireSpeed) 
        {
            _lastDesiredMoveSpeed = lastDesireSpeed;
        }

    }
    #endregion

    #region Crouch
    public void ChangeHeight(float valueHeight)
    {
        transform.localScale = new Vector3(transform.localScale.x, valueHeight, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, transform.position.y - valueHeight / 2, transform.position.z);
        //_rb.AddForce(Vector3.down * downForce, ForceMode.Impulse);
    }
    public void ChangeToDefaultHeight()
    {
        transform.localScale = new Vector3(transform.localScale.x, baseHeight, transform.localScale.z);
    }
    #endregion

    #endregion
}
