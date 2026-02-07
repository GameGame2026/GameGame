using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 lookPos = cam.transform.position - transform.position;
            lookPos.y = 0; 
            lookPos.x = 0; 
            transform.rotation = Quaternion.LookRotation(lookPos);

            transform.forward = cam.transform.forward;
        }
    }
}