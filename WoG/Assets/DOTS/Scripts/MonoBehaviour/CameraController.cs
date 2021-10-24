using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void UpdateCameraPosition(float speed)
    //{
    //    Vector3 newCameraPosition;
    //    if (!m_UseHeadBob)
    //    {
    //        return;
    //    }
    //    if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
    //    {
    //        m_Camera.transform.localPosition =
    //            m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
    //                              (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
    //        newCameraPosition = m_Camera.transform.localPosition;
    //        newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
    //    }
    //    else
    //    {
    //        newCameraPosition = m_Camera.transform.localPosition;
    //        newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
    //    }
    //    m_Camera.transform.localPosition = newCameraPosition;
    //}
}
