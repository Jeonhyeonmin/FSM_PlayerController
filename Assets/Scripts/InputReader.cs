using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace jeonhyeonmin
{
    public class InputReader : MonoBehaviour, PlayerAction.IOnGroundActions
    {

        #region InputSystem Player Actions

        private PlayerAction playerAction;

        #endregion InputSystem Player Actions

        #region Input Composites

        [Header("Input Composites")]
        [SerializeField] private Vector2 keyboardComposite;
        public Vector2 KeyboardComposite
        {
            get => keyboardComposite;
            set => keyboardComposite = value;
        }

        [SerializeField] private Vector2 animationKeyboardComposite;
        public Vector2 AnimationKeyboardComposite
        {
            get => animationKeyboardComposite;
            set => animationKeyboardComposite = value;
        }

        [SerializeField] private Vector2 mouseComposite;
        public Vector2 MouseComposite
        {
            get => mouseComposite;
            set => mouseComposite = value;
        }

        #endregion Input Composites

        #region Input Actions

        private Action onMoveAction;
        private Action onLookAction;
        private Action onCrouchAction;
        private Action onRunActivateAction;
        private Action onRunDeactivateAction;
        private Action onLookTargetAction;
        private Action onTurnAroundAction;

        #endregion Input Actions

        #region Subscription / Unsubscription Methods

        public void SubscribeToOnMove(Action action) => onMoveAction += action;
        public void UnsubscribeFromOnMove(Action action) => onMoveAction -= action;

        public void SubscribeToOnLook(Action action) => onLookAction += action;
        public void UnsubscribeFromOnLook(Action action) => onLookAction -= action;

        public void SubscribeToOnCrouchActivate(Action action) => onCrouchAction += action;
        public void UnsubscribeFromOnCrouch(Action action) => onCrouchAction -= action;

        public void SubscribeToOnRunActivate(Action action) => onRunActivateAction += action;
        public void UnsubscribeFromOnRunActivate(Action action) => onRunActivateAction -= action;

        public void SubscribeToOnRunDeactivate(Action action) => onRunDeactivateAction += action;
        public void UnsubscribeFromOnRunDeactivate(Action action) => onRunDeactivateAction -= action;

        public void SubscribeToOnLookTarget(Action action) => onLookTargetAction += action;
        public void UnsubscribeFromOnLookTarget(Action action) => onLookTargetAction -= action;

        public void SubscribeToOnTurnAround(Action action) => onTurnAroundAction += action;
        public void UnsubscribeFromOnTurnAround(Action action) => onTurnAroundAction -= action;

        #endregion Subscription / Unsubscription Methods

        #region Input Action Handlers

        private void InvokeAction(Action action) => action?.Invoke();

        public void OnMove(InputAction.CallbackContext context)
        {
            KeyboardComposite = context.ReadValue<Vector2>();
            InvokeAction(onMoveAction);
        }

        public void OnAnimationMove(InputAction.CallbackContext context)
        {
            AnimationKeyboardComposite = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            MouseComposite = context.ReadValue<Vector2>();
            InvokeAction(onLookAction);
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            bool isPerformed = context.performed;

            if (isPerformed)
            {
                InvokeAction(onCrouchAction);
            }
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            bool isStarted = context.started;
            bool isCanceled = context.canceled;

            if (isStarted)
            {
                InvokeAction(onRunActivateAction);
            }
            else if (isCanceled)
            {
                InvokeAction(onRunDeactivateAction);
            }
        }

        public void OnLookTarget(InputAction.CallbackContext context)
        {
            bool isPerformed = context.performed;

            if (isPerformed)
            {
                InvokeAction(onLookTargetAction);
            }
        }

        public void OnTurnAround(InputAction.CallbackContext context)
        {
            bool isPerformed = context.performed;

            if (isPerformed)
            {
                InvokeAction(onTurnAroundAction);
            }
        }

        #endregion Input Action Handlers

        private void OnEnable()
        {
            EnablePlayerActions();
        }

        private void EnablePlayerActions()
        {
            playerAction = new PlayerAction();
            playerAction.Enable();
            playerAction.OnGround.SetCallbacks(this);
        }

        private void OnDisable()
        {
            DisablePlayerActions();
        }

        private void DisablePlayerActions()
        {
            if (playerAction != null)
            {
                playerAction.OnGround.Disable();
                playerAction = null;
            }
        }  
    }
}
