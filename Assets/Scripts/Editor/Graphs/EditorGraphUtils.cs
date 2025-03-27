using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Graphs
{
    public static class EditorGraphUtils<TNode, TLink> where TLink : ILink<TNode, TLink> where TNode : class, IPositionNode<TLink, TNode>
    {
        public static void DrawGraph(IEnumerable<TNode> nodes)
        {
            foreach (TNode node in nodes)
            {
                // draw node position
                Handles.color = Color.yellow;
                Handles.CubeHandleCap(0, node.WorldPosition, Quaternion.identity, 0.3f, EventType.Repaint);

                // draw node links
                foreach (TLink link in node.Links)
                {
                    if (link.Target is IPositionNode<TLink, TNode> target)
                    {
                        Handles.color = Color.magenta;
                        Handles.DrawLine(node.WorldPosition, target.WorldPosition);
                    }
                }
            }
        }
    }
}