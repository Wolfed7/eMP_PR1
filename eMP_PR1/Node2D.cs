namespace eMP_PR1;

public enum BoundaryType
{
   Dirichlet,  // Краевое первого типа.
   Neumann     // Второго.
}

public enum NodeType
{
   Boundary,
   Inner,
   Fake
}

public class Node2D
{
   // Положение в декартовых координатах.
   public double X { get; init; }
   public double Y { get; init; }

   // Положение в матрице МКР.
   public int I { get; init; }
   public int J { get; init; }

   public NodeType NT { get; set; } 

   public BoundaryType BT { get; set; }
}
