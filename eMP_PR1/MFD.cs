using System.Xml.Linq;

namespace eMP_PR1;

public class MFD
{
   // Сетка, состоящая из точек.
   private readonly Mesh _Mesh;

   // Исходная функция и правая часть, вынес в класс.
   private ITest Test;

   // СЛАУ. На входе матрица, которая строится по сетке,
   // и правая часть. На выходе вектор значений в точках.
   private Matrix5Diags Matrix;
   private double[] RightPart;
   private double[] U;

   // Метод решения полученной СЛАУ с 5-диагональной матрицей.
   private ISolver Solver;

   // Отрезки с краевыми условиями.
   private readonly Boundary[] Boundaries;
   private readonly double Beta;

   public MFD(Mesh mesh, string boundaryPath)
   {
      try
      {
         using (var sr = new StreamReader(boundaryPath))
         {
            Beta = double.Parse(sr.ReadLine());
            Boundaries = sr.ReadToEnd().Split("\n")
            .Select(str => Boundary.BoundaryParse(str)).ToArray();
         }
         _Mesh = mesh;
      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   public void SetTest(ITest test) => Test = test;

   public void SetMethodSolvingSLAE(ISolver solver) => Solver = solver;

   public void Compute()
   {
      try
      {
         if (Test is null)
            throw new Exception("Ошибка: не выбран тест.");

         if (Solver is null)
            throw new Exception("Ошибка: не выбран метод решения СЛАУ.");

         // Сначала ставим сетку с файлов.
         _Mesh.Build();

         // Потом устанавливаем краевые условия на ней.
         _Mesh.SetBoundaryConditions(Boundaries);

         // Даём память всей СЛАУ.
         Initialization();

         // Строим матрицу по методу конечных разностей.
         // Применяем пятиточечный разностный оператор.
         BuildMatrix();

         // Решаем слау выбранным методом (Гаусс - Зейдель с параметром релаксации).
         U = Solver.Compute(Matrix, RightPart);

      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   private void Initialization()
   {
      Matrix = new(_Mesh.Nodes.Count, (_Mesh.AllLinesX.Count > _Mesh.AllLinesX.Count) ?
      _Mesh.AllLinesX.Count - 2 : _Mesh.AllLinesY.Count - 2);
      RightPart = new double[Matrix.Size];
      U = new double[Matrix.Size];
   }

   public void OutputResultU()
   {
      using (var sw = new StreamWriter("Output/ResultU.txt"))
      {
         for (int i = 0; i < U.Length; i++)
            sw.WriteLine(U[i].ToString("e15"));
      }
   }

   private void BuildMatrix()
   {
      double hx, hy, hix, hiy, hi;
      double lambda, gamma;
      double us, ubeta;
      double leftDerivative, rightDerivative;
      NormalDirection normalDirection;

      // Для каждого узла 
      for (int i = 0; i < _Mesh.Nodes.Count; i++)
      {
         // сверяем тип с кейсами.
         switch (_Mesh.Nodes[i].NT)
         {
            // Если узел граничный: 
            case NodeType.Boundary:

               // Смотрим на тип краевых условий
               switch (_Mesh.Nodes[i].BT)
               {
                  case BoundaryType.Dirichlet:

                     // На диагонали единицу, в правой части значение функции.
                     Matrix.Diags[0][i] = 1;
                     RightPart[i] = Test.U(_Mesh.Nodes[i]);

                     break;

                  case BoundaryType.Neumann:

                     lambda = _Mesh.Areas[_Mesh.Nodes[i].AreaNumber].Item2;

                     normalDirection = _Mesh.Normal(_Mesh.Nodes[i]);
                     switch (normalDirection)
                     {
                        case NormalDirection.LeftX:

                           hi = _Mesh.AllLinesX[_Mesh.Nodes[i].I + 1] - _Mesh.AllLinesX[_Mesh.Nodes[i].I];
                           Matrix.Diags[0][i] = lambda / hi;
                           Matrix.Diags[4][i] = -lambda / hi;
                           RightPart[i] = -lambda * RightDerivativeX(_Mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.BottomY:

                           hi = _Mesh.AllLinesY[_Mesh.Nodes[i].J + 1] - _Mesh.AllLinesY[_Mesh.Nodes[i].J];
                           Matrix.Diags[0][i] = lambda / hi;
                           Matrix.Diags[3][i] = -lambda / hi;
                           RightPart[i] = -lambda * RightDerivativeY(_Mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.RightX:

                           hi = _Mesh.AllLinesX[_Mesh.Nodes[i].I] - _Mesh.AllLinesX[_Mesh.Nodes[i].I - 1];
                           Matrix.Diags[0][i] = lambda / hi;
                           Matrix.Diags[2][i + Matrix.Indexes[2]] = -lambda / hi;
                           RightPart[i] = lambda * LeftDerivativeX(_Mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.UpperY:

                           hi = _Mesh.AllLinesY[_Mesh.Nodes[i].J] - _Mesh.AllLinesY[_Mesh.Nodes[i].J - 1];
                           Matrix.Diags[0][i] = lambda / hi;
                           Matrix.Diags[1][i + Matrix.Indexes[1]] = -lambda / hi;
                           RightPart[i] = lambda * LeftDerivativeY(_Mesh.Nodes[i], hi);

                           break;

                        default:
                           throw new ArgumentOutOfRangeException(nameof(normalDirection),
                           $"Несуществующее направление нормали: {normalDirection}");
                     }

                     break;

                  case BoundaryType.Third:

                     lambda = _Mesh.Areas[_Mesh.Nodes[i].AreaNumber].Item2;

                     normalDirection = _Mesh.Normal(_Mesh.Nodes[i]);
                     us = Test.U(_Mesh.Nodes[i]);

                     switch (normalDirection)
                     {
                        case NormalDirection.LeftX:

                           hi = _Mesh.AllLinesX[_Mesh.Nodes[i].I + 1] - _Mesh.AllLinesX[_Mesh.Nodes[i].I];
                           rightDerivative = RightDerivativeX(_Mesh.Nodes[i], hi);
                           ubeta = -lambda * rightDerivative / Beta + us;
                           Matrix.Diags[0][i] = lambda / hi + Beta;
                           Matrix.Diags[4][i] = -lambda / hi;
                           RightPart[i] = -lambda * rightDerivative + Beta * (us - ubeta) + Beta * ubeta;

                           break;

                        case NormalDirection.BottomY:

                           hi = _Mesh.AllLinesY[_Mesh.Nodes[i].J + 1] - _Mesh.AllLinesY[_Mesh.Nodes[i].J];
                           rightDerivative = RightDerivativeY(_Mesh.Nodes[i], hi);
                           ubeta = -lambda * rightDerivative / Beta + us;
                           Matrix.Diags[0][i] = lambda / hi + Beta;
                           Matrix.Diags[3][i] = -lambda / hi;
                           RightPart[i] = -lambda * rightDerivative + Beta * (us - ubeta) + Beta * ubeta;

                           break;

                        case NormalDirection.RightX:

                           hi = _Mesh.AllLinesX[_Mesh.Nodes[i].I] - _Mesh.AllLinesX[_Mesh.Nodes[i].I - 1];
                           leftDerivative = LeftDerivativeX(_Mesh.Nodes[i], hi);
                           ubeta = lambda * leftDerivative / Beta + us;
                           Matrix.Diags[0][i] = lambda / hi + Beta;
                           Matrix.Diags[2][i + Matrix.Indexes[2]] = -lambda / hi;
                           RightPart[i] = lambda * leftDerivative + Beta * (us - ubeta) + Beta * ubeta;

                           break;

                        case NormalDirection.UpperY:

                           hi = _Mesh.AllLinesY[_Mesh.Nodes[i].J] - _Mesh.AllLinesY[_Mesh.Nodes[i].J - 1];
                           leftDerivative = LeftDerivativeY(_Mesh.Nodes[i], hi);
                           ubeta = lambda * leftDerivative / Beta + us;
                           Matrix.Diags[0][i] = lambda / hi + Beta;
                           Matrix.Diags[1][i + Matrix.Indexes[1]] = -lambda / hi;
                           RightPart[i] = lambda * leftDerivative + Beta * (us - ubeta) + Beta * ubeta;

                           break;

                        default:
                           throw new ArgumentOutOfRangeException(nameof(normalDirection),
                           $"Несуществующее направление нормали: {normalDirection}");
                     }

                     break;


                  default:
                     throw new ArgumentOutOfRangeException(nameof(BoundaryType),
                     $"Несуществующий тип краевых условий: {_Mesh.Nodes[i].BT}");
               }

               break;

            case NodeType.Inner:

               hx = _Mesh.AllLinesX[_Mesh.Nodes[i].I + 1] - _Mesh.AllLinesX[_Mesh.Nodes[i].I];
               hy = _Mesh.AllLinesY[_Mesh.Nodes[i].J + 1] - _Mesh.AllLinesY[_Mesh.Nodes[i].J];

               (lambda, gamma) =
               (_Mesh.Areas[_Mesh.Nodes[i].AreaNumber].Item2,
               _Mesh.Areas[_Mesh.Nodes[i].AreaNumber].Item3);

               RightPart[i] = Test.F(_Mesh.Nodes[i]);

               if (_Mesh is RegularMesh)
               {
                  Matrix.Diags[0][i] = lambda * (2.0 / (hx * hx) + 2.0 / (hy * hy)) + gamma;
                  Matrix.Diags[3][i] = -lambda / (hy * hy);
                  Matrix.Diags[4][i] = -lambda / (hx * hx);
                  Matrix.Diags[1][i + Matrix.Indexes[1]] = -lambda / (hy * hy);
                  Matrix.Diags[2][i + Matrix.Indexes[2]] = -lambda / (hx * hx);
               }
               else
               {
                  hix = _Mesh.AllLinesX[_Mesh.Nodes[i].I] - _Mesh.AllLinesX[_Mesh.Nodes[i].I - 1];
                  hiy = _Mesh.AllLinesY[_Mesh.Nodes[i].J] - _Mesh.AllLinesY[_Mesh.Nodes[i].J - 1];

                  Matrix.Diags[0][i] = lambda * (2.0 / (hix * hx) + 2.0 / (hiy * hy)) + gamma;
                  Matrix.Diags[2][i + Matrix.Indexes[2]] = -lambda * 2.0 / (hix * (hx + hix));
                  Matrix.Diags[1][i + Matrix.Indexes[1]] = -lambda * 2.0 / (hiy * (hy + hiy));
                  Matrix.Diags[4][i] = -lambda * 2.0 / (hx * (hx + hix));
                  Matrix.Diags[3][i] = -lambda * 2.0 / (hy * (hy + hiy));
               }

               break;

            case NodeType.Fake:

               Matrix.Diags[0][i] = 1;
               RightPart[i] = 0;

               break;

            default:
               throw new ArgumentOutOfRangeException(nameof(NodeType),
               $"Несуществующий тип узла: {_Mesh.Nodes[i].NT}");
         }
      }
   }

   private double LeftDerivativeX(Node2D point, double h)
       => (Test.U(point) - Test.U(point - (h, 0))) / h;

   private double RightDerivativeX(Node2D point, double h)
       => (Test.U(point + (h, 0)) - Test.U(point)) / h;

   private double LeftDerivativeY(Node2D point, double h)
       => (Test.U(point) - Test.U(point - (0, h))) / h;

   private double RightDerivativeY(Node2D point, double h)
       => (Test.U(point + (0, h)) - Test.U(point)) / h;
}