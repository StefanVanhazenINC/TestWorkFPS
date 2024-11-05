using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController : InputHandler
{
    #region private variable
    private Vector2 _look = new Vector2();
    private Vector2 _move = new Vector2();
    private bool _isJump = false;
    private bool _isCrouch = false;
    private bool _isSprint = false;
    private bool _isFirstInteract = false;
    #endregion

    #region InputAction

    #endregion

    #region Unity Callback

    #endregion

    #region CharacterController private function

    private void Update()
    {
        OnLook();
        OnMove();
        OnSprint();
        OnInteract();
        OnFirstIntreract();
      
    }
    private void OnLook() 
    {
        _look.x = Input.GetAxisRaw("Mouse X");
        _look.y = Input.GetAxisRaw("Mouse Y");
        OnLookChange?.Invoke(_look);
    }
    private void OnMove() 
    {
        _move.x = Input.GetAxisRaw("Horizontal");
        _move.y = Input.GetAxisRaw("Vertical");
        
        OnMoveChange?.Invoke(_move);
    }
    private void OnSprint() 
    {
        _isSprint = Input.GetButton("Sprint");
        OnSprintChange?.Invoke(_isSprint);
    }
    private void OnCrouch() 
    {
        _isCrouch = Input.GetButton("Crouch");
        OnCrouchChange?.Invoke(_isCrouch);
    }
    private void OnJump()
    {
        _isJump = Input.GetButton("Jump");
        OnJumpChange?.Invoke(_isJump);  
    }
    private void OnInteract() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnInteractChange?.Invoke(true);
        }
        else if (Input.GetMouseButtonUp(0)) 
        {
            OnInteractChange?.Invoke(false);
        }
    }
    private void OnFirstIntreract() 
    {
        _isFirstInteract = Input.GetKey(KeyCode.Tab);
        OnFirstIntereactionChange?.Invoke(_isFirstInteract);
    }
    #endregion
}
