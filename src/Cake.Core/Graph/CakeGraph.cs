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
            private static int index_ctr = 0;

			public string name;

			public int index;
			public int min_index;
			public int up_count;

            // Implement a doubly-linked directed graph.
            // Up and Down are chosen for "colors" based on the visual
            // interpretation of a Hasse diagram
			public List<CakeGraphNode> up_out;
			public List<CakeGraphNode> up_in;
			public List<CakeGraphNode> dn_out;
			public List<CakeGraphNode> dn_in;

            public CakeGraphNode(string name)
            {
                this.name = name;
                index = min_index = up_count = 0;
            }

            // Labels nodes with Tarjan's strongly connected components algorithm
            // 
            // After execution, if index == min_index for all nodes, there are no
            // cycles in the graph. Down Out and Up In are used, because it is
            // checking for dependency cycles, so Up edges are considered to be
            // pointing in the opposite direction.
            public int Tarjan()
            {
                if (min_index == 0)
                {
                    index = min_index = ++index_ctr;

                    foreach (CakeGraphNode node in dn_out.Concat(up_in))
                    {
                        int temp = node.Tarjan();
                        if (temp < min_index)
                        {
                            min_index = temp;
                        }
					}
                }
                return min_index;
            }
        }

        private readonly List<CakeGraphNode> _nodes;

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
            if (_nodes.Any(x => x.name == node))
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

			if (_nodes.All(x => !x.name.Equals(start, StringComparison.OrdinalIgnoreCase)))
			{
				start_node = new CakeGraphNode(start);
				_nodes.Add(start_node);
			}
			else
			{
				start_node = _nodes.Find(x => x.name.Equals(start, StringComparison.OrdinalIgnoreCase));
			}

			if (_nodes.All(x => !x.name.Equals(end, StringComparison.OrdinalIgnoreCase)))
			{
				end_node = new CakeGraphNode(end);
				_nodes.Add(end_node);
			}
			else
			{
				end_node = _nodes.Find(x => x.name.Equals(end, StringComparison.OrdinalIgnoreCase));
			}

            if (pointsDown)
            {
                start_node.dn_out.Add(end_node);
                end_node.up_in.Add(start_node);
            }
            else
            {
                start_node.up_out.Add(end_node);
                end_node.dn_in.Add(start_node);
            }
        }

        public bool Exist(string name)
        {
            return _nodes.Any(x => x.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool ContainsCycles()
        {
            bool result = false;
            foreach (CakeGraphNode node in _nodes)
            {
                node.Tarjan();
                result |= node.index != node.min_index;
            }
            return result;
        }

        public IEnumerable<string> Traverse(string target)
        {
            if (!Exist(target))
            {
                return Enumerable.Empty<string>();
            }
            var result = new List<string>();

            return result;
        }
    }
}