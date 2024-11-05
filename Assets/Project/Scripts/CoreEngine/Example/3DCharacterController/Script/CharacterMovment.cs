using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovemntState
{
    Idle,
    Walking,
    Sprinting,
    Crouching,
    Jump,
    Air
}


public class CharacterMovment : MonoBehaviour
{
    private const float MoveEpsilon = 0.1f;

    [SerializeField] private PhysicalMovement _physicalMovement;

    [Header("Move")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _moveDrag;

    [Header("Sprint")]
    [SerializeField] private float _sprintSpeed;
    //мб стамина 


    [Header("Crouch")]
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _heightCrouch;
    [SerializeField] private float _crouchUpCheck;

    [Header("Jump")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _coolDownJump;


    private InputHandler _inputHandler;
    private Vector3 _moveDirection;
    private MovemntState _states;
    private MovemntState _lastStates;

    private bool _jumpInput;
    private bool _readyToJump;
    private bool _startJump;
    private float _lastTimeJump = -1000;

    private bool _sprintInput;
    
    private bool _crouchInput;
    private bool _startInputCrouch;

    private bool _enableMove;

    private void Awake()
    {
        _physicalMovement.Construct();
        Construct();
    }
    private void Construct()
    {
        _inputHandler ??= GetComponent<InputHandler>();
        InitStateMachine();
    }
    private void OnEnable()
    {
        _inputHandler.OnMoveChange.AddListener(ReadMove);
        _inputHandler.OnJumpChange.AddListener(ReadJump);
        _inputHandler.OnSprintChange.AddListener(ReadSprint);
        _inputHandler.OnCrouchChange.AddListener(ReadCrouch);
    }
    private void ReadMove(Vector2 value)
    {
        _moveDirection = new Vector3(value.x, 0, value.y);
    }
    private void ReadJump(bool value)
    {
        _jumpInput = value;
        if (_jumpInput && _readyToJump && _physicalMovement.Grounded )
        {
            if (_states != MovemntState.Crouching && _states != MovemntState.Jump) 
            {
                _startJump = true;
            }
        }
    }
    private void ResetJump() 
    {
        if (!_readyToJump) 
        {
            if (Time.time>= _lastTimeJump + _coolDownJump) 
            {
                _readyToJump = true;
            }
        }
    }
    private void ReadCrouch(bool value) 
    {
        _crouchInput = value;
        if (_crouchInput && !_startInputCrouch)
        {
            _startInputCrouch = true;

        }
        else if (!_crouchInput)
        {
            _startInputCrouch = false;

        }
    }
    private void ReadSprint(bool value)
    {
        _sprintInput = value;
    }

    

  
    private void FixedUpdate()
    {
        if (_enableMove) 
        {
            _physicalMovement.AddVelocityToOrientation(_moveDirection * _moveSpeed * 10);
        }
        _physicalMovement.GroundCheck();
    }

    private void Update()
    {
        StateHandler();
        ResetJump();
    }

    private void InitStateMachine()
    {
        _states = MovemntState.Idle;
        StateHandler();
    }
    private void StateHandler() 
    {

        if (_physicalMovement.Grounded )
        {
           
            if (_sprintInput &&( _states != MovemntState.Crouching || _states != MovemntState.Air || _states != MovemntState.Jump))
            {
                SwitchState(MovemntState.Sprinting);
            }
            else if (_startJump && _states != MovemntState.Crouching)
            {
                SwitchState(MovemntState.Jump);
            }
            else if (_startInputCrouch && !_crouchInput)
            {
                _startInputCrouch = false;
                SwitchState(MovemntState.Walking);
            }
            else if (_crouchInput && _startInputCrouch)//доп провекра коллайдера сверху
            {
                //_startInputCrouch = true;
                SwitchState(MovemntState.Crouching);
            }
            else if (_moveDirection.magnitude > MoveEpsilon)
            {
                SwitchState(MovemntState.Walking);
            }
            else if (_moveDirection.magnitude < MoveEpsilon)
            {
                SwitchState(MovemntState.Idle);
            }
        }
        else if (!_physicalMovement.Grounded)
        {
            SwitchState(MovemntState.Air);
        }


        switch (_states)
        {
            case MovemntState.Idle:
                if (_states != _lastStates) //новый вход в состояние
                {
                    _enableMove = false;
                    _lastStates = MovemntState.Idle;
                    _physicalMovement.ChangeToDefaultHeight();
                    _physicalMovement.ChangeToBaseDrag();
                }

                break;
            case MovemntState.Walking:
                if (_states != _lastStates) //новый вход в состояние
                {
                    _enableMove = true;
                    _lastStates = MovemntState.Walking;
                    _physicalMovement.ChangeToDefaultHeight();
                    _physicalMovement.ChangeDrag(_moveDrag);
                    _physicalMovement.ChangeDesireMoveSpeed(_moveSpeed);
                }
                break;
            case MovemntState.Sprinting:
                if (_states != _lastStates) //новый вход в состояние
                {
                    _enableMove = true;

                    _lastStates = MovemntState.Sprinting;
                    _physicalMovement.ChangeToDefaultHeight();
                    _physicalMovement.ChangeDrag(_moveDrag);
                    _physicalMovement.ChangeDesireMoveSpeed(_sprintSpeed);
                }
                break;
            case MovemntState.Crouching:
                if (_states != _lastStates) //новый вход в состояние
                {
                    _enableMove = true;

                    _lastStates = MovemntState.Crouching;
                    _physicalMovement.ChangeDrag(_moveDrag);
                    _physicalMovement.ChangeDesireMoveSpeed(_crouchSpeed);
                    _physicalMovement.ChangeHeight(_heightCrouch);
                }
                break;
            case MovemntState.Jump:
                if (_states != _lastStates) //новый вход в состояние
                {
                    Debug.Log("Jump");
                    _enableMove = true;
                    _startJump = false;
                    _lastTimeJump = Time.time;
                    _readyToJump = false;
                    _physicalMovement.AddVelocityImpulse(transform.up * _jumpForce);
                    _lastStates = MovemntState.Jump;
                    _physicalMovement.ChangeToDefaultHeight();
                    _physicalMovement.ChangeDrag(0);
                    SwitchState(MovemntState.Air);
                }
                break;
            case MovemntState.Air:
                if (_states != _lastStates) //новый вход в состояние
                {
                    _enableMove = true;
                    _physicalMovement.ChangeDrag(0);
                    _lastStates = MovemntState.Air;
                    _physicalMovement.ChangeToDefaultHeight();
                }
                break;
        }
    }
    private void LerpCrouchHeight() 
    {
    
    }
    private void SwitchState(MovemntState newState)
    {
        _states = newState;
    }
}


