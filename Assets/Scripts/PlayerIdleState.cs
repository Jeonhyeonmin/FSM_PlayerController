using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        Debug.Log("Player is in Idle State");

        player.isMove = false;
        player.isIdle = true;
        player.isWalk = false;
        player.isSprint = false;

        player.animator.SetBool(player.animationHashes["Movement"]["IsMove"], player.isMove);
        player.animator.SetBool(player.animationHashes["Movement"]["IsIdle"], player.isIdle);
        player.animator.SetBool(player.animationHashes["Movement"]["IsCrouch"], player.isCrouch);
        player.animator.SetBool(player.animationHashes["Movement"]["IsWalk"], player.isWalk);
    }

    public override void ExitState(PlayerStateManager player)
    {
        Debug.Log(player.name + " is exiting Idle State");

        player.isIdle = false;
        player.animator.SetBool(player.animationHashes["Movement"]["IsIdle"], player.isIdle);
    }

    public override void FixedUpdateState(PlayerStateManager player)
    {
        
    }

    public override void UpdateState(PlayerStateManager player)
    {
        player.isMove = player.inputReader.KeyboardComposite != Vector2.zero;
        player.isIdle = !player.isMove;

        if (player.isMove)
        {
            player.SwitchState(player.moveState);
        }
    }
}
