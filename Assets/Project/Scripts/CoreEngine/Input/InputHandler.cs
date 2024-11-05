using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public UnityEvent<Vector2> OnMoveChange;
    [HideInInspector] public UnityEvent<Vector2> OnLookChange;
    [HideInInspector] public UnityEvent<bool> OnJumpChange;
    [HideInInspector] public UnityEvent<bool> OnSlideChange;
    [HideInInspector] public UnityEvent<bool> OnDashChange;
    [HideInInspector] public UnityEvent<bool> OnCrouchChange;
    [HideInInspector] public UnityEvent<bool> OnSprintChange;
    [HideInInspector] public UnityEvent<bool> OnFirstIntereactionChange;
    [HideInInspector] public UnityEvent<bool> OnInteractChange;

}
