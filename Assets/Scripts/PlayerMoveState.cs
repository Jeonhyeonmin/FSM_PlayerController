using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    private float idleTransitionTime = 0.2f;
    private float idleTimer;

    public bool isSliding = false;

    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log("Player is in Move State");

        player.isIdle = false;
        player.isMove = true;
        player.animator.SetBool(player.animationHashes["Movement"]["IsIdle"], player.isIdle);
        player.animator.SetBool(player.animationHashes["Movement"]["IsMove"], player.isMove);

        player.ApplySpeed = player.WalkSpeed;
        idleTimer = 0f;
    }

    public override void ExitState(PlayerStateManager player)
    {
        Debug.Log(player.name + " is exiting Move State");
    }

    public override void FixedUpdateState(PlayerStateManager player)
    {
        Vector3 forwardComposite = player.transform.forward * player.inputReader.KeyboardComposite.y;
        Vector3 rightComposite = player.transform.right * player.inputReader.KeyboardComposite.x;
        Vector3 cameraForward = player.playerCamera.transform.forward;
        Vector3 cameraRight = player.playerCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        if (!player.isSlide)
        {
            if (player.isSprint)
            {
                Vector3 moveDirection = cameraForward * player.inputReader.KeyboardComposite.y + cameraRight * player.inputReader.KeyboardComposite.x;

                if (moveDirection.sqrMagnitude > 0.0f)
                {
                    player.transform.forward = Vector3.Lerp(player.transform.forward, moveDirection.normalized, 0.30f);
                    moveDirection = player.transform.forward;
                }

                moveDirection.Normalize();

                player.characterController.Move(moveDirection * player.ApplySpeed * Time.fixedDeltaTime);
            }
            else
            {
                player.characterController.Move((forwardComposite + rightComposite) * player.ApplySpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            player.characterController.Move(player.transform.forward * 7.5f * Time.fixedDeltaTime);
            Debug.Log("Sliding");   
        }
    }

    public override void UpdateState(PlayerStateManager player)
    {
        if (player.inputReader.KeyboardComposite != Vector2.zero)
        {
            player.isMove = true;
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= idleTransitionTime)
            {
                player.isMove = false;
            }
        }

        if (!player.isMove && !player.isSlide)
        {
            player.SwitchState(player.idleState);
        }
    }
}
