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
            //TestGraph1();
            Console.WriteLine();

            //TestGraph2();

            TestTupleAssign("(a, b) = (b,a)");
            Console.WriteLine();
           
            Console.WriteLine();

            Console.ReadKey();
        }

        private static void TestGraph1()
        {
            Vertex a = new Vertex("a");
            Vertex b = new Vertex("b");
            Vertex c = new Vertex("c");
            Vertex d = new Vertex("d");
            Vertex f = new Vertex("f");
            Vertex e = new Vertex("e");
            Vertex g = new Vertex("g");
            Vertex h = new Vertex("h");
            Vertex i = new Vertex("i");
            Vertex j = new Vertex("j");
            Vertex k = new Vertex("k");

            Edge ab = new Edge(a, b);
            Edge bc = new Edge(b, c);
            Edge ca = new Edge(c, a);
            Edge bd = new Edge(b, d);
            Edge de = new Edge(d, e);
            Edge ef = new Edge(e, f);
            Edge fd = new Edge(f, d);
            Edge gf = new Edge(g, f);
            Edge gh = new Edge(g, h);
            Edge hi = new Edge(h, i);
            Edge ij = new Edge(i, j);
            Edge jg = new Edge(j, g);
            Edge jk = new Edge(j, k);

            var edges = new List<Edge>()
            {
                ab,
                bc,
                ca,
                bd,
                de,
                ef,
                fd,
                gf, gh,
                hi,
                ij, jg,
                jk
            };

            AssignGraph graph = new AssignGraph(edges);

            var components = graph.findStronglyConnectedComponets();

            foreach (var component in components)
            {
                foreach (var vert in component)
                    Console.WriteLine("{0}", vert.label);
                Console.WriteLine();
            }

            var cycles = graph.findAllUniqueElementaryCycles();

            cycles.ForEach(cycle => Console.WriteLine(cycle));
        }

        static void TestGraph2()
        {
            Vertex v1 = new Vertex("1");
            Vertex v2 = new Vertex("2");
            Vertex v3 = new Vertex("3");
            Vertex v4 = new Vertex("4");
            Vertex v5 = new Vertex("5");
            Vertex v6 = new Vertex("6");
            Vertex v7 = new Vertex("7");
            Vertex v8 = new Vertex("8");
            Vertex v9 = new Vertex("9");
            Vertex v10 = new Vertex("10");

            Edge v12 = new Edge(v1, v2);
            Edge v15 = new Edge(v1, v5);
            Edge v18 = new Edge(v1, v8);
            Edge v23 = new Edge(v2, v3);
            Edge v27 = new Edge(v2, v7);
            Edge v29 = new Edge(v2, v9);
            Edge v31 = new Edge(v3, v1);
            Edge v32 = new Edge(v3, v2);
            Edge v34 = new Edge(v3, v4);
            Edge v36 = new Edge(v3, v6);
            Edge v45 = new Edge(v4, v5);
            Edge v52 = new Edge(v5, v2);
            Edge v510 = new Edge(v5, v10);
            Edge v64 = new Edge(v6, v4);
            Edge v89 = new Edge(v8, v9);
            Edge v98 = new Edge(v9, v8);
            Edge v10v5 = new Edge(v10, v5);
            Edge v10v1 = new Edge(v10, v1);

            var edges = new List<Edge>()
            {
                v12, v15, v18,
                v23, v27, v29,
                v31, v32, v34, v36,
                v45,
                v52, v510,
                v64,
                v89,
                v98,
                v10v5, v10v1
            };

            AssignGraph graph = new AssignGraph(edges);

            var components = graph.findStronglyConnectedComponets();

            foreach (var component in components)
            {
                foreach (var vert in component)
                    Console.WriteLine("{0}", vert.label);
                Console.WriteLine();
            }

            var cycles = graph.findAllUniqueElementaryCycles();

            cycles.ForEach(cycle => Console.WriteLine(cycle));
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