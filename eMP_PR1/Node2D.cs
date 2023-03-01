namespace eMP_PR1;

public enum BoundaryType
{
   Dirichlet,  // Краевое первого типа.
   Neumann,    // Второго.
   Third       // Третьего.
}

public enum NodeType
{
   Boundary,   // Точки делятся на граничные,
   Inner,      // внутренние (со всех сторон по существующей точке)
   Fake        // и ложные (храним в матрице для сохранения формата)
}

public class Node2D
{
   // Положение в декартовых координатах.
   public double X { get; init; }
   public double Y { get; init; }

   public int I { get; init; }
   public int J { get; init; }

   public NodeType NT { get; set; } 

   public BoundaryType BT { get; set; }

   public int AreaNumber { get; set; }

   public Node2D(double x, double y, int i, int j, NodeType nodeType)
   {
      X = x;
      Y = y;
      I = i;
      J = j;
      NT = nodeType;
   }

   public static Node2D Parse(string nodeStr)
   {
      var data = nodeStr.Split();
      Node2D node = new(double.Parse(data[0]), double.Parse(data[1]),
      int.Parse(data[2]), int.Parse(data[3]), (NodeType)Enum.Parse(typeof(NodeType), data[4]));

      return node;
   }

   public static Node2D operator +(Node2D node, (double, double) value)
    => new(node.X + value.Item1, node.Y + value.Item2, node.I, node.J, node.NT);

   public static Node2D operator -(Node2D node, (double, double) value)
       => new(node.X - value.Item1, node.Y - value.Item2, node.I, node.J, node.NT);

   public override string ToString()
       => $"({X}, {Y})";
}

