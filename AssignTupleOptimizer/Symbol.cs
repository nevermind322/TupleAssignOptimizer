namespace AssignTupleOptimizer
{
    public class Symbol
    {
        public readonly string name;

        public bool fromOuterScope = false;

        public Symbol(string n)
        {
            name = n;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;
            return this.name == (obj as Symbol).name;
        }
    }
}