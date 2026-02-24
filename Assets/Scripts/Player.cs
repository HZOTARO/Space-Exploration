using System.Collections;
using UnityEngine;

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right
}

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 2f;
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool inAction = false;

    [Header("Animations")]
    public Animator animator;
    private string currentAnimation;
    public string[] idleAnimations = { "Idle_Aggro", "Idle_Aggro", "Idle_Breaker" };
    private Coroutine idleCoroutine;

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

    public void ChangeAnimation(string newState, float transitionTime = 0.1f)
    {
        if (currentAnimation == newState) return;
        animator.CrossFade(newState, transitionTime);
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
                break;
            case Direction.Backward:
                movementVector = Vector3.back;
                moveAnimName = "Walk_Back";
                break;
            case Direction.Left:
                movementVector = Vector3.left;
                moveAnimName = "Walk_Left";
                break;
            case Direction.Right:
                movementVector = Vector3.right;
                moveAnimName = "Walk_Right";
                break;
        }
        
        if (movementVector != Vector3.zero)
        {
            movementVector.z *= 2;

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

            ChangeAnimation(idleAnimations[Random.Range(0, idleAnimations.Length)]);
            idleCoroutine = StartCoroutine(IdleRandomize());
        }
    }

    IEnumerator IdleRandomize()
    {
        yield return new WaitForEndOfFrame();

        float length = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(length);

        string nextIdle = idleAnimations[Random.Range(0, idleAnimations.Length)];

        //Debug.Log(nextIdle);

        ChangeAnimation(nextIdle);

        idleCoroutine = StartCoroutine(IdleRandomize());
    }
}