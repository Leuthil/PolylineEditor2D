using UnityEngine;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
    public class Simple2DTerrain : MonoBehaviour
    {
        public List<Vector3> nodes = new List<Vector3>();
    }
}