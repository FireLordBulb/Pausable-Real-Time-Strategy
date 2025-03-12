using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Graphs
{
    public static class EditorGraphUtils<TNode, TLink> where TLink : ILink<TNode, TLink> where TNode : class, INode<TLink, TNode>
    {
        public static void DrawGraph(IGraph<TNode, TLink> graph)
        {
            foreach (TNode node in graph.Nodes)
            {
                if (node is IPositionNode<TLink, TNode> source)
                {
                    // draw node position
                    Handles.color = Color.yellow;
                    Handles.CubeHandleCap(0, source.WorldPosition, Quaternion.identity, 0.1f, EventType.Repaint);
    
                    // draw node links
                    foreach (TLink link in source.Links)
                    {
                        if (link.Target is IPositionNode<TLink, TNode> target)
                        {
                            Handles.color = Color.blue;
                            Handles.DrawLine(source.WorldPosition, target.WorldPosition);
                        }
                    }
                }
            }
        }
    }
}