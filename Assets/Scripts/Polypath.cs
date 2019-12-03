using System.Collections.Generic;
using UnityEngine;

namespace LinguineGames.Util.PolylineEditor2D
{
    public class Polypath : Polyline
    {
        public override void InitializeNodes()
        {
            Nodes = new List<Vector3>(new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f, 1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(1f, -1f, 0f)
            });
        }
    }
}