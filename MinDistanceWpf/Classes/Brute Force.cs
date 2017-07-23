using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphColoring.Classes
{
    class Brute_Force
    {
        private readonly LinkedList<Int32>[] _graph;
        private readonly int _colors;
        private readonly int[] _result;
        private int _fitness;
         static int _v;
         static int[] result;

         public MT19937 f = new MT19937(); // mersenne_twister

         static int _iteration = 0;
         public  int Iteration;

        public Brute_Force(LinkedList<Int32>[] adj, int colors)
        {
            _graph = adj;
            var nodes = _v; 
            _colors = colors;
            _result = new int[nodes];
        }

        public Brute_Force(int V,LinkedList<Int32>[] adj, int colors)
        {
            _graph = adj;
            _v = V;
            _colors = colors; 
            _result = new int[_v];
        }

        public int[] Resolve()
        {
            int i;
            var isFound = true;
            var colors = _colors;
            _iteration = 0;
            for (i = colors - 1; i > 0 && isFound; i--)
            {
                var bf = new Brute_Force(_graph,i + 1);
                if (bf.TrySolve() >= 0)
                {
                    isFound = false;
                    result = bf._result;
                    Iteration = _iteration;
                }
            }
            return result;
        }

        public int TrySolve()
        {
            _fitness = 100;
            for (var i = 0; i < 1000000 && _fitness != 0; i++)
            {
                GenerateResult();
                EvaluateResult(_graph);
                _iteration++;
            }

            return _fitness;
        }

        public void GenerateResult()
        {
            
            for (var j = 0; j < _result.Length; j++)
                _result[j] =f.RandomRange(0,_colors);
        }

        public void EvaluateResult(LinkedList<Int32>[] adj)
        {
            var conflicts = 0;
            for (var i = 0; i < _result.Length; i++)
            {
                var current = _result[i];
                var edgesArr = adj[i];
                conflicts += edgesArr.Count(t => _result[t] == current);
            }
            _fitness = conflicts / 2;

        }
    }
}
