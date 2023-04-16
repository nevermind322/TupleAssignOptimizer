using System;

namespace AssignTupleOptimizer
{
    public class Edge : IEquatable<Edge>, IComparable<Edge>
    {
        public Vertex from;
        public Vertex to;
        public int weight;

        public Edge(Vertex f, Vertex t, int w = 0)
        {
            if (weight < 0) throw new ArgumentException("weight can't be negative!");
            from = f;
            to = t;
            weight = w;
        }

        public int CompareTo(Edge other)
        {
            return weight - other.weight;
        }

        public bool Equals(Edge other)
        {
            if (other is null) return false;
            return from == other.from && to == other.to;
        }
    }
}