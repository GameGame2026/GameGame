using UnityEngine;

namespace _Projects.GamePlay
{
    // 石墙用动画的方式来做到：依次下砸
    public class StoneWall : DisposableObject
    {
        public Animator stoneWallAnimation;
        
        
        public override void ChangeState()
        {
            stoneWallAnimation.enabled = false;
            base.ChangeState();
        }
        

        public override void Recycle()
        {
            stoneWallAnimation.enabled = true;
            base.Recycle();
        }
    }
}