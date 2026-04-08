using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AnimationMapping
{
    public PlayerAction action;
    public string animatorStateName;
}

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 2f;
    public float baseMoveSpeed = 5f;
    private float moveSpeed = 0f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool inAction = false;

    [Header("Animations")]
    public Animator animator;
    private string currentAnimation;
    public string[] idleAnimations = { "Idle_Aggro", "Idle_Aggro", "Idle_Breaker" };
    private Coroutine idleCoroutine;

    public AnimationMapping[] animationSetup;
    private Dictionary<PlayerAction, string> animationDict = new Dictionary<PlayerAction, string>();

    void Awake()
    {
        //animationSetup = new AnimationMapping[]
        //{
        //    new AnimationMapping { action = PlayerAction.Mine, animatorStateName = "Mine" },
        //    new AnimationMapping { action = PlayerAction.Collect, animatorStateName = "Collect" },
        //    new AnimationMapping { action = PlayerAction.Purify, animatorStateName = "Purify" },
        //    new AnimationMapping { action = PlayerAction.Drill, animatorStateName = "Drill" },
        //    new AnimationMapping { action = PlayerAction.Pump, animatorStateName = "Pump" }
        //};

        foreach (AnimationMapping map in animationSetup)
        {
            if (!animationDict.ContainsKey(map.action))
            {
                animationDict.Add(map.action, map.animatorStateName);
            }
            else
            {
                Debug.LogWarning($"You have assigned the action {map.action} in your Player Inspector!");
            }
        }
    }

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        targetPosition = transform.position;

        idleCoroutine = StartCoroutine(IdleRandomize());
    }

    void Update()
    {
        if (isMoving)
        {
            MoveTowardTarget();
        }
    }

    public void ChangeAnimation(string newState, float transitionTime = 0.1f, bool forceRestart = false)
    {
        if (currentAnimation == newState && !forceRestart) return;

        if (forceRestart)
        {
            animator.CrossFade(newState, transitionTime, 0, 0f);
        }
        else
        {
            animator.CrossFade(newState, transitionTime);
        }

        currentAnimation = newState;
    }
    
    void StopAnimationCoroutine()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
    }

    public void Move(Direction dir)
    {
        Vector3 movementVector = Vector3.zero;
        string moveAnimName = "";

        switch (dir)
        {
            case Direction.Forward:
                movementVector = Vector3.forward;
                moveAnimName = "Walk";
                moveSpeed = baseMoveSpeed;
                break;
            case Direction.Backward:
                movementVector = Vector3.back;
                moveAnimName = "Walk_Back";
                moveSpeed = baseMoveSpeed;
                break;
            case Direction.Left:
                movementVector = Vector3.left;
                moveAnimName = "Walk_Left";
                moveSpeed = baseMoveSpeed/1.25f;
                break;
            case Direction.Right:
                movementVector = Vector3.right;
                moveAnimName = "Walk_Right";
                moveSpeed = baseMoveSpeed/1.25f;
                break;
        }
        
        if (movementVector != Vector3.zero)
        {
            movementVector.z *= 1.25f;

            StopAnimationCoroutine();

            targetPosition = transform.position + (movementVector * moveDistance);
            isMoving = true;
            inAction = true;

            ChangeAnimation(moveAnimName);
        }
    }

    void MoveTowardTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            isMoving = false;
            inAction = false;

            idleCoroutine = StartCoroutine(IdleRandomize());
        }
    }

    IEnumerator IdleRandomize()
    {
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(0.1f);

        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        string nextIdle = idleAnimations[UnityEngine.Random.Range(0, idleAnimations.Length)];
        ChangeAnimation(nextIdle);

        yield return new WaitForSeconds(length);

        idleCoroutine = StartCoroutine(IdleRandomize());
    }

    /// <summary>
    /// Plays animation, waits for it to finish, then executes action.
    /// </summary>
    public void PerformAction(PlayerAction actionKey, Action onAnimationComplete)
    {
        if (animationDict.TryGetValue(actionKey, out string actualAnimatorState))
        {
            StartCoroutine(ActionRoutine(actualAnimatorState, onAnimationComplete));
        }
        else
        {
            Debug.LogError($"The action {actionKey} is missing from the Player's Animation Setup in the Inspector!");
            onAnimationComplete?.Invoke();
        }
    }

    private IEnumerator ActionRoutine(string animName, Action onAnimationComplete)
    {
        StopAnimationCoroutine();
        inAction = true;

        ChangeAnimation(animName, 0.1f, true);

        float timeout = 0.5f;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animName) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        onAnimationComplete?.Invoke();

        inAction = false;

        currentAnimation = "";

        idleCoroutine = StartCoroutine(IdleRandomize());
    }
}