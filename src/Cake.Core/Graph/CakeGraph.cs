// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cake.Core.Graph
{
    internal sealed class CakeGraph
    {
        internal sealed class CakeGraphNode
        {
            private static int indexCtr = 0;
            private static Stack<CakeGraphNode> stack = new Stack<CakeGraphNode>();

            internal string Name { get; private set; }

            internal int Index { get; private set; }

            internal int MinIndex { get; private set; }

            internal int DownCount { get; set; }

            // Implement a doubly-linked directed graph.
            // Up and Down are chosen for "colors" based on the visual
            // interpretation of a Hasse diagram
            internal List<CakeGraphNode> UpOut { get; }

            internal List<CakeGraphNode> UpIn { get; }

            internal List<CakeGraphNode> DownOut { get; }

            internal List<CakeGraphNode> DownIn { get; }

            internal CakeGraphNode(string name)
            {
                this.Name = name;
                Index = MinIndex = DownCount = 0;
                UpOut = new List<CakeGraphNode>();
                UpIn = new List<CakeGraphNode>();
                DownOut = new List<CakeGraphNode>();
                DownIn = new List<CakeGraphNode>();
            }

            // Labels nodes with Tarjan's strongly connected components algorithm
            //
            // After execution, if index == min_index for all nodes, there are no
            // cycles in the graph. Down Out and Up In are used, because it is
            // checking for dependency cycles, so Up edges are considered to be
            // pointing in the opposite direction.
            internal int Tarjan()
            {
                if (MinIndex == 0)
                {
                    stack.Push(this);

                    Index = MinIndex = ++indexCtr;

                    foreach (CakeGraphNode node in DownOut.Concat(UpIn).Where(x => stack.Contains(x) || x.Index == 0))
                    {
                        int temp = node.Tarjan();
                        if (temp < MinIndex)
                        {
                            MinIndex = temp;
                        }
                    }

                    stack.Pop();
                }
                return MinIndex;
            }
        }

        private readonly List<CakeGraphNode> _nodes;

        public IReadOnlyList<CakeGraphNode> Nodes => _nodes;

        public CakeGraph()
        {
            _nodes = new List<CakeGraphNode>();
        }

        public void Add(string node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (_nodes.Any(x => x.Name == node))
            {
                throw new CakeException("Node has already been added to graph.");
            }
            _nodes.Add(new CakeGraphNode(node));
        }

        public void Connect(string start, string end, bool pointsDown = true)
        {
            if (start.Equals(end, StringComparison.OrdinalIgnoreCase))
            {
                throw new CakeException("Reflexive edges in graph are not allowed.");
            }

            CakeGraphNode start_node;
            CakeGraphNode end_node;

            if (_nodes.All(x => !x.Name.Equals(start, StringComparison.OrdinalIgnoreCase)))
            {
                start_node = new CakeGraphNode(start);
                _nodes.Add(start_node);
            }
            else
            {
                start_node = _nodes.Find(x => x.Name.Equals(start, StringComparison.OrdinalIgnoreCase));
            }

            if (_nodes.All(x => !x.Name.Equals(end, StringComparison.OrdinalIgnoreCase)))
            {
                end_node = new CakeGraphNode(end);
                _nodes.Add(end_node);
            }
            else
            {
                end_node = _nodes.Find(x => x.Name.Equals(end, StringComparison.OrdinalIgnoreCase));
            }

            if (pointsDown)
            {
                start_node.DownOut.Add(end_node);
                end_node.DownIn.Add(start_node);
            }
            else
            {
                start_node.UpOut.Add(end_node);
                end_node.UpIn.Add(start_node);
            }
        }

        public bool Exist(string name)
        {
            return _nodes.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool ContainsCycles()
        {
            bool result = false;
            foreach (CakeGraphNode node in _nodes)
            {
                node.Tarjan();
                result |= node.Index != node.MinIndex;
            }
            return result;
        }

        // Generate a traversal list with two passes of the graph
        // The first pass follows the dependency graph, using a BFS to find all nodes
        // which must be executed for the execution of the target node.
        // The second pass uses a modifed DFS to convert the poset represented
        // by the subgraph into a total ordering for execution.
        public IEnumerable<string> Traverse(string target)
        {
            if (!Exist(target))
            {
                return Enumerable.Empty<string>();
            }

            var targetNode = _nodes.Find(x => x.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
            var poset = new List<CakeGraphNode>() { targetNode };

            for (int i = 0; i < poset.Count; i++)
            {
                foreach (CakeGraphNode node in poset[i].DownOut.Union(poset[i].UpOut))
                {
                    if (!poset.Contains(node))
                    {
                        node.DownCount = node.UpIn.Count + node.DownOut.Count + 1;
                        poset.Add(node);
                    }
                }
            }

            poset.ForEach(node => node.DownCount = node.UpIn.Union(node.DownOut).Intersect(poset).Count());
            var root = poset.Where(x => x.DownCount == 0).ToList();

            var result = new List<string>();

            foreach (CakeGraphNode curr in root)
            {
                result.Add(curr.Name);
                foreach (CakeGraphNode node in curr.UpOut.Concat(curr.DownIn))
                {
                    Traverse(node, result, poset);
                }
            }

            return result;
        }

        private void Traverse(CakeGraphNode curr, List<string> result, List<CakeGraphNode> poset)
        {
            if (--curr.DownCount == 0)
            {
                result.Add(curr.Name);

                foreach (CakeGraphNode node in curr.UpOut.Union(curr.DownIn).Intersect(poset))
                {
                    Traverse(node, result, poset);
                }
            }
        }
    }
}