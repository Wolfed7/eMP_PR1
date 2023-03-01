using System.Collections.Immutable;
namespace eMP_PR1;

public enum NormalDirection
{
   None,
   LeftX,
   RightX,
   UpperY,
   BottomY
}

public abstract class Mesh
{
   public abstract ImmutableArray<double> LinesX { get; init; }
   public abstract ImmutableArray<double> LinesY { get; init; }
   public abstract ImmutableList<double> AllLinesX { get; }
   public abstract ImmutableList<double> AllLinesY { get; }
   public abstract ImmutableList<Node2D> Nodes { get; }

   // Номер области,
   // Лямбда,
   // Гамма,
   // левая граница X,
   // правая граница X,
   // нижняя граница Y,
   // верхняя граница Y. 
   public abstract ImmutableArray<(int, double, double, int, int, int, int)> Areas { get; }

   public abstract void Build();

   public NormalDirection Normal(Node2D node)
   {
      if (node.NT != NodeType.Boundary)
         throw new Exception($"Точка ({node.X}, {node.Y}) не является граничной.");

      var normalDirection = NormalDirection.None;

      foreach (var area in Areas)
      {
         normalDirection = node switch
         {
            _ when node.X == LinesX[area.Item4] => NormalDirection.LeftX,
            _ when node.X == LinesX[area.Item5] => NormalDirection.RightX,
            _ when node.Y == LinesY[area.Item6] => NormalDirection.BottomY,
            _ when node.Y == LinesY[area.Item7] => NormalDirection.UpperY,
      
            _ => NormalDirection.None
         };

         if (normalDirection != NormalDirection.None)
         {
            break;
         }
      }

      return normalDirection;
   }

   protected NodeType NodeTypesWithoutInternalCheck(double x, double y)
   {
      foreach (var area in Areas)
      {
         if (x >= LinesX[area.Item4] && x <= LinesX[area.Item5] && y >= LinesY[area.Item6] && y <= LinesY[area.Item7])
         {
            return NodeType.Boundary;
         }
      }

      return NodeType.Fake;
   }

   // Метод определяет и присваивает тип внутренним узлам сетки.
   // Внутренним узлом считается узел, находящийся в центре
   // пятиточечного разностного оператора.
   protected void InternalCheck()
   {
      foreach (var node in Nodes.Where(node => node.NT != NodeType.Fake))
      {
         if (Nodes.Any(lstNode => lstNode.I == node.I - 1 && lstNode.J == node.J && lstNode.NT != NodeType.Fake) &&
             Nodes.Any(lstNode => lstNode.I == node.I + 1 && lstNode.J == node.J && lstNode.NT != NodeType.Fake) &&
             Nodes.Any(lstNode => lstNode.J == node.J - 1 && lstNode.I == node.I && lstNode.NT != NodeType.Fake) &&
             Nodes.Any(lstNode => lstNode.J == node.J + 1 && lstNode.I == node.I && lstNode.NT != NodeType.Fake))
         {
            node.NT = NodeType.Inner;
         }
      }
   }

   // Устанавливает номер подобласти для каждого узла сетки.
   protected void SetAreaNumber()
   {
      for (int i = 0; i < Nodes.Count; i++)
         for (int iArea = 0; iArea < Areas.Length; iArea++)
            if (Nodes[i].X >= Areas[iArea].Item4 && Nodes[i].X <= Areas[iArea].Item5 &&
                Nodes[i].Y >= Areas[iArea].Item6 && Nodes[i].Y <= Areas[iArea].Item7)
            {
               Nodes[i].AreaNumber = Areas[iArea].Item1;
            }
   }

   // Устанавливает тип краевых условий для всех граничных узлов сетки
   // в соответствии с входными параметрами.
   public void SetBoundaryConditions(Boundary[] boundaries)
   {
      foreach (var node in Nodes.Where(node => node.NT == NodeType.Boundary))
         for (int k = 0; k < boundaries.Length; k++)
            if (node.X >= LinesX[boundaries[k].X1] && node.X <= LinesX[boundaries[k].X2] &&
                node.Y >= LinesY[boundaries[k].Y1] && node.Y <= LinesY[boundaries[k].Y2])
            {
               node.BT = boundaries[k].BoundaryType;
               break;
            }
   }

   // Вывод в файлы. Нужны для отрисовки области.
   protected void WriteToFilePoints()
   {
      using (var sw = new StreamWriter("Nodes/boundaryNodes.txt"))
      {
         Nodes.ForEach(x => { if (x.NT == NodeType.Boundary) sw.WriteLine(x); });
      }

      using (var sw = new StreamWriter("Nodes/innerNodes.txt"))
      {
         Nodes.ForEach(x => { if (x.NT == NodeType.Inner) sw.WriteLine(x); });
      }

      using (var sw = new StreamWriter("Nodes/FakeNodes.txt"))
      {
         Nodes.ForEach(x => { if (x.NT == NodeType.Fake) sw.WriteLine(x); });
      }
   }
}

