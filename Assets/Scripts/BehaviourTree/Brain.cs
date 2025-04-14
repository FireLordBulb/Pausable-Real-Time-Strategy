using UnityEngine;

namespace BehaviourTree
{
    public class Brain : MonoBehaviour
    {
        [SerializeField]
        private Tree m_tree;

        #region Properties

        public Tree Tree => m_tree;

        #endregion

        protected virtual void Start()
        {
            if (m_tree != null)
            {
                m_tree = m_tree.Clone();
                m_tree.StartTree(this);
            }
        }

        protected virtual void Update()
        {
            if (m_tree != null &&
                m_tree.CurrentState == Nodes.Node.State.Running)
            {
                m_tree.Update();
            }
        }
    }
}