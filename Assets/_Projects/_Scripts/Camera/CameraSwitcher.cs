using System;
using UnityEngine;
using Cinemachine;
using System.Collections;

namespace _Projects.Camera
{
    public class CameraSwitcher : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                CinemachineVirtualCamera vcam = GetComponentInParent<CinemachineVirtualCamera>();
                if(vcam != null)
                {
                    vcam.Priority = 99; // 激活相机
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                CinemachineVirtualCamera vcam = GetComponentInParent<CinemachineVirtualCamera>();
                if(vcam != null)
                {
                    vcam.Priority = 10; // 恢复默认优先级
                }
            }
        }
    }
}
