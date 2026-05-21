using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAction
{
    Idle,
    Move,
    Attack,
    Death
}

[System.Serializable]
public struct EnemyAnimationMapping
{
    public EnemyAction action;
    public string animatorStateName;
}

public class Enemy : BaseTile
{
    [HideInInspector] public Vector2Int gridLoc;

    [HideInInspector] public List<Vector2Int> patrolPath = new List<Vector2Int>();
    public int currentPathIndex = 0;
    public int pathDirection = 1;

    [Header("Reference")]
    public GameObject enemyMesh;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool inAction = false;
    public bool isDead = false;

    [Header("Animations")]
    public Animator animator;
    private string currentAnimation;

    public EnemyAnimationMapping[] animationSetup;
    private Dictionary<EnemyAction, string> animationDict = new Dictionary<EnemyAction, string>();

    void Awake()
    {
        foreach (EnemyAnimationMapping map in animationSetup)
        {
            if (!animationDict.ContainsKey(map.action))
            {
                animationDict.Add(map.action, map.animatorStateName);
            }
            else
            {
                Debug.LogWarning($"You have assigned the action {map.action} multiple times in your Enemy Inspector!");
            }
        }
    }

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        targetPosition = transform.position;

        PlayIdle();
    }

    void Update()
    {
        if (isMoving && !isDead) MoveTowardTarget();
    }

    public void Setup(List<Vector2Int> path)
    {
        patrolPath = path;
        currentPathIndex = 0;
        pathDirection = 1;

        if (patrolPath != null && patrolPath.Count > 0)
        {
            gridLoc = patrolPath[0];
            UpdateVisualPosition();

            if (patrolPath.Count > 1)
            {
                SnapRotationToDirection(patrolPath[1] - patrolPath[0]);
            }
        }
    }

    public void UpdateVisualPosition()
    {
        Vector3 newPos = new Vector3(gridLoc.y, transform.localPosition.y, gridLoc.x);
        transform.localPosition = newPos;
        targetPosition = newPos;
    }

    public void SnapRotationToDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.zero) return;
        Vector3 lookDir = new Vector3(direction.y, 0, direction.x);
        if(enemyMesh) enemyMesh.transform.localRotation = Quaternion.LookRotation(lookDir);
    }

    public Vector2Int GetNextPatrolNode()
    {
        if (patrolPath == null || patrolPath.Count <= 1) return gridLoc;

        int nextIndex = currentPathIndex + pathDirection;

        if (nextIndex >= patrolPath.Count || nextIndex < 0)
        {
            pathDirection *= -1;
            nextIndex = currentPathIndex + pathDirection;
        }

        return patrolPath[nextIndex];
    }

    public void MoveForward(Vector2Int newGridLoc)
    {
        if (isDead) return;

        Vector2Int dir = newGridLoc - gridLoc;
        SnapRotationToDirection(dir);

        gridLoc = newGridLoc;
        targetPosition = new Vector3(gridLoc.y, transform.localPosition.y, gridLoc.x);

        isMoving = true;
        inAction = true;

        if (animationDict.TryGetValue(EnemyAction.Move, out string moveAnim))
        {
            ChangeAnimation(moveAnim);
        }
    }

    public void AdvancePathIndex()
    {
        if (patrolPath == null || patrolPath.Count <= 1) return;

        int nextIndex = currentPathIndex + pathDirection;

        if (nextIndex >= patrolPath.Count || nextIndex < 0)
        {
            pathDirection *= -1;
        }

        currentPathIndex += pathDirection;
    }

    void MoveTowardTarget()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            transform.localPosition = targetPosition;
            isMoving = false;
            inAction = false;

            PlayIdle();
        }
    }

    public void ChangeAnimation(string newState, float transitionTime = 0.1f, bool forceRestart = false)
    {
        Debug.Log(newState);

        if (currentAnimation == newState && !forceRestart) return;

        if (forceRestart)
        {
            animator.Play(newState, 0, 0f);
        }
        else
        {
            animator.CrossFade(newState, transitionTime);
        }

        currentAnimation = newState;
    }

    private void PlayIdle()
    {
        if (isDead) return;

        if (animationDict.TryGetValue(EnemyAction.Idle, out string idleAnim))
        {
            ChangeAnimation(idleAnim);
        }
    }

    public void PerformAction(EnemyAction actionKey, Action onAnimationComplete)
    {
        if (isDead) return;

        if (animationDict.TryGetValue(actionKey, out string actualAnimatorState))
        {
            StartCoroutine(ActionRoutine(actionKey, actualAnimatorState, onAnimationComplete));
        }
        else
        {
            Debug.LogError($"The action {actionKey} is missing from the Enemy's Animation Setup!");
            onAnimationComplete?.Invoke();
        }
    }

    private IEnumerator ActionRoutine(EnemyAction action, string animName, Action onAnimationComplete)
    {
        inAction = true;

        if (action == EnemyAction.Death)
        {
            isDead = true;
        }

        ChangeAnimation(animName, 0.1f, true);

        yield return null;

        float timeout = 0.5f;

        while (timeout > 0)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(animName) ||
                animator.GetNextAnimatorStateInfo(0).IsName(animName))
            {
                break;
            }
            timeout -= Time.deltaTime;
            yield return null;
        }

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(animName) && stateInfo.normalizedTime >= 0.95f)
            {
                break;
            }

            if (!stateInfo.IsName(animName) && !animator.GetNextAnimatorStateInfo(0).IsName(animName) && timeout <= 0)
            {
                break;
            }

            yield return null;
        }

        onAnimationComplete?.Invoke();

        inAction = false;
        currentAnimation = "";

        PlayIdle();
    }
}