using System.Collections;
using System.Collections.Generic;

namespace Graphs
{
    public abstract class Link : ILink<Node, Link>
    {
        private Node m_source;
        private Node m_target;

        #region Properties

        public Node Source => m_source;

        public Node Target => m_target;

        #endregion

        public Link(Node source, Node target)
        {
            m_source = source;
            m_target = target;
        }
    }
    public abstract class Node : INode<Link, Node>
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty // This is an abstract class. 
        public IEnumerable<Link> Links { get; }
    }
}