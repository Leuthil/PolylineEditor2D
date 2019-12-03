using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    [CustomEditor(typeof(Polypath))]
    public class PolypathEditor : PolylineEditor
    {
        protected override void DrawPolyLine(Vector3[] nodes, Color color)
        {
            base.DrawPolyLine(nodes, color);

            // draw line between last node and first node to make it a path
            Color previousColor = Handles.color;
            Handles.color = color;
            Handles.DrawPolyLine(new Vector3[] { nodes[nodes.Length - 1], nodes[0] });
            Handles.color = previousColor;
        }

        protected override void RemoveNode(Polyline polyline, int indexToDelete)
        {
            if (polyline.Nodes.Count <= 3)
            {
                return;
            }

            base.RemoveNode(polyline, indexToDelete);
        }

        protected override int FindNodeIndex(Vector3[] worldNodesPositions, Vector3 newNode)
        {
            return base.FindNodeIndex(new List<Vector3>(worldNodesPositions) { worldNodesPositions[0] }.ToArray(), newNode);
        }
    }
}