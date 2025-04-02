using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Nodes
    {
        protected NodeState state;

        public Nodes parent;
        protected List<Nodes> children = new List<Nodes>();

        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

        public Nodes()
        {
            parent = null;
        }
        public Nodes(List<Nodes> children)
        {
            foreach (Nodes child in children)
            {
                _Attach(child);
            }
        }

        private void _Attach(Nodes node)
        {
            node.parent = this;
            children.Add(node);
        }
        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public object GetData(string key) 
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value))
                return value;
            Nodes node = parent;
            while (node != null) 
            {
                value = node.GetData(key);
                if (value != null) return value;
                node = node.parent;
            }
            return null;
        }
        public bool ClearData(string key) 
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true; 
            }
            Nodes node = parent;
            while (node != null) 
            {
                bool cleared = node.ClearData(key);
                if (cleared) return true;
                node = node.parent;
            }
            return false;
        }
    }

}

