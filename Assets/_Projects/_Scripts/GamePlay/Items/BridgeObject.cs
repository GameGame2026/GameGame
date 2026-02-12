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
            if (!isOpened)
            {
                bridgePrefab.SetActive(false);
                afterBridgePrefab.SetActive(true);
                isOpened = true;
            }
            base.ChangeState();
        }

        public override void Recycle()
        {
            if (isOpened)
            {
                bridgePrefab.SetActive(true);
                afterBridgePrefab.SetActive(false);
                isOpened = false;
            }
            base.Recycle();
        }
    }
}