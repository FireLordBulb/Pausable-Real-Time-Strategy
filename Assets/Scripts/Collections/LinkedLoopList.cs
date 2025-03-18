using System.Collections.Generic;

namespace Collections {
	public class LinkedLoopList<T> {
		public readonly Node<T> First;
		public readonly Node<T> Last;
		public LinkedLoopList(IEnumerable<T> enumerable){
			Node<T> previous = null;
			foreach (T value in enumerable){
				Node<T> node = new Node<T>{Value = value};
				if (previous == null){
					First = node;
				} else {
					previous.Next = node;
				}
				previous = node;
			}
			if (previous == null){
				return;
			}
			Last = previous;
			Last.Next = First;
		}
		private LinkedLoopList(Node<T> firstNode, Node<T> lastNode){
			First = firstNode;
			Last = lastNode;
		}
		public (LinkedLoopList<T>, LinkedLoopList<T>) Split(Node<T> beforeStart, Node<T> splitStart, Node<T> splitEnd){
			LinkedLoopList<T> left  = new(splitStart   , splitEnd       );
			LinkedLoopList<T> right = new(splitEnd.Copy, splitStart.Copy);
		
			left.Last.Next = left.First;

			beforeStart.Next = right.Last;
			right.Last.Next = right.First;

			return (left, right);
		}

		public class Node<TT> {
			public Node<TT> Next {get; internal set;}
			public TT Value {get; internal set;}
			public Node<TT> Copy => new(){Next = Next, Value = Value};

			public IEnumerable<Node<TT>> LoopUntilNextIs(Node<TT> disallowedNext){
				Node<TT> node = this;
				while (node.Next != disallowedNext){
					yield return node;
					node = node.Next;
				}
			}
		}
	}
}