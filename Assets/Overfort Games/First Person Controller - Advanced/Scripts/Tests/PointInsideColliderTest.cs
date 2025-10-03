using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverfortGames.FirstPersonController.Test
{

    public class PointInsideColliderTest : MonoBehaviour
    {
        public MeshCollider target;

        private void Update()
        {
            //  DebugPlus.DrawSphere(transform.position, 0.05f).Color(Color.yellow);

            if (target == null)
                return;

            if (IsInsideMeshCollider(target, transform.position))
            {
                Debug.Log("true");
            }
            else
            {
                Debug.Log("false");
            }

        }


        RaycastHit[] _hitsUp = new RaycastHit[300];
        RaycastHit[] _hitsDown = new RaycastHit[300];
        private bool IsInsideMesh(Vector3 point)
        {
            Physics.queriesHitBackfaces = true;
            int hitsUp = Physics.RaycastNonAlloc(point, Vector3.up, _hitsUp);
            int hitsDown = Physics.RaycastNonAlloc(point, Vector3.down, _hitsDown);
            Physics.queriesHitBackfaces = false;

            Debug.Log(hitsUp);
            for (var i = 0; i < hitsUp; i++)
                if (_hitsUp[i].normal.y > 0)
                    for (var j = 0; j < hitsDown; j++)
                        if (_hitsDown[j].normal.y < 0 && _hitsDown[j].collider == _hitsUp[i].collider)
                            return true;

            return false;
        }

        private bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
        {
            var temp = Physics.queriesHitBackfaces;
            Ray ray = new Ray(point, Vector3.back);

            bool hitFrontFace = false;
            RaycastHit hit = default;

            Physics.queriesHitBackfaces = true;
            bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
            if (hitFrontOrBackFace)
            {
                Physics.queriesHitBackfaces = false;
                hitFrontFace = col.Raycast(ray, out hit, 100f);
            }
            Physics.queriesHitBackfaces = temp;

            if (!hitFrontOrBackFace)
            {
                return false;
            }
            else if (!hitFrontFace)
            {
                return true;
            }
            else
            {
                // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
                if (hit.distance > hit2.distance)
                {
                    return true;
                }
                else
                    return false;
            }

        }
    }

}
