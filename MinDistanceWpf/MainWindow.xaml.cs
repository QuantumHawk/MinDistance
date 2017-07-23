using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using GraphColoring.Classes;
using System.Reflection;
using System.Collections;
using System.Data;
using System.Diagnostics;


namespace GraphColoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const double _diameter = 30;
        private const double _edgeLabelSize = 20;

        private const int _fontSize = 12;
        private const int _edgeFontSize = 10;

        private int _count;
        private bool _findMinDistance;

        private List<Node> _cloud;
        private ReachableNodeList _reachableNodes;

        private List<Node> _nodes;
        private List<Edge> _edges;

        private Node _edgeNode1, _edgeNode2;
        private SolidColorBrush _unvisitedBrush, _visitedBrush;
        private bool _isGraphConnected;
        private List<SolidColorBrush> _brushes;

        static DataTable tbl;

        public MainWindow()
        {
            InitializeComponent();
            drawingCanvas.SetValue(Canvas.ZIndexProperty, 0);

            _cloud = new List<Node>();
            _reachableNodes = new ReachableNodeList();

            _nodes = new List<Node>();
            _edges = new List<Edge>();
            
            _count = 1;
            _findMinDistance = false;
            _isGraphConnected = true;

           _brushes = new List<SolidColorBrush>();
            var props = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propInfo in props)
            {
                _brushes.Add((SolidColorBrush)propInfo.GetValue(null, null));
            }

            _unvisitedBrush = new SolidColorBrush(Colors.Black);
            _visitedBrush = new SolidColorBrush(Colors.DarkViolet);

            tbl = new DataTable("SomeData");
            tbl.Columns.Add("Algorithm", typeof(string));
            tbl.Columns.Add("Vertex", typeof(string));
            tbl.Columns.Add("Edge", typeof(string));
            tbl.Columns.Add("Time", typeof(string));
            tbl.Columns.Add("Iterations", typeof(string));
            listView.DataContext = tbl;
        }


        /// <summary>
        /// The event establishes the all the GUI interactions with the user:
        ///     -the creation of nodes
        ///     -the creation of edges
        ///     -invoke the min distance 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                Point clickPoint = e.GetPosition(drawingCanvas);

                if (HasClickedOnNode(clickPoint.X, clickPoint.Y))
                {
                    AssignEndNodes(clickPoint.X, clickPoint.Y);
                    if (_edgeNode1 != null && _edgeNode2 != null)
                    {
                        if (_findMinDistance)
                        {
                            statusLabel.Content = "Calculating...";
                            LaunchMinDistanceTask();
                        }
                        else
                        {
                            //build an edge
                            double distance = GetEdgeDistance();
                            if (distance != 0.0)
                            {
                                Edge edge = CreateEdge(_edgeNode1, _edgeNode2, distance);
                                _edges.Add(edge);

                                _edgeNode1.Edges.Add(edge);
                                _edgeNode2.Edges.Add(edge);

                                PaintEdge(edge);
                            }
                            ClearEdgeNodes();
                        }
                    }
                }
                else
                {
                    if (!OverlapsNode(clickPoint))
                    {
                        Node n = CreateNode(clickPoint);
                        _nodes.Add(n);
                        PaintNode(n);
                        _count++;
                        ClearEdgeNodes();
                    }
                }
            }
        }

        private void LaunchMinDistanceTask()
        {
            Task.Factory.StartNew(() =>
                FindMinDistancePath(_edgeNode1, _edgeNode2)
            )
            .ContinueWith((task) =>
            {
                if (task.IsFaulted)
                    MessageBox.Show("An error has occurred", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    PaintMinDistancePath(_edgeNode1, _edgeNode2);

            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ClearEdgeNodes()
        {
            _edgeNode1 = _edgeNode2 = null;
        }

        /// <summary>
        /// A method to detect whether the user has clicked on a node
        /// used either for edge creation or for indicating the end-points for which to find
        /// the minimum distance
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>Whether a user is clicked on a existing node</returns>
        private bool HasClickedOnNode(double x, double y)
        {
            bool rez = false;
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].HasPoint(new Point(x, y)))
                {
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// <summary>
        /// Get a node at a specific coordinate
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>The node that has been found or null if there is no node at the speicied coordinates</returns>
        private Node GetNodeAt(double x, double y)
        {
            Node rez = null;
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].HasPoint(new Point(x, y)))
                {
                    rez = _nodes[i];
                    break;
                }
            }
            return rez;
        }
        /// <summary>
        /// Upon the creation of a new node,
        /// make sure that it is not overlapping an existing node
        /// </summary>
        /// <param name="p">A x,y point</param>
        /// <returns>Whether there is an overlap with an existing node</returns>
        private bool OverlapsNode(Point p)
        {
            bool rez = false;
            double distance;
            for (int i = 0; i < _nodes.Count; i++)
            {
                distance = GetDistance(p, _nodes[i].Center);
                if (distance < _diameter)
                {
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// <summary>
        /// Use an additional dialog window to get the distance
        /// for an edge as specified by the user
        /// </summary>
        /// <returns>The distance value specified by the user</returns>
        private double GetEdgeDistance() 
        {
            double distance = 1.0; //0.0
            //DistanceDialog dd = new DistanceDialog();
            //dd.Owner = this;

            //dd.ShowDialog();
            //distance = dd.Distance;

            return distance;
        }
        /// <summary>
        /// Calculate the Eucledean distance between two points
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>The distance between the two points</returns>
        private double GetDistance(Point p1, Point p2)
        {
            double xSq = Math.Pow(p1.X - p2.X, 2);
            double ySq = Math.Pow(p1.Y - p2.Y, 2);
            double dist = Math.Sqrt(xSq + ySq);

            return dist;
        }

        private void AssignEndNodes(double x, double y)
        {
            Node currentNode = GetNodeAt(x, y);
            if (currentNode != null)
            {
                if (_edgeNode1 == null)
                {
                    _edgeNode1 = currentNode;
                    statusLabel.Content = "You have selected node " + currentNode.Label + ". Please select another node.";
                }
                else
                {
                    if (currentNode != _edgeNode1)
                    {
                        _edgeNode2 = currentNode;
                        statusLabel.Content = "Click on the canvas to create a node.";
                    }
                }
            }
        }

        /// <summary>
        /// Create a new node using the coordinates specified by a point
        /// </summary>
        /// <param name="p">A Point object that carries the coordinates for Node creation</param>
        /// <returns></returns>
        private Node CreateNode(Point p)
        {
            double nodeCenterX = p.X - _diameter / 2;
            double nodeCenterY = p.Y - _diameter / 2;
            Node newNode = new Node(new Point(nodeCenterX, nodeCenterY), p, _count.ToString(), _diameter, _count - 1, _unvisitedBrush);
            return newNode;
        }

        /// <summary>
        /// Paint a single node on the canvas
        /// </summary>
        /// <param name="node">A node object carrying the coordinates</param>
        private void PaintNode(Node node)
        {
            //paint the node
            Ellipse ellipse = new Ellipse();
           
            if (node.Visited)
                ellipse.Fill = node.Color;
            else
                ellipse.Fill = _unvisitedBrush;

            ellipse.Width = _diameter;
            ellipse.Height = _diameter;

            ellipse.SetValue(Canvas.LeftProperty, node.Location.X);
            ellipse.SetValue(Canvas.TopProperty, node.Location.Y);
            ellipse.SetValue(Canvas.ZIndexProperty, 2);
            //add to the canvas
            drawingCanvas.Children.Add(ellipse);

            //paint the node label 
            TextBlock tb = new TextBlock();
            tb.Text = node.Label;
            tb.Foreground = Brushes.Red;
            tb.FontSize = _fontSize;
            tb.TextAlignment = TextAlignment.Center;
            tb.SetValue(Canvas.LeftProperty, node.Center.X - (_fontSize / 4 * node.Label.Length));
            tb.SetValue(Canvas.TopProperty, node.Center.Y - _fontSize / 2);
            tb.SetValue(Canvas.ZIndexProperty, 3);

            //add to canvas on top of the cirle
            drawingCanvas.Children.Add(tb);
        }

        private Edge CreateEdge(Node node1, Node node2, double distance)
        {
            return new Edge(node1, node2, distance);
        }

        private void PaintEdge(Edge edge)
        {
            //draw the edge
            Line line = new Line();
            line.X1 = edge.FirstNode.Center.X;
            line.X2 = edge.SecondNode.Center.X;

            line.Y1 = edge.FirstNode.Center.Y;
            line.Y2 = edge.SecondNode.Center.Y;

            line.Stroke = _unvisitedBrush;

            line.StrokeThickness = 1;
            line.SetValue(Canvas.ZIndexProperty, 1);
            drawingCanvas.Children.Add(line);

            ////draw the distance label
            //Point edgeLabelPoint = GetEdgeLabelCoordinate(edge);
            //TextBlock tb = new TextBlock();
            //tb.Text = edge.Length.ToString();
            //tb.Foreground = Brushes.White;

            // tb.Background = _unvisitedBrush;

            //tb.Padding = new Thickness(5);
            //tb.FontSize = _edgeFontSize;

            //tb.MinWidth = _edgeLabelSize;
            //tb.MinHeight = _edgeLabelSize;

            //tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //tb.TextAlignment = TextAlignment.Center;

            //tb.SetValue(Canvas.LeftProperty, edgeLabelPoint.X);
            //tb.SetValue(Canvas.TopProperty, edgeLabelPoint.Y);
            //tb.SetValue(Canvas.ZIndexProperty, 2);
           // drawingCanvas.Children.Add(tb);
        }

        /// <summary>
        /// Calculate the coordinates where an edge label is to be drawn
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private Point GetEdgeLabelCoordinate(Edge edge)
        {

            double x = Math.Abs(edge.FirstNode.Location.X - edge.SecondNode.Location.X) / 2;
            double y = Math.Abs(edge.FirstNode.Location.Y - edge.SecondNode.Location.Y) / 2;

            if (edge.FirstNode.Location.X > edge.SecondNode.Location.X)
                x += edge.SecondNode.Location.X;
            else
                x += edge.FirstNode.Location.X;

            if (edge.FirstNode.Location.Y > edge.SecondNode.Location.Y)
                y += edge.SecondNode.Location.Y;
            else
                y += edge.FirstNode.Location.Y;

            return new Point(x, y);
        }
        /// <summary>
        /// The implementation of the Djikstra algorithm
        /// </summary>
        /// <param name="start">Starting Node</param>
        /// <param name="end">Ending Node</param>
        private void FindMinDistancePath(Node start, Node end)
        {
            _cloud.Clear();
            _reachableNodes.Clear();
            Node currentNode = start;
            currentNode.Visited = true;
            start.TotalCost = 0;
            _cloud.Add(currentNode);
            ReachableNode currentReachableNode;

            while (currentNode != end)
            {
                AddReachableNodes(currentNode);
                UpdateReachableNodesTotalCost(currentNode);

                //if we cannot reach any other node, the graph is not connected
                if (_reachableNodes.ReachableNodes.Count == 0)
                {
                    _isGraphConnected = false;
                    break;
                }

                //get the closest reachable node
                currentReachableNode = _reachableNodes.ReachableNodes[0];
                //remove if from the reachable nodes list
                _reachableNodes.RemoveReachableNode(currentReachableNode);
                //mark the current node as visited
                currentNode.Visited = true;
                //set the current node to the closest one from the cloud
                currentNode = currentReachableNode.Node;
                //set a pointer to the edge from where we came from
                currentNode.EdgeCameFrom = currentReachableNode.Edge;
                //mark the edge as visited
                currentReachableNode.Edge.Visited = true;

                _cloud.Add(currentNode);
            }
        }

        /// <summary>
        /// The method paints the the minimum distance path starting from the end node
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void PaintMinDistancePath(Node start, Node end)
        {
            if (_isGraphConnected)
            {
                Node currentNode = end;
                double totalCost = 0;
                while (currentNode != start)
                {
                    currentNode.Visited = true;
                    currentNode.EdgeCameFrom.Visited = true;
                    totalCost += currentNode.EdgeCameFrom.Length;

                    PaintNode(currentNode);
                    PaintEdge(currentNode.EdgeCameFrom);

                    currentNode = GetNeighbour(currentNode, currentNode.EdgeCameFrom);
                }
                //paint the current node -> this is the start node
                if(currentNode != null)
                    PaintNode(currentNode);

                start.Visited = true;
                statusLabel.Content = "Total cost: " + totalCost.ToString();
            }
            else
            {
                ClearEdgeNodes();
                _isGraphConnected = true;
                _findMinDistance = false;
                statusLabel.Content = "Click on the canvas to create a node.";
                MessageBox.Show("The graph is not connected. Cannot find a path", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        /// <summary>
        /// On each iteration we need to discover the reachable nodes by going through all the edges that are incident on the node
        /// </summary>
        /// <param name="node"></param>
        private void AddReachableNodes(Node node)
        {
            Node neighbour;
            ReachableNode rn;
            foreach (Edge edge in node.Edges)
            {
                neighbour = GetNeighbour(node, edge);
                //make sure we don't add the node we came from
                if (node.EdgeCameFrom == null || neighbour != GetNeighbour(node, node.EdgeCameFrom))
                {
                    //make sure we don't add a node already in the cloud
                    if (!_cloud.Contains(neighbour))
                    {
                        //if the node is already reachable
                        if (_reachableNodes.HasNode(neighbour))
                        {
                            //if the distance from this edge is smaller than the current total cost
                            //amend the reachable node using the current edge
                            if (node.TotalCost + edge.Length < neighbour.TotalCost)
                            {
                                rn = _reachableNodes.GetReachableNodeFromNode(neighbour);
                                rn.Edge = edge;
                            }
                        }
                        else
                        {
                            rn = new ReachableNode(neighbour, edge);
                            _reachableNodes.AddReachableNode(rn);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// On each iteration the total cost of all reachable nodes needs to be recalculated
        /// </summary>
        /// <param name="node">The current node</param>
        private void UpdateReachableNodesTotalCost(Node node)
        {
            double currentCost = node.TotalCost;
            foreach (ReachableNode rn in _reachableNodes.ReachableNodes)
            {
                if (currentCost + rn.Edge.Length < rn.Node.TotalCost || rn.Node.TotalCost == -1)
                    rn.Node.TotalCost = currentCost + rn.Edge.Length;
            }

            _reachableNodes.SortReachableNodes();
        }

        /// <summary>
        /// Get the node on the other side of the edge
        /// </summary>
        /// <param name="node">The node from which we look for the neighbour</param>
        /// <param name="edge">The edge that we are currently on</param>
        /// <returns></returns>
        private Node GetNeighbour(Node node, Edge edge)
        {
            if (edge.FirstNode == node)
                return edge.SecondNode;
            else
                return edge.FirstNode;
        }

        private void findMinDistanceBtn_Click(object sender, RoutedEventArgs e)
        {
            this._findMinDistance = true;
            statusLabel.Content = "Select two nodes for which you want to find the minimum distance path";
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            drawingCanvas.Children.Clear();
        }

        private void Clear()
        {
            this._nodes.Clear();
            this._edges.Clear();
            this._cloud.Clear();
            this._reachableNodes.Clear();
            this._findMinDistance = false;
            this._count = 1;
        }

        private void Restart()
        {
            this._findMinDistance = false;
            
            this._cloud.Clear();
            this._reachableNodes.Clear();

            foreach (Node n in _nodes)
                n.Visited = false;

            foreach (Edge e in _edges)
                e.Visited = false;
        }

        private void PaintAllNodes()
        {
            foreach (Node n in _nodes)
                PaintNode(n);
        }

        private void PaintAllEdges()
        {
            foreach (Edge e in _edges)
                PaintEdge(e);
        }

        private void restartBtn_Click(object sender, RoutedEventArgs e)
        {
            Restart();
            PaintAllNodes();
            PaintAllEdges();
        }

        private void Button_Greedy_Click(object sender, RoutedEventArgs e)
        {

            Stopwatch time = new Stopwatch();
            time.Start();

            LinkedList<Int32>[] adj; //Adjacency List
            adj = new LinkedList<Int32>[_nodes.Count];
            for (int i = 0; i < _nodes.Count; ++i)
            {
                adj[i] = new LinkedList<Int32>();
            }

            foreach (Edge _edge in _edges)
            {
                int v = _edge.FirstNode.Mark;
                int w = _edge.SecondNode.Mark;

                adj[v].AddFirst(w);
                adj[w].AddFirst(v);
            }
            Greedy_Algorithm greedy = new Greedy_Algorithm();
            greedy.Greedy(_nodes.Count, adj);
            int[] _result = greedy.Result;

              foreach (Node _node in _nodes)
                {
                   _node.Visited = true;
                   _node.Color = _brushes[_result[_node.Mark]];
                   PaintNode(_node);                 
                }
              time.Stop();
              var resultTime = time.ElapsedMilliseconds;

              tbl.Rows.Add("Greedy Algorithm", _nodes.Count, _edges_Grage(), resultTime.ToString(), greedy.Iteration);
            
        }

        private void Button_Brutal_Click(object sender, RoutedEventArgs e)
        {
            if (tbCol.Text.Length > 0)
            {
                Stopwatch time = new Stopwatch();
                time.Start();

                LinkedList<Int32>[] adj; //Adjacency List
                adj = new LinkedList<Int32>[_nodes.Count];
                for (int i = 0; i < _nodes.Count; ++i)
                {
                    adj[i] = new LinkedList<Int32>();
                }

                foreach (Edge _edge in _edges)
                {
                    int v = _edge.FirstNode.Mark;
                    int w = _edge.SecondNode.Mark;

                    adj[v].AddFirst(w);
                    adj[w].AddFirst(v);
                }
                var br = new Brute_Force(_nodes.Count, adj, Convert.ToInt32(tbCol.Text)); // TODO: разобраться с цветами!!!
                int[] _result = br.Resolve();
                foreach (Node _node in _nodes)
                {
                    _node.Visited = true;
                    _node.Color = _brushes[_result[_node.Mark]];
                    PaintNode(_node);
                }

                time.Stop();
                var resultTime = time.ElapsedMilliseconds;
                tbl.Rows.Add("Brute Force", _nodes.Count, _edges_Grage(), resultTime.ToString(), br.Iteration);
            }
        }

        private void Button_GA_Click(object sender, RoutedEventArgs e)
        {
            if (tbCol.Text.Length > 0 && tbPop.Text.Length > 0)
            {     
               Stopwatch time = new Stopwatch();
              time.Start();
              int _color_Num = Convert.ToInt32(tbCol.Text);
              int _population_Size = Convert.ToInt32(tbPop.Text);

       
              var ga = new Genetic_Algorithm();
              ga.G_Algorithm(_edges,_nodes.Count,_color_Num,_population_Size);

              int[] _result = ga.Result;
              foreach (Node _node in _nodes)
              {
                  _node.Visited = true;
                  _node.Color = _brushes[_result[_node.Mark]];
                  PaintNode(_node);
              }
             time.Stop();
             var resultTime = time.ElapsedMilliseconds;
             tbl.Rows.Add("Genetic Algorithm", _nodes.Count, _edges_Grage(), resultTime.ToString(), ga.Iteration);
            }
        }

        private void listView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbl.Clear();
        }

        private int _edges_Grage()
        {
            int previous = 0;
            foreach (Node _node in _nodes)
            {
                if (_node.Edges.Count>previous)
                {
                    previous = _node.Edges.Count;
                }

            }
            return previous;

        }

        private void bHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"Help.txt");

        }
    }
}
