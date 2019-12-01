using UnityEngine;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    public class Polyline : MonoBehaviour
    {
        public List<Vector3> nodes = new List<Vector3>(new Vector3[] { new Vector3(-3, 0, 0), new Vector3(3, 0, 0) });
    }
}
