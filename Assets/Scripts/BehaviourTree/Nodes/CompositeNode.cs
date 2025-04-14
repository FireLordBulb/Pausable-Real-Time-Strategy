using System.Collections.Generic;

namespace BehaviourTree.Nodes {
	public abstract class CompositeNode : Node {
		public List<Node> children = new();
		
		public override Node Clone(){
			CompositeNode clone = (CompositeNode)base.Clone();
			clone.children = children.ConvertAll(c => c.Clone());
			return clone;
		}
		public void SortChildren(){
			children.Sort((a, b) => a.position.x < b.position.x ? -1 : 1);
		}
	}
}
