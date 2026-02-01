using GamePlay.Controller;
using UnityEngine;

namespace GamePlay.Controller
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")]
        public PlayerInputHandler inputHandler;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            inputHandler.SetMoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            inputHandler.SetLookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            inputHandler.SetJumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            inputHandler.SetSprintInput(virtualSprintState);
        }
    }

}
