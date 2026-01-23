using GamePlay.Controller;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Controller
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [FormerlySerializedAs("starterAssetsInputs")] [Header("Output")]
        public PlayerAssetsInputs playerAssetsInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            playerAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            playerAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            playerAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            playerAssetsInputs.SprintInput(virtualSprintState);
        }
        
    }

}
