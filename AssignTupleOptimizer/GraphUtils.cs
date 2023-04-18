using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace AssignTupleOptimizer
{
    using AdjStructure = Dictionary<Vertex, List<Vertex>>;

    public static class GraphUtils
    {
        static string tab = "";

        static void incTab() => tab += "    ";

        static void decTab() => tab = tab.Substring(0, tab.Length - 4);

        static void writeTab() => Console.Write(tab);

        static public void dfs(this AssignGraph graph,
            Action<AssignGraph> OnEnterGraph,
            Func<AssignGraph, Vertex, bool> OnProcessVertex,
            Action<AssignGraph, Vertex> OnEnterNode,
            Func<AssignGraph, Vertex, Vertex, bool> OnProcessNodeChild,
            Action<AssignGraph, Vertex> OnLeaveNode,
            Action<AssignGraph> OnLeaveGraph,
            Func<AssignGraph, Vertex, IEnumerable<Vertex>> vertexTraversalOrderFunc = null,
            IEnumerable<Vertex> graphTraversalOrder = null)
        {
            void dfs(Vertex start_node,
                Action<AssignGraph, Vertex> OnEnter,
                Func<AssignGraph, Vertex, Vertex, bool> OnProcessChild,
                Action<AssignGraph, Vertex> OnLeave
            )
            {
                OnEnter(graph, start_node);
                IEnumerable<Vertex> vertexTraversalOrder;
                if (vertexTraversalOrderFunc == null) vertexTraversalOrder = graph.OutAdjStructure[start_node];
                else vertexTraversalOrder = vertexTraversalOrderFunc(graph, start_node);
                foreach (Vertex current_node in vertexTraversalOrder)
                {
                    if (OnProcessChild(graph, start_node, current_node))
                        dfs(current_node, OnEnter, OnProcessChild, OnLeave);
                }

                OnLeave(graph, start_node);
            }


            if (graphTraversalOrder == null) graphTraversalOrder = graph.vertexes;

            OnEnterGraph(graph);
            foreach (Vertex v in graphTraversalOrder)
            {
                if (OnProcessVertex(graph, v))
                    dfs(v, OnEnter: OnEnterNode, OnProcessChild: OnProcessNodeChild, OnLeave: OnLeaveNode);
            }

            OnLeaveGraph(graph);
        }

        public static AssignGraph createAssignGraph(List<Symbol> left, List<Symbol> right)
        {
            List<SymbolVertex> v_left = new List<SymbolVertex>();
            List<SymbolVertex> v_right = new List<SymbolVertex>();

            for (int i = 0; i < left.Count(); i++)
            {
                if (left.Count(it => it.Equals(left[i])) > 1 || left[i].Equals(right[i]))
                {
                    left.RemoveAt(i);
                    right.RemoveAt(i);
                    i--;
                }
            }

            List<Edge> assign_first = new List<Edge>();
            List<Edge> assign_last = new List<Edge>();

            int leftFromOuterCount = left.Count(symbol => symbol.fromOuterScope);
            int rightFromOuterCount = right.Count(symbol => symbol.fromOuterScope);
            
            string temp_name;
            Symbol temp_symbol;

            for (int i = 0; i < left.Count(); i++)
            {
               if (right[i].isExpr)
               {
                    temp_name = "$" + i + "_temp";
                    temp_symbol = new Symbol(temp_name);
                    assign_first.Add(new Edge(new SymbolVertex(right[i]),
                        new SymbolVertex(temp_symbol)));
                    right[i] = temp_symbol;
                }
            }

            if (leftFromOuterCount > 0 && rightFromOuterCount > 0)
            {
                if (leftFromOuterCount < rightFromOuterCount)
                {
                    

                    for (int i = 0; i < left.Count; i++)
                    {
                        if (left[i].fromOuterScope)
                        {
                            if (left.Contains(right[i]) || right[i].fromOuterScope)
                            {
                                temp_name = "$" + left[i].name + "_temp";
                                temp_symbol = new Symbol(temp_name);
                                assign_last.Add(new Edge(new SymbolVertex(temp_symbol),
                                    new SymbolVertex(left[i])));
                                left[i] = temp_symbol;
                            }
                            else
                            {
                                var to = new SymbolVertex(left[i]);
                                var from =new SymbolVertex( right[i]);
                                left.RemoveAt(i);
                                right.RemoveAt(i);
                                i--;
                                
                                var  unnecessaryAssign = assign_last.Find(assign => assign.to.Equals(to));
                                if (unnecessaryAssign != null) assign_last.Remove(unnecessaryAssign);
                                
                                assign_last.Add(new Edge(from,to));
                            }
                        }
                    }
                }
                else
                {
           
                    for (int i = 0; i < left.Count; i++)
                    {
                        if (right[i].fromOuterScope)
                        {
                            if (right.Contains(left[i]) || left[i].fromOuterScope)
                            {
                                temp_name = "$" + right[i].name + "_temp";
                                temp_symbol = new Symbol(temp_name);
                                assign_first.Add(new Edge(new SymbolVertex(right[i]),
                                    new SymbolVertex(temp_symbol)));
                                right[i] = temp_symbol;
                            }
                            else
                            {
                                var to = new SymbolVertex(left[i]);
                                var from =new SymbolVertex( right[i]);
                                left.RemoveAt(i);
                                right.RemoveAt(i);
                                i--;
                                
                                var  unnecessaryAssign = assign_first.Find(assign => assign.to.Equals(to));
                                if (unnecessaryAssign != null) assign_first.Remove(unnecessaryAssign);
                                
                                assign_first.Add(new Edge(from,to));
                            }
                        }
                    }
                }
            }

            foreach (Symbol sym in left)
            {
                SymbolVertex s = v_left.Find(vertex => vertex.symbol.Equals(sym));
                if (s != null)
                {
                    v_left.Add(s);
                }
                else v_left.Add(new SymbolVertex(sym));
            }

            foreach (Symbol sym in right)
            {
                SymbolVertex s = v_left.Find(vertex => vertex.symbol.Equals(sym));
                if (s != null)
                {
                    v_right.Add(s);
                }
                else
                {
                    SymbolVertex s2 = v_right.Find(vertex => vertex.symbol.Equals(sym));
                    if (s2 != null)
                    {
                        v_right.Add(s);
                    }
                    else v_right.Add(new SymbolVertex(sym));
                }
            }

            List<Edge> assigns = new List<Edge>();

            Edge cur_assign;

            for (int i = 0; i < v_left.Count; i++)
            {
                cur_assign = new Edge(v_right[i], v_left[i], i);
                assigns.Add(cur_assign);
            }

            var graph = new AssignGraph(assigns);
            graph.assignFirst.AddRange(assign_first);
            graph.assignLast.AddRange(assign_last);
            return graph;
        }

        public static AssignGraph createReversedGraph(this AssignGraph g)
        {
            List<Edge> newEdges = new List<Edge>();
            foreach (var edge in g.edges)
            {
                newEdges.Add(new Edge(f: edge.to, t: edge.from));
            }

            return new AssignGraph(newEdges, g.vertexes.ToList());
        }

        public static List<List<Vertex>> findStronglyConnectedComponets(this AssignGraph g)
        {
            List<List<Vertex>> res = new List<List<Vertex>>();

            Stack<Vertex> route = new Stack<Vertex>();
            HashSet<Vertex> visited = new HashSet<Vertex>();

            Action<AssignGraph> onEnterGraph = (_) =>
            {
                route.Clear();
                visited.Clear();
            };

            Action<AssignGraph, Vertex> onEnterNode = (graph, node) => { visited.Add(node); };

            Action<AssignGraph, Vertex> onLeaveNode = (graph, node) => { route.Push(node); };

            Func<AssignGraph, Vertex, Vertex, bool> onProcessChildNode = (graph, parent, childNode) =>
            {
                if (!visited.Contains(childNode))
                {
                    return true;
                }
                else return false;
            };

            Func<AssignGraph, Vertex, bool> onProcessVertex = (graph, vert) => { return !visited.Contains(vert); };

            g.dfs(OnEnterGraph: onEnterGraph, OnProcessVertex: onProcessVertex,
                OnEnterNode: onEnterNode, OnProcessNodeChild: onProcessChildNode,
                OnLeaveNode: onLeaveNode,
                OnLeaveGraph: (_) => { }
            );

            Action<AssignGraph> onEnterGraphReversed = (_) => { visited.Clear(); };

            Action<AssignGraph, Vertex> onEnterNodeReversed = (graph, node) =>
            {
                visited.Add(node);
                res.Last().Add(node);
            };

            Action<AssignGraph, Vertex> onLeaveNodeReversed = (graph, node) => { };

            Func<AssignGraph, Vertex, Vertex, bool> onProcessChildNodeReversed = (graph, parent, childNode) =>
            {
                if (!visited.Contains(childNode))
                {
                    return true;
                }
                else return false;
            };

            Func<AssignGraph, Vertex, bool> onProcessVertexReversed = (graph, vert) =>
            {
                if (visited.Contains(vert)) return false;
                else
                {
                    res.Add(new List<Vertex>());
                    return true;
                }
            };

            var graphTraversalOrder = route.ToList();

            var g_rev = g.createReversedGraph();

            g_rev.dfs(OnEnterGraph: onEnterGraphReversed, OnProcessVertex: onProcessVertexReversed,
                OnEnterNode: onEnterNodeReversed, OnProcessNodeChild: onProcessChildNodeReversed,
                OnLeaveNode: onLeaveNodeReversed,
                OnLeaveGraph: (_) => { },
                graphTraversalOrder: graphTraversalOrder);

            return res;
        }

        public static List<Cycle> findAllUniqueElementaryCycles(this AssignGraph g)
        {
            List<Cycle> cycles = new List<Cycle>();

            Stack<Vertex> stack = new Stack<Vertex>();

            Stack<bool> foundCycle = new Stack<bool>();
            foundCycle.Push(false);

            AdjStructure blockedMap = new AdjStructure();
            HashSet<Vertex> blockedSet = new HashSet<Vertex>();

            Vertex start = null;
            int startIndex = 1;
            int i = 1;

            Action<AssignGraph> onEnterGraph =
                (graph) =>
                {
                    blockedMap.Clear();
                    blockedSet.Clear();
                    Console.WriteLine("Cleared blockSet and blockMap");
                };

            Action<AssignGraph, Vertex> onEnterNode =
                (graph, node) =>
                {
                    stack.Push(node);
                    foundCycle.Push(false);
                    blockedSet.Add(node);
                    incTab();
                    writeTab();
                    Console.Write("Entering {0}, current stack |", node);
                    stack.Reverse().ToList().ForEach(vert => Console.Write("{0}->", vert));
                    Console.WriteLine();
                    writeTab();
                    Console.Write("blocking node:{0}, blocked set |", node);
                    foreach (var vert in blockedSet) Console.Write("{0},", vert);
                    Console.WriteLine();
                };


            Func<AssignGraph, Vertex, Vertex, bool> onProcessNodeChild = (graph, startNode, node) =>
            {
                writeTab();
                Console.WriteLine("Considering visiting node:{0}", node);
                if (start == node)
                {
                    Cycle cycle = new Cycle(stack.Reverse().ToList());
                    writeTab();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Found cycle(start node:{0}, current node:{1}): {2}", start, node, cycle);
                    Console.ResetColor();
                    cycles.Add(cycle);
                    foundCycle.Pop();
                    foundCycle.Push(true);
                    return false;
                }
                else if (!blockedSet.Contains(node))
                {
                    writeTab();
                    Console.WriteLine("Going to visit node:{0} since it is not in blocked set", node);
                    return true;
                }
                else
                {
                    writeTab();
                    Console.WriteLine("Not going to visit node:{0} since it is in blocked set", node);
                    return false;
                }
            };


            Action<AssignGraph, Vertex> onLeaveNode = (graph, node) =>
            {
                if (foundCycle.Pop())
                {
                    foundCycle.Pop();
                    foundCycle.Push(true);
                    writeTab();
                    Console.WriteLine("going to unblock node:{0} since cycle has been found", node);
                    unblock(node);
                }
                else
                {
                    writeTab();
                    Console.WriteLine("not going to unblock node:{0} since no cycles has been found", node);
                    foreach (var ver in graph.OutAdjStructure[node])
                    {
                        if (blockedMap.ContainsKey(ver))
                        {
                            blockedMap[ver].Add(node);
                        }
                        else blockedMap.Add(ver, new List<Vertex>() { node });

                        writeTab();
                        Console.WriteLine("i need to unblock node:{0} when node:{1} is unblocked", node, ver);
                    }
                }

                stack.Pop();
                writeTab();
                Console.Write("Leaving {0}, current stack |", node);
                stack.Reverse().ToList().ForEach(vert => Console.Write("{0}->", vert));
                Console.WriteLine();
                decTab();
            };


            void unblock(Vertex u)
            {
                writeTab();

                Console.WriteLine("unblocking node:{0}", u);
                blockedSet.Remove(u);
                if (blockedMap.ContainsKey(u))
                {
                    writeTab();

                    Console.WriteLine("there are nodes that needs to be unblocked since node:{0} has been unblocked");
                    blockedMap[u].ForEach(v =>
                    {
                        if (blockedSet.Contains(v))
                        {
                            writeTab();

                            Console.WriteLine("Going to unblock node:{0} since node:{0} has been unblocked", v, u);
                            unblock(v);
                        }
                    });
                    writeTab();
                    Console.WriteLine("Removing node:{0} from blocked set", u);
                    blockedMap.Remove(u);
                }
            }


            foreach (Vertex v in g.vertexes)
            {
                v.number = i;
                i++;
            }

            while (startIndex <= g.vertexes.Count())
            {
                AssignGraph subGraph = getInducedGraph(g, startIndex);
                Console.WriteLine("Working with subgraph: ");
                subGraph.LogGraph();
                Console.WriteLine();

                List<List<Vertex>> stronglyConnectedComponents = subGraph.findStronglyConnectedComponets();
                Console.WriteLine("Found components: ");
                stronglyConnectedComponents.ForEach(component =>
                {
                    Console.Write("Component: ");
                    component.ForEach(vert => Console.Write("{0}, ", vert));
                    Console.WriteLine();
                });
                Console.WriteLine();

                Vertex leastVertex = findLeastVertex(stronglyConnectedComponents);
                Console.WriteLine("Found least vertex {0}", leastVertex);
                Console.WriteLine();

                List<Vertex> componentWithLeastVertex =
                    stronglyConnectedComponents.Find(component => component.Contains(leastVertex));
                AssignGraph graphWithLeastVertex = subGraph.getInducedGraph(componentWithLeastVertex);
                Console.WriteLine("Graph with least vertex: ");
                graphWithLeastVertex.LogGraph();
                Console.WriteLine();

                if (graphWithLeastVertex.vertexes.Count > 1)
                {
                    Console.WriteLine("Start - {0}", leastVertex);
                    start = leastVertex;
                    graphWithLeastVertex.dfs(OnEnterGraph: onEnterGraph, OnProcessVertex: (_, __) =>
                        {
                            Console.WriteLine("starting at {0}", __);
                            return true;
                        },
                        OnEnterNode: onEnterNode, OnProcessNodeChild: onProcessNodeChild,
                        OnLeaveNode: onLeaveNode, OnLeaveGraph: (_) => { },
                        graphTraversalOrder: new List<Vertex>() { start });
                }

                startIndex = (int)(leastVertex.number + 1);
                Console.WriteLine();
            }

            return cycles;
        }

        public static void LogGraph(this AssignGraph graph)
        {
            foreach (var vert in graph.vertexes)
            {
                Console.Write("{0}->[", vert);
                graph.OutAdjStructure[vert].ForEach(v => Console.Write("{0}, ", v));
                Console.WriteLine("]");
            }
        }

        public static AssignGraph getInducedGraph(this AssignGraph graph, List<Vertex> vertexes)
        {
            var new_edges = new List<Edge>();
            foreach (Edge edge in graph.edges)
            {
                if (vertexes.Contains(edge.from) && vertexes.Contains(edge.to))
                    new_edges.Add(edge);
            }

            return new AssignGraph(new_edges, vertexes);
        }

        public static AssignGraph getInducedGraph(this AssignGraph graph, int startVertex)
        {
            List<Vertex> newVertixes = new List<Vertex>();
            foreach (var vert in graph.vertexes)
                if (vert.number >= startVertex)
                    newVertixes.Add(vert);
            return graph.getInducedGraph(newVertixes);
        }

        public static Vertex findLeastVertex(this AssignGraph g)
        {
            Vertex leastVertex = null;
            foreach (var vert in g.vertexes)
            {
                if (vert.number == null) throw new InvalidOperationException("not numbered graph");
                if (leastVertex == null) leastVertex = vert;
                else if (vert.number < leastVertex.number) leastVertex = vert;
            }

            return leastVertex;
        }

        public static Vertex findLeastVertex(List<AssignGraph> components)
        {
            Vertex leastVertex = null;
            foreach (var component in components)
            {
                foreach (var vert in component.vertexes)
                {
                    if (vert.number == null) throw new InvalidOperationException("not numbered graph");
                    if (leastVertex == null) leastVertex = vert;
                    else if (vert.number < leastVertex.number) leastVertex = vert;
                }
            }

            return leastVertex;
        }

        public static Vertex findLeastVertex(List<List<Vertex>> components)
        {
            Vertex leastVertex = null;
            foreach (var component in components)
            {
                foreach (var vert in component)
                {
                    if (vert.number == null) throw new InvalidOperationException("not numbered graph");
                    if (leastVertex == null) leastVertex = vert;
                    else if (vert.number < leastVertex.number) leastVertex = vert;
                }
            }

            return leastVertex;
        }

        public static void drawGraph(this AssignGraph g)
        {
            var writer = File.CreateText("graph.dot");
            writer.WriteLine("digraph _graph {");

            Action<AssignGraph> enterGraph = (graph) => { graph.resetVertexesColor(); };

            Func<AssignGraph, Vertex, bool> ProcessNode = (graph, node) => node.color != Vertex.Color.GREY;

            Action < AssignGraph, Vertex > EnterNode = (graph, vert) => { vert.color = Vertex.Color.GREY; };

            Func<AssignGraph, Vertex, Vertex, bool> ProcessNodeChild = (graph, parent, child) =>
            {
                writer.WriteLine(parent.label + "->" + child.label);
                return child.color == Vertex.Color.WHITE;
            };

            g.dfs(OnEnterGraph: enterGraph, OnEnterNode: EnterNode, OnProcessNodeChild: ProcessNodeChild,
                OnProcessVertex: ProcessNode, OnLeaveGraph: (graph) => {graph.resetVertexesColor(); }, OnLeaveNode: (graph, vertex) => { });
            writer.WriteLine("}");
            writer.Close();
        }
    }
}