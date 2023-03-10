using System.Collections.Immutable;

namespace eMP_PR1;

public class IrregularMesh : Mesh
{
   private List<double> _allLinesX;
   private List<double> _allLinesY;
   private List<Node2D> _nodes;
   private int[] _splitsX;
   private int[] _splitsY;
   private double[] _kX;
   private double[] _kY;
   private (int, double, double, int, int, int, int)[] _areas;
   public override ImmutableArray<double> LinesX { get; init; }
   public override ImmutableArray<double> LinesY { get; init; }
   public override ImmutableList<Node2D> Nodes => _nodes.ToImmutableList();
   public override ImmutableList<double> AllLinesX => _allLinesX.ToImmutableList();
   public override ImmutableList<double> AllLinesY => _allLinesY.ToImmutableList();
   public override ImmutableArray<(int, double, double, int, int, int, int)> Areas => _areas.ToImmutableArray();
   public ImmutableArray<int> SplitsX => _splitsX.ToImmutableArray();
   public ImmutableArray<int> SplitsY => _splitsY.ToImmutableArray();
   public ImmutableArray<double> KX => _kX.ToImmutableArray();
   public ImmutableArray<double> KY => _kY.ToImmutableArray();

   public IrregularMesh(string path)
   {
      try
      {
         using (var sr = new StreamReader(path))
         {
            LinesX = sr.ReadLine().Split().Select(value => double.Parse(value)).ToImmutableArray();
            LinesY = sr.ReadLine().Split().Select(value => double.Parse(value)).ToImmutableArray();
            _splitsX = sr.ReadLine().Split().Select(value => int.Parse(value)).ToArray();
            _splitsY = sr.ReadLine().Split().Select(value => int.Parse(value)).ToArray();
            _kX = sr.ReadLine().Split().Select(value => double.Parse(value)).ToArray();
            _kY = sr.ReadLine().Split().Select(value => double.Parse(value)).ToArray();
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
      // Разбиение каждой области в соответствии с её параметрами:
      // количеством разбиений
      // коэффициенте разрядки

      // По оси X
      for (int i = 0; i < LinesX.Length - 1; i++)
      {
         double h;
         double sum = 0;
         double lenght = LinesX[i + 1] - LinesX[i];

         for (int k = 0; k < _splitsX[i]; k++)
            sum += Math.Pow(_kX[i], k);

         h = lenght / sum;

         _allLinesX.Add(LinesX[i]);

         while (Math.Round(_allLinesX.Last() + h, 1) < LinesX[i + 1])
         {
            _allLinesX.Add(_allLinesX.Last() + h);
            h *= _kX[i];
         }
      }
      _allLinesX.Add(LinesX.Last());

      // По оси Y
      for (int i = 0; i < LinesY.Length - 1; i++)
      {
         double h;
         double sum = 0;
         double lenght = LinesY[i + 1] - LinesY[i];

         for (int k = 0; k < _splitsY[i]; k++)
            sum += Math.Pow(_kY[i], k);

         h = lenght / sum;

         _allLinesY.Add(LinesY[i]);

         while (Math.Round(_allLinesY.Last() + h, 1) < LinesY[i + 1])
         {
            _allLinesY.Add(_allLinesY.Last() + h);
            h *= _kY[i];
         }
      }
      _allLinesY.Add(LinesY.Last());

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
