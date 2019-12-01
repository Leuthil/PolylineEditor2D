﻿using UnityEngine;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    public class Polyline : MonoBehaviour
    {
        [HideInInspector]
        public List<Vector3> nodes = new List<Vector3>(new Vector3[] {
            new Vector3(-1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, -1f, 0f)
        });
    }
}