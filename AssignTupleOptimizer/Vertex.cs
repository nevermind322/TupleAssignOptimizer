using System;

namespace AssignTupleOptimizer
{
    public class Vertex : IEquatable<Vertex>
    {
        public readonly string label;
        public int? number = null;

        public Vertex(string label)
        {
            this.label = label;
        }


        public enum Color
        {
            WHITE,
            GREY,
            BLACK
        }

        public Color color = Color.WHITE;
        public void resetColor() => color = Color.WHITE;

        public bool Equals(Vertex other)
        {
            if (other is null) return false;

            return label == other.label;
        }

        public override int GetHashCode()
        {
            return label.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (GetType() != other.GetType()) return false;
            return Equals((Vertex)other);
        }


        public override string ToString()
        {
            string n = "";
            if (number != null)
            {
                n = "(" + number.ToString() + ")";
            }

            return label + n;
        }
    }
}