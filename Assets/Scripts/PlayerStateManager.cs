using jeonhyeonmin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    public abstract void EnterState(PlayerStateManager player);
    public abstract void UpdateState(PlayerStateManager player);
    public abstract void FixedUpdateState(PlayerStateManager player);
    public abstract void ExitState(PlayerStateManager player);
}

public class PlayerStateManager : MonoBehaviour
{
    private readonly float ANIMATION_DAMPING_TIME = 0.05f;

    #region Player StateMachine Properties

    private PlayerBaseState currentState { get; set; }

    #endregion Player StateMachine Properties

    #region StateMachine States

    public readonly PlayerIdleState idleState = new PlayerIdleState();
    public readonly PlayerMoveState moveState = new PlayerMoveState();

    #endregion StateMachine States

    #region SCRIPTS/OBJECTS References

    [Header("SCRIPTS/OBJECTS References")]
    public InputReader inputReader;

    public Camera playerCamera;

    public CharacterController characterController;
    public Animator animator;

    [SerializeField] private float normalHeight = 1.8f;

    public readonly Dictionary<string, Dictionary<string, int>> animationHashes = new Dictionary<string, Dictionary<string, int>>()
    {
        {
            "Movement",
            new Dictionary<string, int>()
            {
                { "IsMove", Animator.StringToHash("IsMove") },
                { "IsIdle", Animator.StringToHash("IsIdle") },
                { "IsCrouch", Animator.StringToHash("IsCrouch") },
                { "IsWalk", Animator.StringToHash("IsWalk") },
                { "IsSprint", Animator.StringToHash("IsSprint") },
                { "IsTurnAround", Animator.StringToHash("IsTurnAround") },
                { "MoveX", Animator.StringToHash("MoveX") },
                { "MoveZ", Animator.StringToHash("MoveZ") },
                { "MoveForward", Animator.StringToHash("MoveForward") },
            }
        },
        {
            "State",
            new Dictionary<string, int>()
            {
                { "IsJump", Animator.StringToHash("IsJump") },
                { "IsFall", Animator.StringToHash("IsFall") },
                { "IsGrounded", Animator.StringToHash("IsGrounded") },
                { "IsLookTarget", Animator.StringToHash("IsLookTarget") },
                { "CameraRotationOffset", Animator.StringToHash("CameraRotationOffset") },
                { "BodyLookX", Animator.StringToHash("BodyLookX") },
                { "BodyLookY", Animator.StringToHash("BodyLookY") },
                { "HeadLookX", Animator.StringToHash("HeadLookX") },
                { "HeadLookY", Animator.StringToHash("HeadLookY") },
                { "LeanValue", Animator.StringToHash("LeanValue") }
            }
        }
    };  // Readonly Class로 관리 방법도 있다.

    #endregion Animation Hashes

    #region Player State Booleans

    [Space(25), Header("Player State Booleans")]
    public bool isMove;
    public bool isIdle;
    public bool isCrouch;
    public bool isWalk;
    public bool isSprint;
    public bool isJump;
    public bool isFall;
    public bool isGrounded;
    public bool isTurnAround;
    public bool isLookTarget;
    public bool isSlide;

    #endregion Player State Booleans

    #region Player Movement Properties

    [Space(25), Header("Player Movement Properties")]
    [SerializeField] private float applySpeed;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float sprintSpeed = 6.0f;

    [SerializeField] private float rotationMoveSpeed = 0.1f;
    [SerializeField] private float rotationIdleSpeed = 0.008f;

    public float ApplySpeed
    {
        get => applySpeed;
        set => applySpeed = value;
    }

    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;

    #endregion Player Movement Properties

    private void Start()
    {
        inputReader = GetComponent<InputReader>();

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        SwitchState(idleState);

        inputReader.SubscribeToOnRunActivate(OnRunActivate);
        inputReader.SubscribeToOnRunDeactivate(OnRunDeactivate);
        inputReader.SubscribeToOnCrouchActivate(OnCrouchActivate);
    }

    private void Update()
    {
        currentState.UpdateState(this);

        animator.SetFloat(animationHashes["Movement"]["MoveX"], Mathf.Lerp(animator.GetFloat(animationHashes["Movement"]["MoveX"]), inputReader.AnimationKeyboardComposite.x, ANIMATION_DAMPING_TIME));
        animator.SetFloat(animationHashes["Movement"]["MoveZ"], Mathf.Lerp(animator.GetFloat(animationHashes["Movement"]["MoveZ"]), inputReader.AnimationKeyboardComposite.y, ANIMATION_DAMPING_TIME));
        animator.SetFloat(animationHashes["Movement"]["MoveForward"], Mathf.Lerp(animator.GetFloat(animationHashes["Movement"]["MoveForward"]), inputReader.KeyboardComposite != Vector2.zero ? 1 : 0, ANIMATION_DAMPING_TIME));

        CharacterRotation();

        if ((isJump || isFall || !isGrounded) && isCrouch)
        {
            OnCrouchActivate();
        }
    }

