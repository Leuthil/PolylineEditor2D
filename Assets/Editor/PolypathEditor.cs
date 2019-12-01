using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    [CustomEditor(typeof(Polypath))]
    public class PolypathEditor : PolylineEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
        }

        private void UpdateTerrain(Vector3[] localPoints)
        {
            return;

            if (localPoints.Length < 3)
            {
                return;
            }

            List<Vector3> vertices = new List<Vector3>(localPoints);
            Triangulator triangulator = new Triangulator(vertices.ToArray());
            int[] indecies = triangulator.Triangulate();
            Polypath terrain = target as Polypath;
            MeshFilter meshFilter = terrain.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;

            mesh.triangles = null;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indecies;
            mesh.uv = Vec3ToVec2Array(vertices.ToArray());

            PolygonCollider2D collider = terrain.GetComponent<PolygonCollider2D>();
            collider.points = Vec3ToVec2Array(vertices.ToArray());
        }

        private Vector2[] Vec3ToVec2Array(Vector3[] data)
        {
            Vector2[] result = new Vector2[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = data[i];
            }

            return result;
        }
    }
}