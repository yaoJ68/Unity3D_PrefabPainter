
#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace nTools.PrefabPainter
{
    public struct RaycastHitEx
    {       
        public Ray     ray;  
        public bool    isHit;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 localPoint;
        public Vector3 localNormal;
        public Vector2 textureCoord;
        public Vector2 barycentricCoordinate;
        public float   distance;
        public int     triangleIndex;
    }



 
	public static class Utility
	{
        // 
        public const float  kEpsilon = 0.001f;

        //
        public delegate bool HandleUtility_IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out UnityEngine.RaycastHit raycastHit);
        public static HandleUtility_IntersectRayMesh IntersectRayMesh = null;



        // Static Constructor
        static Utility()
        {            
            MethodInfo methodIntersectRayMesh = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic);

            if (methodIntersectRayMesh != null)
            {
                IntersectRayMesh = delegate(Ray ray, Mesh mesh, Matrix4x4 matrix, out UnityEngine.RaycastHit raycastHit)
                {
                    object[] parameters = new object[] { ray, mesh, matrix, null };
                    bool result = (bool)methodIntersectRayMesh.Invoke(null, parameters);
                    raycastHit = (UnityEngine.RaycastHit)parameters[3];
                    return result;
                };
            }

        }


	

        public static bool IntersectRayMeshEx(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHitEx raycastHit)
        {
            raycastHit = default(RaycastHitEx);
            raycastHit.isHit = false;
            raycastHit.distance = Mathf.Infinity;
            raycastHit.ray = ray;

            UnityEngine.RaycastHit unityRaycastHit = default(UnityEngine.RaycastHit);

            if (Utility.IntersectRayMesh != null && 
                Utility.IntersectRayMesh(ray, mesh, matrix, out unityRaycastHit))
            {       
                raycastHit.isHit = true;
                raycastHit.point = unityRaycastHit.point;
                raycastHit.normal = unityRaycastHit.normal.normalized;
                raycastHit.ray = ray;
                raycastHit.triangleIndex = unityRaycastHit.triangleIndex;
                raycastHit.textureCoord = unityRaycastHit.textureCoord;
                raycastHit.distance = unityRaycastHit.distance;

                raycastHit.barycentricCoordinate.x = unityRaycastHit.barycentricCoordinate.x;
                raycastHit.barycentricCoordinate.y = unityRaycastHit.barycentricCoordinate.y;

                Matrix4x4 normal_WToL_Matrix = matrix.transpose.inverse;
                raycastHit.localNormal = normal_WToL_Matrix.MultiplyVector(raycastHit.normal).normalized;
                raycastHit.localPoint = matrix.inverse.MultiplyPoint (raycastHit.point);

                return true;
            }

            return false;
        }


        //
        //
		public static bool CompareVector2 (Vector2 a, Vector2 b) {
			return Mathf.Abs (a.x - b.x) < kEpsilon && Mathf.Abs (a.y - b.y) < kEpsilon;
		}



        static public void MarkActiveSceneDirty()
        {
            // Mark scene changed
#if (UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            EditorApplication.MarkSceneDirty ();
#else       
            UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
#endif
        }


	} // class Utility





} // namespace 

#endif // (UNITY_EDITOR)

