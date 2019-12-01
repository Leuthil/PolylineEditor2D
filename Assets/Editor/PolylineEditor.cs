using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LinguineGames.Util.PolylineEditor2D
{
    [CustomEditor(typeof(Polyline))]
    public class PolylineEditor : Editor
    {
        protected Texture nodeTexture;
        protected GUIStyle handleStyle = new GUIStyle();
        protected bool nodeBeingDragged = false;

        protected virtual void OnEnable()
        {
            nodeTexture = Resources.Load<Texture>("PolylineEditor2DHandle");
            
            if (nodeTexture == null)
            {
                nodeTexture = EditorGUIUtility.whiteTexture;
            }

            handleStyle.alignment = TextAnchor.MiddleCenter;
            handleStyle.fixedWidth = 15;
            handleStyle.fixedHeight = 15;
        }

        protected virtual void OnSceneGUI()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (Event.current.type == EventType.Layout)
            {
                // repaint our level editor window in case scene view layout got changed to 2d or 3d
                Repaint();
            }

            if (!sceneView.in2DMode)
            {
                return;
            }

            if (nodeBeingDragged && Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                nodeBeingDragged = false;
            }

            Polyline polyline = (target as Polyline);
            Vector3[] localPoints = polyline.nodes.ToArray();
            Vector3[] worldPoints = new Vector3[polyline.nodes.Count];

            for (int i = 0; i < worldPoints.Length; i++)
            {
                worldPoints[i] = polyline.transform.TransformPoint(localPoints[i]);
            }

            DrawPolyLine(worldPoints);
            DrawNodes(polyline, worldPoints);

            if (!nodeBeingDragged && Event.current.shift)
            {
                //Adding Points
                Vector3 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                Vector3 polyLocalMousePos = polyline.transform.InverseTransformPoint(mousePos);
                Vector3 nodeOnPoly = HandleUtility.ClosestPointToPolyLine(worldPoints);
                float handleSize = HandleUtility.GetHandleSize(nodeOnPoly);
                int nodeIndex = FindNodeIndex(worldPoints, nodeOnPoly);

                Handles.color = Color.green;
                Handles.DrawLine(worldPoints[nodeIndex - 1], mousePos);
                Handles.DrawLine(worldPoints[nodeIndex], mousePos);
                Handles.color = Color.white;

                if (Handles.Button(mousePos, Quaternion.identity, handleSize * 0.1f, handleSize, AddPolylineNodeHandleCap))
                {
                    polyLocalMousePos.z = 0;
                    Undo.RecordObject(polyline, "Insert Node");
                    polyline.nodes.Insert(nodeIndex, polyLocalMousePos);
                    Event.current.Use();
                }
            }
            else if (!nodeBeingDragged && Event.current.control)
            {
                //Deleting Points
                int indexToDelete = FindNearestNodeToMouse(worldPoints);
                Handles.color = Color.red;
                float handleSize = HandleUtility.GetHandleSize(worldPoints[0]);

                if (Handles.Button(worldPoints[indexToDelete], Quaternion.identity, handleSize * 0.1f, handleSize, DeletePolylineNodeHandleCap))
                {
                    Undo.RecordObject(polyline, "Remove Node");
                    polyline.nodes.RemoveAt(indexToDelete);
                    indexToDelete = -1;
                    Event.current.Use();
                }

                Handles.color = Color.white;
            }
        }

        protected virtual int FindNearestNodeToMouse(Vector3[] worldNodesPositions)
        {
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            int index = -1;
            float minDistnce = float.MaxValue;

            mousePos.z = 0;

            for (int i = 0; i < worldNodesPositions.Length; i++)
            {
                float distance = Vector3.Distance(worldNodesPositions[i], mousePos);

                if (distance < minDistnce)
                {
                    index = i;
                    minDistnce = distance;
                }
            }

            return index;
        }

        protected virtual int FindNodeIndex(Vector3[] worldNodesPositions, Vector3 newNode)
        {
            float smallestdis = float.MaxValue;
            int prevIndex = 0;

            for (int i = 1; i < worldNodesPositions.Length; i++)
            {
                float distance = HandleUtility.DistanceToPolyLine(worldNodesPositions[i - 1], worldNodesPositions[i]);

                if (distance < smallestdis)
                {
                    prevIndex = i - 1;
                    smallestdis = distance;
                }
            }

            return prevIndex + 1;
        }

        protected virtual void DrawPolyLine(Vector3[] nodes)
        {
            DrawPolyLine(nodes, Color.white);
        }

        protected virtual void DrawPolyLine(Vector3[] nodes, Color color)
        {
            Color previousColor = Handles.color;
            Handles.color = color;
            Handles.DrawPolyLine(nodes);
            Handles.color = previousColor;
        }

        protected virtual void DrawNodes(Polyline polyline, Vector3[] worldPoints)
        {
            for (int i = 0; i < polyline.nodes.Count; i++)
            {
                Vector3 pos = polyline.transform.TransformPoint(polyline.nodes[i]);
                float handleSize = HandleUtility.GetHandleSize(pos);
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.FreeMoveHandle(pos, Quaternion.identity, handleSize * 0.09f, Vector3.one, PolylineNodeHandleCap);
                List<Vector3> alignTo;

                if (EditorGUI.EndChangeCheck())
                {
                    nodeBeingDragged = true;

                    if (CheckAlignment(worldPoints, handleSize * 0.1f, i, ref newPos, out alignTo))
                    {
                        Handles.color = Color.green;

                        for (int j = 0; j < alignTo.Count; j++)
                        {
                            Handles.DrawPolyLine(newPos, alignTo[j]);
                        }

                        Handles.color = Color.white;
                    }

                    Undo.RecordObject(polyline, "Move Node");
                    polyline.nodes[i] = polyline.transform.InverseTransformPoint(newPos);

                    Event.current.Use();
                }
            }
        }

        protected virtual bool CheckAlignment(Vector3[] worldNodes, float offset, int index, ref Vector3 position, out List<Vector3> alignedTo)
        {
            //check vertical
            //check with the prev node
            bool aligned = false;
            //the node can be aligned to the prev and next node at once, we need to return more than one alginedTo Node
            alignedTo = new List<Vector3>(2);

            if (index > 0)
            {
                float dx = Mathf.Abs(worldNodes[index - 1].x - position.x);

                if (dx < offset)
                {
                    position.x = worldNodes[index - 1].x;
                    alignedTo.Add(worldNodes[index - 1]);
                    aligned = true;
                }
            }

            //check with the next node
            if (index < worldNodes.Length - 1)
            {
                float dx = Mathf.Abs(worldNodes[index + 1].x - position.x);

                if (dx < offset)
                {
                    position.x = worldNodes[index + 1].x;
                    alignedTo.Add(worldNodes[index + 1]);
                    aligned = true;
                }
            }

            //check horizontal
            if (index > 0)
            {
                float dy = Mathf.Abs(worldNodes[index - 1].y - position.y);

                if (dy < offset)
                {
                    position.y = worldNodes[index - 1].y;
                    alignedTo.Add(worldNodes[index - 1]);
                    aligned = true;
                }
            }

            //check with the next node
            if (index < worldNodes.Length - 1)
            {
                float dy = Mathf.Abs(worldNodes[index + 1].y - position.y);

                if (dy < offset)
                {
                    position.y = worldNodes[index + 1].y;
                    alignedTo.Add(worldNodes[index + 1]);
                    aligned = true;
                }
            }

            //check straight lines
            //To be implemented

            return aligned;
        }

        protected virtual void PolylineNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Layout)
            {
                HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position, size));

                return;
            }

            if (eventType != EventType.Repaint)
            {
                return;
            }

            if (controlID == GUIUtility.hotControl)
            {
                GUI.color = Handles.color = new Color(0f, 1f, 0f, 0.5f);
            }
            else
            {
                GUI.color = Color.green;
            }

            Handles.Label(position, new GUIContent(nodeTexture), handleStyle);
            GUI.color = Color.white;
        }

        protected virtual void AddPolylineNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Layout)
            {
                HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position, size));

                return;
            }

            if (eventType != EventType.Repaint)
            {
                return;
            }

            GUI.color = Color.green;
            Handles.Label(position, new GUIContent(nodeTexture), handleStyle);
            GUI.color = Color.white;
        }

        protected virtual void DeletePolylineNodeHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            if (eventType == EventType.Layout)
            {
                HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position, size));

                return;
            }

            if (eventType != EventType.Repaint)
            {
                return;
            }

            GUI.color = Color.red;
            Handles.Label(position, new GUIContent(nodeTexture), handleStyle);
            GUI.color = Color.white;
        }
    }
}