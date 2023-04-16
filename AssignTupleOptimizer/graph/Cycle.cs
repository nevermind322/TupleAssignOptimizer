using System.Collections.Generic;

namespace AssignTupleOptimizer
{
    public class Cycle : System.IEquatable<Cycle>
    {
        List<Vertex> cycle;


        public int Len
        {
            get { return cycle.Count; }
        }


        public Cycle(List<Vertex> c)
        {
            cycle = new List<Vertex>(c);
        }

        public Vertex this[int i]
        {
            get { return cycle[i % Len]; }
        }


        public class CycleComparer : EqualityComparer<Cycle>
        {
            public override bool Equals(Cycle x, Cycle y)
            {
                return x.Equals(y);
            }

            public override int GetHashCode(Cycle obj)
            {
                throw new System.NotImplementedException();
            }
        }

        public bool Equals(Cycle other)
        {
            if (Len != other.Len) return false;

            bool is_cyclic_shift;

            for (int i = 0; i < Len; i++)
            {
                is_cyclic_shift = true;
                for (int j = 0; j < other.Len; j++)
                {
                    if (this[j + i] != other[j])
                    {
                        is_cyclic_shift = false;
                        break;
                    }
                }

                if (is_cyclic_shift) return true;
            }

            return false;
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            return Equals((Cycle)other);
        }

        public override int GetHashCode()
        {
            int res = 0;
            foreach (var el in cycle) res ^= el.GetHashCode();
            return res;
        }

        public override string ToString()
        {
            string res = "";
            foreach (var v in cycle)
            {
                res += v + "->";
            }

            return res + cycle[0];
        }
    }
}