using UnityEngine;
using UnityEngine.Serialization;

namespace _Projects.GamePlay
{
    public class BottleObject : DisposableObject
    { 
        public GameObject bottlePrefab; 
        public GameObject afterBottlePrefab;

        public bool isOpened = false;
    
        public override void ChangeState()
        {
            if (!isOpened)
            {
                bottlePrefab.SetActive(false);
                afterBottlePrefab.SetActive(true);
                isOpened = true;
            }
            base.ChangeState();
        
        }

        public override void Recycle()
        {
            if (isOpened)
            {
                bottlePrefab.SetActive(true);
                afterBottlePrefab.SetActive(false);
                isOpened = false;
            }
            base.Recycle();
        }
    }
}