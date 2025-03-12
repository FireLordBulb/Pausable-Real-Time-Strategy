using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    public static class GraphAlgorithms<TNode, TLink> where TLink : ILink<TNode, TLink> where TNode : class, INode<TLink, TNode>
    {
        private class NodeWrapper<T> where T : TNode
        {
            public TNode            m_node;
            public NodeWrapper<T>   m_parent;
            public float            m_fDistance;
            public float            m_fRemainingDistance;

            public NodeWrapper(TNode node)
            {
                m_node = node;
                m_parent = null;
                m_fDistance = float.MaxValue;
                m_fRemainingDistance = float.MaxValue;
            }
        }

        public delegate bool LinkEvaluator(TLink link);

        public class Path : List<TLink>
        {
        }

        public static TPositionNode GetClosestNode<TPositionNode>(IGraph<TNode, TLink> graph, Vector3 vWorldPos, float fMaxDistance = float.MaxValue) where TPositionNode : class, IPositionNode<TLink, TNode>
        {
            TPositionNode bestNode = null;
            if (graph != null)
            {
                foreach (TNode node in graph.Nodes)
                {
                    if (node is not TPositionNode positionNode)
                    {
                        continue;
                    }
                    
                    float fDistance = Vector3.Distance(vWorldPos, positionNode.WorldPosition);
                    if (fDistance < fMaxDistance)
                    {
                        fMaxDistance = fDistance;
                        bestNode = positionNode;
                    }
                }
            }

            return bestNode;
        }

        public static HashSet<TNode> FloodFill(TNode start)
        {
            // setup
            Queue<TNode> open = new Queue<TNode>();
            HashSet<TNode> closed = new HashSet<TNode>();
            open.Enqueue(start);

            // search / iteration
            while (open.Count > 0) 
            {
                TNode node = open.Dequeue();
                closed.Add(node);

                // search the neighbors
                foreach (TLink link in node.Links)
                {
                    TNode neighbor = link.Target;
                    if (neighbor != null &&
                        !open.Contains(neighbor) &&
                        !closed.Contains(neighbor))
                    {
                        open.Enqueue(neighbor);
                    }
                }
            }

            // goodies in here
            return closed;
        }

        public static HashSet<TNode> GetNodesInRange(TNode start, int iRange)
        {
            // setup
            Queue<TNode> open = new Queue<TNode>();
            HashSet<TNode> closed = new HashSet<TNode>();
            open.Enqueue(start);

            for (int i = 0; i <= iRange; ++i)
            {
                Queue<TNode> nextRipple = new Queue<TNode>();
                while (open.Count > 0)
                {
                    TNode node = open.Dequeue();
                    closed.Add(node);

                    // search the neighbors
                    foreach (TLink link in node.Links)
                    {
                        TNode neighbor = link.Target;
                        if (neighbor != null &&
                            !open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            nextRipple.Enqueue(neighbor);
                        }
                    }
                }
                open = nextRipple;
            }

            // goodies in here
            return closed;
        }

        public static List<TNode> FindPath_BreadthFirstSearch(TNode start, TNode goal)
        {
            // setup
            Queue<TNode> open = new Queue<TNode>();
            HashSet<TNode> closed = new HashSet<TNode>();
            open.Enqueue(start);
            Dictionary<TNode, TNode> parentLookup = new Dictionary<TNode, TNode>();

            // search / iteration
            while (open.Count > 0)
            {
                TNode current = open.Dequeue();
                closed.Add(current);

                // did we find the goal?
                if (current == goal)
                {
                    // construct path
                    List<TNode> path = new List<TNode>();
                    while (current != null)
                    {
                        path.Add(current);
                        current = parentLookup.ContainsKey(current) ? parentLookup[current] : null;
                    }
                    return path;
                }

                // search the neighbors
                foreach (TLink link in current.Links)
                {
                    TNode neighbor = link.Target;
                    if (neighbor != null &&
                        !open.Contains(neighbor) &&
                        !closed.Contains(neighbor))
                    {
                        open.Enqueue(neighbor);
                        parentLookup[neighbor] = current;
                    }
                }
            }

            // no path :(
            return null;
        }

        private static NodeWrapper<TNode> GetNode(TNode node, Dictionary<TNode, NodeWrapper<TNode>> wrapperLookup)
        {
            NodeWrapper<TNode> wrapper;
            if (!wrapperLookup.TryGetValue(node, out wrapper))
            {
                wrapper = new NodeWrapper<TNode>(node);
                wrapperLookup[node] = wrapper;
            }

            return wrapper;
        }

        static float GetNodeDistance(TNode A, TNode B)
        {
            
            if (A is IPositionNode<TLink, TNode> nA && B is IPositionNode<TLink, TNode> nB)
            {
                return Vector3.Distance(nA.WorldPosition, nB.WorldPosition);
            }

            return A != B ? 1.0f : 0.0f;
        }

        static float GetNodeDistance(IGraph<TNode, TLink> graph, TNode A, TNode B)
        {
            if (graph is ISearchableGraph<TNode, TLink> searchableGraph)
            {
                return searchableGraph.Heuristic(A, B);
            }

            return GetNodeDistance(A, B);
        }

        public static List<TNode> FindShortestPath_Dijkstra(TNode start, TNode goal)
        {
            // setup
            Dictionary<TNode, NodeWrapper<TNode>> wrapperLookup = new Dictionary<TNode, NodeWrapper<TNode>>();
            List<NodeWrapper<TNode>> open = new List<NodeWrapper<TNode>>();
            HashSet<NodeWrapper<TNode>> closed = new HashSet<NodeWrapper<TNode>>();
            NodeWrapper<TNode> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0)
            {
                // find node with smallest distance from start
                NodeWrapper<TNode> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fDistance < current.m_fDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // did we find the goal?
                if (current.m_node == goal)
                {
                    // construct path
                    List<TNode> path = new List<TNode>();
                    while (current != null)
                    {
                        path.Add(current.m_node);
                        current = current.m_parent;
                    }

                    return path;
                }

                // search the neighbors
                foreach (ILink<TNode, TLink> link in current.m_node.Links)
                {
                    TNode target = link.Target;
                    {
                        NodeWrapper<TNode> neighbor = GetNode(target, wrapperLookup);
                        float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);

                        // investigate neighbor?
                        if (!open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            open.Add(neighbor);
                            neighbor.m_parent = current;
                        }

                        // update parent?
                        if (fNewDistance < neighbor.m_fDistance)
                        {
                            neighbor.m_fDistance = fNewDistance;
                            neighbor.m_parent = current;
                        }
                    }
                }
            }

            // no path :(
            return null;
        }

        public static Dictionary<TNode, TNode> CalculateShortestPathTree(TNode start)
        {
            // setup
            Dictionary<TNode, NodeWrapper<TNode>> wrapperLookup = new Dictionary<TNode, NodeWrapper<TNode>>();
            List<NodeWrapper<TNode>> open = new List<NodeWrapper<TNode>>();
            HashSet<NodeWrapper<TNode>> closed = new HashSet<NodeWrapper<TNode>>();
            NodeWrapper<TNode> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0)
            {
                // find node with smallest distance from start
                NodeWrapper<TNode> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fDistance < current.m_fDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // search the neighbors
                foreach (var link in current.m_node.Links)
                {
                    if (link.Target is TNode target)
                    {
                        NodeWrapper<TNode> neighbor = GetNode(target, wrapperLookup);
                        float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);

                        // investigate neighbor?
                        if (!open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            open.Add(neighbor);
                            neighbor.m_parent = current;
                        }

                        // update parent?
                        if (fNewDistance < neighbor.m_fDistance)
                        {
                            neighbor.m_fDistance = fNewDistance;
                            neighbor.m_parent = current;
                        }
                    }
                }
            }

            // create result
            Dictionary<TNode, TNode> parentLookup = new Dictionary<TNode, TNode>();
            foreach (NodeWrapper<TNode> node in closed)
            {
                if (node.m_parent != null)
                {
                    parentLookup[node.m_node] = node.m_parent.m_node;
                }
            }

            return parentLookup;
        }

        public static List<TNode> FindShortestPath_AStar(IGraph<TNode, TLink> graph, TNode start, TNode goal, LinkEvaluator linkEvaluator = null)
        {
            // setup
            Dictionary<TNode, NodeWrapper<TNode>> wrapperLookup = new Dictionary<TNode, NodeWrapper<TNode>>();
            List<NodeWrapper<TNode>> open = new List<NodeWrapper<TNode>>();
            HashSet<NodeWrapper<TNode>> closed = new HashSet<NodeWrapper<TNode>>();
            NodeWrapper<TNode> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            startNode.m_fRemainingDistance = GetNodeDistance(graph, start, goal);
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0 && open.Count < 5000)
            {
                // find node with smallest remaining distance from start
                NodeWrapper<TNode> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fRemainingDistance < current.m_fRemainingDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // did we find the goal?
                if (current.m_node.Equals(goal))
                {
                    // construct path
                    List<TNode> path = new List<TNode>();
                    while (current != null)
                    {
                        path.Add(current.m_node);
                        current = current.m_parent;
                    }

                    path.Reverse();
                    return path;
                }

                // search the neighbors
                foreach (TLink link in current.m_node.Links)
                {
                    // got link evaluator?
                    if (linkEvaluator != null &&
                        !linkEvaluator(link))
                    {
                        continue;
                    }

                    TNode target = link.Target;
                    if (target == null)
                    {
                        continue;
                    }

                    NodeWrapper<TNode> neighbor = GetNode(target, wrapperLookup);
                    float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);
                    float fNewRemainingDistance = fNewDistance + GetNodeDistance(graph, target, goal);

                    // investigate neighbor?
                    if (!open.Contains(neighbor) &&
                        !closed.Contains(neighbor))
                    {
                        open.Add(neighbor);
                        neighbor.m_parent = current;
                    }

                    // update parent?
                    if (fNewRemainingDistance < neighbor.m_fRemainingDistance)
                    {
                        neighbor.m_fDistance = fNewDistance;
                        neighbor.m_fRemainingDistance = fNewRemainingDistance;
                        neighbor.m_parent = current;
                    }
                }
            }

            // no path :(
            return null;
        }
    }
}