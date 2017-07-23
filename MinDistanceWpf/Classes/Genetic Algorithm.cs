using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphColoring.Classes
{
    class Genetic_Algorithm
    {
        public int Size;
        private int Population_Size;					// Size of Initial Random Population - This is a sample
        // static int Population_Size = 10;					// Size of Initial Random Population - Mentor Sample Test
        private int Node_Quantity;						// Number of Nodes - Mentor Sample Test
        //const int Node_Quantity = 8;						// Number of Nodes 
        private int Color_Num;						//You Must change this if you want coloring wuth more colors
        public int[] Population_Fitness_Value;	//Define Global
        int repeat = 0;			//Maximum 
        //int k = Population_Size;						// K is Variable for using in Circles (While,do etc.) Easing use of Population_Size
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        private int _iteration;
        public int Iteration
        {
            get { return _iteration; }
            set { _iteration = value; }
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

        public MT19937 f = new MT19937(); // mersenne_twister
        // A Node in Adjacency LIST
        public struct node
        {
            public int name;
            public int color;
        };

        // Graph is new Data Structures for saving our MAP
        public struct Graph
        {
            public int Size;
            public node[] Nodes_Array;
            public List<node>[] Adjacency;

            public Graph(int Node_Quantity)
            {
                Size = Node_Quantity;
                Nodes_Array = new node[Node_Quantity];
                Adjacency = new List<node>[Node_Quantity];
             
                for (int i = 0; i < Node_Quantity; ++i)
                    Adjacency[i] = new List<node>();
            }
        }

        //public void Edge_ADD(int src, int dest)
        //{
        //    MAP.Adjacency[src].Add(MAP.Nodes_Array[dest]);
        //    MAP.Adjacency[dest].Add(MAP.Nodes_Array[src]);
        //}

        public Graph MAP = new Graph();

        private int[] _result;


        public int[] Result
        {
            get { return _result; }
            set { _result = value; }
        }


        public struct MAP_COLOR_ARRAY
        {
            public int[] Nodes_Colors;
            public MAP_COLOR_ARRAY(int Node_Quantity)
            {
                Nodes_Colors = new int[Node_Quantity];
            }
        };


        public static void Swap(int[,] a1, int x1, int y1, int x2, int y2)
        {
            int temp = a1[x1, y1];
            a1[x1, y1] = a1[x2, y2];
            a1[x2, y2] = temp;
        }

        public float GetRandomNumber(double minimum, double maximum)
        {

            return (float)(random.NextDouble() * (maximum - minimum) + minimum);
        }
        public int Random_Selection()
        {		//Random Selection regards to goodness and randomness

            float r = (float)f.genrand_real3();

            float Temp = 0;		//Temporary Number for using goodness in randomize process
            float Sum = 0;		//Sum of All Population Values
            int worst = 0;
            int[] Temp_Population = new int[Population_Size];
            for (int i = 0; i < Population_Size; i++)  //Copy Population_Fitness_Value in Temp Array
            {	
                Temp_Population[i] = Population_Fitness_Value[i];
            }
            for (int i = 0; i < Population_Size; i++)
            {	//Find worst
                if (Population_Fitness_Value[i] < worst)
                {
                    worst = Population_Fitness_Value[i];
                }
            }
            for (int i = 0; i < Population_Size; i++) //Shift all and change the value of worst and best
            {	
                Temp_Population[i] -= worst;
            }
            for (int i = 0; i < Population_Size; i++) //Compute Sum of All Population Size
            {	
                Sum += Temp_Population[i];
            }

            //Sorting and keep Name of each fitness value
            int[,] Array_2D = new int[Population_Size, 2];
            for (int i = 0; i < Population_Size; i++)
            {
                Array_2D[i, 0] = i;
                Array_2D[i, 1] = Temp_Population[i];
            }
            //shell sort Algorithm for sorting
            int[] gap_sequence = new int[] { 13, 9, 5, 2, 1 };
            int n = Population_Size;

            foreach (int gap in gap_sequence) if (gap < n) 
                {
                    for (int i = gap; i < n; ++i)
                        for (int j = i - gap; j >= 0 && Array_2D[j, 1] > Array_2D[j + gap, 1]; j -= gap)
                        {

                            Swap(Array_2D, j, 0, j + gap, 0);
                            Swap(Array_2D, j, 1, j + gap, 1);
                        }
                }
            //Randomize process according to goodness and randomness
            for (int i = 0; i < Population_Size; i++)
            {
                Temp += Array_2D[i, 1] / Sum;
                if (r <= Temp) return Array_2D[i, 0];
            }
            return 0;
        }



        public void Pop_Fitness_Evaluator(MAP_COLOR_ARRAY[] Initital_Population)
        {	
            int value;

            for (int i = 0; i < Population_Size; i++)
            {
                value = 0;
   
                for (int j = 0; j < Node_Quantity; j++)             //Coloring Graph with each Individuals in Population 
                {
                    MAP.Nodes_Array[j].color = Initital_Population[i].Nodes_Colors[j];
                }

                for (int k = 0; k < MAP.Size; k++)
                {

                    IEnumerator<node> it = MAP.Adjacency[k].GetEnumerator();

                    while (it.MoveNext())
                    {
                        int Temp1 = MAP.Nodes_Array[k].name;
                        int Temp2 = MAP.Nodes_Array[k].color;
                        int Temp3 = it.Current.name;
                        int Temp4 = MAP.Nodes_Array[it.Current.name].color;
                        if (MAP.Nodes_Array[it.Current.name].color == MAP.Nodes_Array[k].color)
                        {
                            value--;
                        }
                    }
                }
                Population_Fitness_Value[i] = value;
            }
        }

        public MAP_COLOR_ARRAY Reproduce(MAP_COLOR_ARRAY[] Population, int x, int y)//CrossOver Operation in Genetic Algorithm
        {		
            MAP_COLOR_ARRAY Result;

            //Select Real X and Y according to their name x and y
            MAP_COLOR_ARRAY X_MAP_COLOR = new MAP_COLOR_ARRAY(Node_Quantity);
            MAP_COLOR_ARRAY Y_MAP_COLOR = new MAP_COLOR_ARRAY(Node_Quantity);
            for (int i = 0; i < Node_Quantity; i++)
            {
                X_MAP_COLOR.Nodes_Colors[i] = Population[x].Nodes_Colors[i];
                Y_MAP_COLOR.Nodes_Colors[i] = Population[y].Nodes_Colors[i];
            }


            int c = f.RandomRange(0, Node_Quantity - 1);

            //CrossOver compound two Population
            for (int i = c; i < Node_Quantity; i++)
            {
                X_MAP_COLOR.Nodes_Colors[i] = Y_MAP_COLOR.Nodes_Colors[i];
            }
            Result = X_MAP_COLOR;
            return Result;
        }

        public MAP_COLOR_ARRAY Mutation(MAP_COLOR_ARRAY Child) //Mutation Operation in Genetic Algorithm
        {		
            
            MAP_COLOR_ARRAY Result;
            int Chance = f.RandomRange(0, 20);

            if (Chance > 1)
            {
                return Child;
            }

            int Max_Change = f.RandomRange(0, Node_Quantity - 1);

            for (int i = 0; i < Max_Change; i++)
            {

                int Rand_Num = f.RandomRange(0, Node_Quantity - 1);
                int Rand_Color_Num = f.RandomRange(0, Color_Num - 1);

                Child.Nodes_Colors[Rand_Num] = Rand_Color_Num;
            }
            Result = Child;
            return Result;
        }

        //See if All the Constraints is pass and return false if we have unpass constraint - Use global Variables
        private bool Fit_Enough()
        {
            bool Result = false;
            for (int i = 0; i < Population_Size; i++)
            {
                if (Population_Fitness_Value[i] == 0)
                {
                    Result = true;
                    return Result;
                }
            }
            return Result;
        }

        public void G_Algorithm(List<Edge> _edges, int _node_Quantitym, int _color_Num, int _population_Size)
        {
            Population_Size = _population_Size;
            Node_Quantity = _node_Quantitym;
            Color_Num = _color_Num;
            MAP = new Graph(Node_Quantity);

            _result = new int[Node_Quantity];
            _result[0] = 0;

            for (int u = 1; u < Node_Quantity; u++)
              {
                    _result[u] = -1;
              }

            for (int i = Node_Quantity - 1; i >= 0; i--)
            {

                MAP.Nodes_Array[i].color = 0;
                MAP.Nodes_Array[i].name = i;
            }

            foreach (Edge _edge in _edges)
            {
                int v = _edge.FirstNode.Mark;
                int w = _edge.SecondNode.Mark;

                MAP.Adjacency[v].Add(MAP.Nodes_Array[w]);
                MAP.Adjacency[w].Add(MAP.Nodes_Array[v]);
            }

            Random rand = new Random();

            bool Fitness_Func_Result = false;

            Population_Fitness_Value = new int[Population_Size]; 

            MAP_COLOR_ARRAY[] Initial_Population = new MAP_COLOR_ARRAY[Population_Size];
            for (int i = 0; i < Population_Size; ++i)
                Initial_Population[i] = new MAP_COLOR_ARRAY(Node_Quantity);

            //Initial Random Population Making Process  
            for (int i = 0; i < Population_Size; i++)
            {
                for (int j = 0; j < Node_Quantity; j++)
                {
                    int temp = RandomNumber(0, Color_Num - 1); //rand() % Color_Num;  
                    Initial_Population[i].Nodes_Colors[j] = temp;
                }
            }

            do
            {
                MAP_COLOR_ARRAY[] New_Population = new MAP_COLOR_ARRAY[Population_Size];
                for (int i = 0; i < Population_Size; ++i)
                    New_Population[i] = new MAP_COLOR_ARRAY(Node_Quantity);
                repeat++;

                for (int i = 0; i < Population_Size; i++)
                {
                    int x = Random_Selection(); //Using global variable Population_Fitness_Value
                    int y = Random_Selection();


                    MAP_COLOR_ARRAY Child = Reproduce(Initial_Population, x, y);
                    Child = Mutation(Child);

                    //Copy Array
                    for (int j = 0; j < Node_Quantity; j++)
                    {
                        New_Population[i].Nodes_Colors[j] = Child.Nodes_Colors[j];
                    }
                }
                //Copy Arrays
                for (int i = 0; i < Population_Size; i++)
                {
                    for (int j = 0; j < Node_Quantity; j++)
                    {
                        Initial_Population[i].Nodes_Colors[j] = New_Population[i].Nodes_Colors[j];
                    }
                }
                Pop_Fitness_Evaluator(Initial_Population);	//Fitness & Goodness evaluator
                Fitness_Func_Result = Fit_Enough();


            } while (repeat < 10000 && !Fitness_Func_Result);

            int Best_Individual = -1;
            for (int i = 0; i < Population_Size; i++)
            {
                if (Population_Fitness_Value[i] == 0)
                    Best_Individual = i;
            }

            if (Best_Individual == -1)
            {
                //Answer Not found :C
                if (!Fitness_Func_Result)
                {
                    //если ответ не найден, наверное стоит еще раз запустить процедуру
                }
            }
            else
            {
                
                //Print Best Population-Test
                for (int i = 0; i < Node_Quantity; i++)
                {
                    _result[i] = Initial_Population[Best_Individual].Nodes_Colors[i];
                    _iteration = repeat;
                }

            }
        }
    }
}
