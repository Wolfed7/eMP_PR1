using System.Collections.Immutable;
namespace eMP_PR1;

public class RegularMesh : Mesh
{

   private List<double> _allLinesX;
   private List<double> _allLinesY;
   private List<Node2D> _nodes;
   private (int, double, double, int, int, int, int)[] _areas;
   public override ImmutableArray<double> LinesX { get; init; }
   public override ImmutableArray<double> LinesY { get; init; }
   public override ImmutableList<double> AllLinesX => _allLinesX.ToImmutableList();
   public override ImmutableList<double> AllLinesY => _allLinesY.ToImmutableList();
   public override ImmutableList<Node2D> Nodes => _nodes.ToImmutableList();
   public override ImmutableArray<(int, double, double, int, int, int, int)> Areas => _areas.ToImmutableArray();

   // Количество разбиений по X, Y осям.
   public int SplitsX { get; init; }
   public int SplitsY { get; init; }

   public RegularMesh(string path)
   {
      try
      {
         using (var sr = new StreamReader(path))
         {
            LinesX = sr.ReadLine().Split().Select(value => double.Parse(value)).ToImmutableArray();
            LinesY = sr.ReadLine().Split().Select(value => double.Parse(value)).ToImmutableArray();
            SplitsX = int.Parse(sr.ReadLine());
            SplitsY = int.Parse(sr.ReadLine());
            _areas = sr.ReadToEnd().Split("\n").Select(row => row.Split())
            .Select(value => (int.Parse(value[0]), double.Parse(value[1]), double.Parse(value[2]),
            int.Parse(value[3]), int.Parse(value[4]), int.Parse(value[5]), int.Parse(value[6]))).ToArray();
         }

         _allLinesX = new();
         _allLinesY = new();
         _nodes = new();
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   public override void Build()
   {
      // Разбиение по X
      double lenght = LinesX.Last() - LinesX.First();
      double h = lenght / SplitsX;

      _allLinesX.Add(LinesX.First());
      while (Math.Round(_allLinesX.Last() + h, 1) < LinesX.Last())
         _allLinesX.Add(_allLinesX.Last() + h);
      _allLinesX = _allLinesX.Union(LinesX).OrderBy(value => value).ToList();

      // Разбиение по Y
      lenght = LinesY.Last() - LinesY.First();
      h = lenght / SplitsY;

      _allLinesY.Add(LinesY.First());
      while (Math.Round(_allLinesY.Last() + h, 1) < LinesY.Last())
         _allLinesY.Add(_allLinesY.Last() + h);
      _allLinesY = _allLinesY.Union(LinesY).OrderBy(value => value).ToList();

      // Сборка массива узлов.
      for (int i = 0; i < _allLinesX.Count; i++)
         for (int j = 0; j < _allLinesY.Count; j++)
            _nodes.Add(new(_allLinesX[i], _allLinesY[j], i, j, 
               NodeTypesWithoutInternalCheck(_allLinesX[i], _allLinesY[j])));

      InternalCheck();
      SetAreaNumber();
      WriteToFilePoints();
   }
}

