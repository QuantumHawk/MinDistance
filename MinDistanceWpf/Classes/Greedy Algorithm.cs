using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphColoring.Classes
{
    class Greedy_Algorithm
    {
        private int[] _result;
     
        
        public int[] Result
        {
           get {return _result;}
           set { _result = value; }
        }

        private int _iteration;
        public int Iteration
        {
            get { return _iteration; }
            set { _iteration = value; }
        }

        public void Greedy(int V,LinkedList<Int32>[] adj)
        {
            _result = new int[V];

            _result[0] = 0;

            for (int u = 1; u < V; u++)
            {
                _result[u] = -1;
            }
            // A temporary array to store the available colors. True
            // value of available[cr] would mean that the color cr is
            // assigned to one of its adjacent vertices
      

            bool[] available = new bool[V];
            for (int cr = 0; cr < V; cr++)
                available[cr] = false;

            // Assign colors to remaining V-1 vertices
            for (int u = 1; u < V; u++)
            {
                // Process all adjacent vertices and flag their colors
                // as unavailable
                IEnumerator<Int32> it = adj[u].GetEnumerator();
                while (it.MoveNext())
                {
                    int i = it.Current;
                    if (_result[i] != -1)
                        available[_result[i]] = true;
                }

                // Find the first available color
                int cr;
                for (cr = 0; cr < V; cr++)
                    if (available[cr] == false)
                        break;

                _result[u] = cr; // Assign the found color

                // Reset the values back to false for the next iteration
                it = adj[u].GetEnumerator();
                while (it.MoveNext())
                {
                    int i = it.Current;
                    if (_result[i] != -1)
                        available[_result[i]] = false;
                }
                _iteration++;
            }

        }
    }
}