    private void CharacterRotation()
    {
        if (isSprint)
        {
            animator.SetBool(animationHashes["Movement"]["IsTurnAround"], false);

            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            Vector3 moveDirection = cameraForward * inputReader.KeyboardComposite.y + cameraRight * inputReader.KeyboardComposite.x;

            animator.SetFloat(animationHashes["State"]["BodyLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookX"]), inputReader.MouseComposite.x / 10, ANIMATION_DAMPING_TIME));
            animator.SetFloat(animationHashes["State"]["BodyLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookY"]), inputReader.MouseComposite.y / 10, ANIMATION_DAMPING_TIME));
            animator.SetFloat(animationHashes["State"]["HeadLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookX"]), inputReader.MouseComposite.x / 20, ANIMATION_DAMPING_TIME));
            animator.SetFloat(animationHashes["State"]["HeadLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookY"]), inputReader.MouseComposite.y / 20, ANIMATION_DAMPING_TIME));
            animator.SetFloat(animationHashes["State"]["LeanValue"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["LeanValue"]), inputReader.MouseComposite.x / 5, ANIMATION_DAMPING_TIME));
        }
        else
        {
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;

            Vector3 characterForward = transform.forward;
            characterForward.y = 0f;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            Quaternion characterRotation = Quaternion.LookRotation(cameraForward);

            if (isMove)
            {
                animator.SetBool(animationHashes["Movement"]["IsTurnAround"], false);

                transform.rotation = Quaternion.Lerp(transform.rotation, characterRotation, rotationMoveSpeed);

                if (cameraForward != Vector3.zero)
                {
                    animator.SetFloat(animationHashes["State"]["BodyLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookX"]), inputReader.MouseComposite.x / 30, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["BodyLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookY"]), inputReader.MouseComposite.y / 30, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["HeadLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookX"]), inputReader.MouseComposite.x / 30, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["HeadLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookY"]), inputReader.MouseComposite.y / 30, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["LeanValue"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["LeanValue"]), inputReader.MouseComposite.x / 35, ANIMATION_DAMPING_TIME));
                }
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, characterRotation, rotationIdleSpeed);

                characterForward.Normalize();
                cameraForward.Normalize();

                float cameraRotationOffset = Vector3.SignedAngle(characterForward, cameraForward, Vector3.up);

                if (Mathf.Abs(cameraRotationOffset) >= 5)
                {
                    Debug.Log("Turn Right");
                    animator.SetBool(animationHashes["Movement"]["IsTurnAround"], true);

                    animator.SetFloat(animationHashes["State"]["CameraRotationOffset"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["CameraRotationOffset"]), cameraRotationOffset, ANIMATION_DAMPING_TIME));
                }
                else
                {
                    animator.SetBool(animationHashes["Movement"]["IsTurnAround"], false);

                    animator.SetFloat(animationHashes["State"]["CameraRotationOffset"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["CameraRotationOffset"]), 0, ANIMATION_DAMPING_TIME));
                }

                if (cameraForward != Vector3.zero)
                {
                    animator.SetFloat(animationHashes["State"]["BodyLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookX"]), inputReader.MouseComposite.x / 15, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["BodyLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["BodyLookY"]), inputReader.MouseComposite.y / 15, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["HeadLookX"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookX"]), inputReader.MouseComposite.x / 15, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["HeadLookY"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["HeadLookY"]), inputReader.MouseComposite.y / 15, ANIMATION_DAMPING_TIME));
                    animator.SetFloat(animationHashes["State"]["LeanValue"], Mathf.Lerp(animator.GetFloat(animationHashes["State"]["LeanValue"]), 0, ANIMATION_DAMPING_TIME));
                }
            } 
        }
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdateState(this);
    }

    public void SwitchState(PlayerBaseState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        inputReader.UnsubscribeFromOnRunActivate(OnRunActivate);
        inputReader.UnsubscribeFromOnRunDeactivate(OnRunDeactivate);
        inputReader.SubscribeToOnCrouchActivate(OnCrouchActivate);
    }

    #region Action Methods

    private void OnRunActivate()
    {
        if (currentState is PlayerMoveState moveState)
        {
            ApplySpeed = SprintSpeed;
            isWalk = false;
            isSprint = true;

            animator.SetBool(animationHashes["Movement"]["IsWalk"], isWalk);
            animator.SetBool(animationHashes["Movement"]["IsSprint"], isSprint);
        }
    }

    private void OnRunDeactivate()
    {
        if (currentState is PlayerMoveState moveState)
        {
            ApplySpeed = WalkSpeed;
        }

        isWalk = true;
        isSprint = false;

        animator.SetBool(animationHashes["Movement"]["IsWalk"], isWalk);
        animator.SetBool(animationHashes["Movement"]["IsSprint"], isSprint);
    }

    private void OnCrouchActivate()
    {
        if ((isFall || isJump || !isGrounded) && !isCrouch || isSlide)
        {
            Debug.Log("return Crouch Activate");  
            return;
        }

        isCrouch = !isCrouch;

        if (isSprint && !isSlide)
        {
            StopCoroutine(SlideCoroutine());
            StartCoroutine(SlideCoroutine());
            return;
        }

        Debug.Log("Crouch Activate");
        animator.SetBool(animationHashes["Movement"]["IsCrouch"], isCrouch);
    }

    public IEnumerator SlideCoroutine()
    {
        animator.SetBool(animationHashes["Movement"]["IsCrouch"], isCrouch);
        isSlide = true;
        Debug.Log("아아ㅏ");

        yield return new WaitForSeconds(1f);

        isSlide = false;
        isCrouch = false;
        animator.SetBool(animationHashes["Movement"]["IsCrouch"], isCrouch);
    }

    #endregion Action Methods
}
