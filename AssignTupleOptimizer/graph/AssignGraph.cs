using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssignTupleOptimizer
{
    public class AssignGraph : IEnumerable<Vertex>
    {
        public readonly HashSet<Vertex> vertexes = new HashSet<Vertex>();

        public readonly List<Edge> edges = new List<Edge>();
        private bool AdjStructuresNeedSync = false;

        public List<Edge> assignOrder = new List<Edge>();
        public List<Edge> assignLast  = new List<Edge>();
        public List<Edge> assignFirst = new List<Edge>();


        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
            vertexes.Add(edge.from);
            vertexes.Add(edge.to);
            AdjStructuresNeedSync = true;
        }

        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
            AdjStructuresNeedSync = true;
        }


        private Dictionary<Vertex, List<Vertex>> outAdjStructure = null;

        public Dictionary<Vertex, List<Vertex>> OutAdjStructure
        {
            get
            {
                if (outAdjStructure == null || AdjStructuresNeedSync)
                {
                    outAdjStructure = createOutAdjStructure();
                    inAdjStructure = createInAdjStructure();
                    AdjStructuresNeedSync = false;
                }

                return outAdjStructure;
            }
        }

        private Dictionary<Vertex, List<Vertex>> createOutAdjStructure()
        {
            Dictionary<Vertex, List<Vertex>> res = new Dictionary<Vertex, List<Vertex>>();

            foreach (var vert in vertexes) res.Add(vert, new List<Vertex>());

            foreach (Edge edge in edges) res[edge.from].Add(edge.to);

            return res;
        }

        private Dictionary<Vertex, List<Vertex>> inAdjStructure = null;

        public Dictionary<Vertex, List<Vertex>> InAdjStructure
        {
            get
            {
                if (inAdjStructure == null || AdjStructuresNeedSync)
                {
                    outAdjStructure = createOutAdjStructure();
                    inAdjStructure = createInAdjStructure();
                    AdjStructuresNeedSync = false;
                }

                return inAdjStructure;
            }
        }

        private Dictionary<Vertex, List<Vertex>> createInAdjStructure()
        {
            Dictionary<Vertex, List<Vertex>> res = new Dictionary<Vertex, List<Vertex>>();

            foreach (var vert in vertexes) res.Add(vert, new List<Vertex>());

            foreach (Edge edge in edges) res[edge.to].Add(edge.from);

            return res;
        }


        public List<Edge> GetInEdgesForVertex(Vertex v) => edges.FindAll(edge => edge.to == v);
        public List<Edge> GetOutEdgesForVertex(Vertex v) => edges.FindAll(edge => edge.from == v);

        public void resetVertexesColor()
        {
            foreach (Vertex v in vertexes) v.resetColor();
        }

        public void DeleteUnnecessaryEdges()
        {
            foreach (var vertex in vertexes)
            {
                var inEdges = GetInEdgesForVertex(vertex);
                if (inEdges.Count > 1)
                {
                    Edge last_assign = inEdges.Max();
                    foreach (var edge in inEdges)
                        if (edge != last_assign) RemoveEdge(edge);
                        
                }
            }
        }

        public bool EnsureThatEveryVertexHasOneOrZeroInEdge() =>
            vertexes.All(vert => GetInEdgesForVertex(vert).Count <= 1);

        public List<Edge> GetAssignOrder()
        {
            List<Edge> assignOrder = new List<Edge>();
            assignOrder.AddRange(assignFirst);
            var s = vertexes.First() as SymbolVertex;

            DeleteUnnecessaryEdges();

            var cycles = this.findAllUniqueElementaryCycles();

            foreach (var cycle in cycles)
            {
                Vertex cut_place = cycle[0];
                Edge edge_to_cut = GetInEdgesForVertex(cut_place).First();

                Vertex temp_assign_from = edge_to_cut.from;
                string temp_var_name = "$" + temp_assign_from.label + "_temp";
                Vertex temp_vertex = new SymbolVertex(new Symbol(temp_var_name));

                assignOrder.Add(new Edge(temp_assign_from, temp_vertex));
                assignLast.Add(new Edge(temp_vertex, cut_place));
                RemoveEdge(edge_to_cut);
            }

            List<Vertex> roots = vertexes.ToList().FindAll(vertex => GetInEdgesForVertex(vertex).Count == 0);

            void treeTraversal(Vertex root, Action<Vertex> onLeave)
            {
                foreach (Vertex node in OutAdjStructure[root])
                {
                    treeTraversal(node, onLeave);
                }

                onLeave(root);
            }

            foreach (Vertex root in roots)
            {
                treeTraversal(root,
                    vertex =>
                    {
                        if (vertex != root) assignOrder.Add(GetInEdgesForVertex(vertex).First());
                    });
            }

            assignOrder.AddRange(assignLast);
            this.assignOrder = assignOrder;
            return this.assignOrder;
        }

        public AssignGraph()
        {
        }

        public AssignGraph(List<Edge> edges, List<Vertex> vertexes) : this(edges)
        {
            foreach (Vertex vert in vertexes) this.vertexes.Add(vert);
        }

        public AssignGraph(List<Edge> e)
        {
            edges.AddRange(e);
            foreach (var edge in edges)
            {
                vertexes.Add(edge.from);
                vertexes.Add(edge.to);
            }
        }

        public AssignGraph(List<Vertex> l)
        {
            vertexes = new HashSet<Vertex>(l);
        }

        public AssignGraph(params Vertex[] v)
        {
            foreach (var vert in v)
            {
                _addVertex(vert);
            }
        }

        void _addVertex(Vertex v)
        {
            vertexes.Add(v);
        }

        public void AddVertexes(params Vertex[] v)
        {
            foreach (var vert in v)
            {
                _addVertex(vert);
            }
        }


        public IEnumerator<Vertex> GetEnumerator()
        {
            return ((IEnumerable<Vertex>)vertexes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)vertexes).GetEnumerator();
        }
    }
}