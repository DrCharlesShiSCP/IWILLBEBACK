using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{
    public class TestCapsuleCast : MonoBehaviour
    {
        // Start is called before the first frame update

        private FirstPersonController fpController;

        public float targetHeight = 1.8f;

        void Start()
        {
            fpController = GetComponent<FirstPersonController>();
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log(CheckForCeilingFree(targetHeight));
        }

        private bool CheckForCeilingFree(float targetHeight)
        {
            //Ray ceilRay = new Ray(GetColliderCeilPosition(), tr.up);
            //bool isCeilingFree = !Physics.SphereCast(ceilRay, characterController.radius, 0.1f, ceilingDetectionLayerMask);

            //return isCeilingFree;

            float targetRadius = fpController.characterController.radius = Mathf.Min(targetHeight / 2f, 0.3f);

            Vector3 p1 = fpController.GetColliderBasePosition() + (Vector3.up * targetRadius) + (Vector3.up * 0.25f);
            Vector3 p2 = fpController.GetColliderBasePosition() + (Vector3.up * targetHeight) - (Vector3.up * targetRadius);

            climbGizmosP1 = p1;
            climbGizmosP2 = p2;

            if (Physics.CheckCapsule(p1, p2, fpController.characterController.radius, fpController.ceilingDetectionLayerMask))
            {
                return false;
            }

            return true;
        }

        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
#if UNITY_EDITOR
            // Special case when both points are in the same position
            if (p1 == p2)
            {
                // DrawWireSphere works only in gizmo methods
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }
            using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
            {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f)
                {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }
                // First side
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
                // Second side
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
                // Lines
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
#endif
        }

        Vector3 climbGizmosP1;
        Vector3 climbGizmosP2;

        private void OnDrawGizmos()
        {
            if (fpController == null)
                return;

            Gizmos.color = Color.red;
            DrawWireCapsule(climbGizmosP1, climbGizmosP2, fpController.characterController.radius);
        }
    }

}
