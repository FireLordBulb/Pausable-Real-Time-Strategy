using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    public interface INode<out TLink, TNode> where TLink : ILink<TNode, TLink> where TNode :  INode<TLink, TNode> 
    {
        IEnumerable<TLink> Links { get; }
    }

    public interface IPositionNode<out TLink, TNode> : INode<TLink, TNode> where TLink : ILink<TNode, TLink> where TNode :  INode<TLink, TNode> 
    {
        Vector3 WorldPosition { get; }
    }
    public interface ILink<out TNode, TLink> where TNode : INode<TLink, TNode> where TLink : ILink<TNode, TLink>
    {
         TNode Source { get; }

         TNode Target { get; }
    }
    public interface IGraph<out TNode, TLink> where TNode : INode<TLink, TNode> where TLink : ILink<TNode, TLink>
    {
        IEnumerable<TNode> Nodes { get; }
    }

    public interface ISearchableGraph<TNode, TLink> : IGraph<TNode, TLink> where TNode : INode<TLink, TNode> where TLink : ILink<TNode, TLink>
    {
        float Heuristic(TNode start, TNode goal);
    }
}