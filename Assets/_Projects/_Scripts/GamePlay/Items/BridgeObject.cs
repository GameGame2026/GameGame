using UnityEngine;
using UnityEngine.Serialization;

namespace _Projects.GamePlay
{
    public class BridgeObject : DisposableObject
    {
        public GameObject bridgePrefab; 
        public GameObject afterBridgePrefab;

        public bool isOpened = false;
    
        public override void ChangeState()
        {
            base.ChangeState();
            if (!isOpened)
            {
                bridgePrefab.SetActive(false);
                afterBridgePrefab.SetActive(true);
                isOpened = true;
            }
        
        }

        public override void Recycle()
        {
            base.Recycle();
        }
    }
}