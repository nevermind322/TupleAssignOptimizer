using System;
using System.Collections.Generic;
using System.Diagnostics;
using TupleParser;

namespace AssignTupleOptimizer
{
    class Program
    {
        public static void Main()
        {

            TestTupleAssign("(a, b) = (b, a)");
            
            Console.ReadKey();
        }

        static TupleAssign getTupleAssign(string input)
        {
            Scanner scanner = new Scanner();
            scanner.SetSource(input, 0);

            Parser parser = new Parser(scanner);
            var b = parser.Parse();
            if (!b) throw new Exception("Parsing error");
            return new TupleAssign(parser.left, parser.right);
        }

        static void TestTupleAssign(string input)
        {
            var t_a = getTupleAssign(input);
            TestTupleAssign(t_a);
        }

        static void TestTupleAssign(TupleAssign tupleAssign)
        {
            var assignGraph = GraphUtils.createAssignGraph(tupleAssign.left, tupleAssign.right);
            assignGraph.drawGraph();
            var order = assignGraph.GetAssignOrder();
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var assign in order)
            {
                Console.WriteLine("{0} = {1}", assign.to, assign.from);
            }
            Console.ResetColor();
        }


        class TupleAssign
        {
            public List<Symbol> left;
            public List<Symbol> right;

            public TupleAssign(List<Symbol> l, List<Symbol> r)
            {
                left = l;
                right = r;
            }
        }
    }
}