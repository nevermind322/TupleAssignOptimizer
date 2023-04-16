namespace AssignTupleOptimizer
{
    public class SymbolVertex : Vertex
    {
        public readonly Symbol symbol;

        public SymbolVertex(Symbol s) : base(s.name)
        {
            symbol = s;
        }
    }
}