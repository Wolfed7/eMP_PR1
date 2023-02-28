namespace eMP_PR1;

public class MFD
{
   // Сетка, состоящая из точек.
   private readonly Mesh _mesh;

   // СЛАУ. На входе матрица, которая строится по сетке,
   // и правая часть. На выходе вектор значений в точках.
   private Matrix5Diags _matrix;
   private double[] _pr;
   private double[] _q;

   private ITest _test;

   // Метод решения полученной СЛАУ с 5-диагональной матрицей.
   private ISolver _solver;

   private readonly Boundary[] _boundaries;
   private readonly double _beta;
   public double[] Weights
       => _q;

   public MFD(Mesh grid, string boundaryPath)
   {
      try
      {
         using (var sr = new StreamReader(boundaryPath))
         {
            _beta = double.Parse(sr.ReadLine());
            _boundaries = sr.ReadToEnd().Split("\n")
            .Select(str => Boundary.BoundaryParse(str)).ToArray();
         }

         _mesh = grid;

      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   public void SetTest(ITest test)
       => _test = test;

   public void SetMethodSolvingSLAE(ISolver solver)
       => _solver = solver;

   public void Compute()
   {
      try
      {
         if (_test is null)
            throw new Exception("Тест не выбран.");

         if (_solver is null)
            throw new Exception("Метод решения СЛАУ не выбран.");

         _mesh.Build();
         _mesh.AssignBoundaryConditions(_boundaries);
         Init();
         BuildMatrix();
         _q = _solver.Compute(_matrix, _pr);

      }
      catch (Exception ex)
      {
         Console.WriteLine(ex.Message);
      }
   }

   private void Init()
   {
      _matrix = new(_mesh.Nodes.Count, (_mesh.AllLinesX.Count > _mesh.AllLinesX.Count) ?
      _mesh.AllLinesX.Count - 2 : _mesh.AllLinesY.Count - 2);
      _pr = new double[_matrix.Size];
      _q = new double[_matrix.Size];
   }

   private void BuildMatrix()
   {
      double hx, hy, hix, hiy, hi;
      double lambda, gamma;
      double us, ubeta;
      double leftDerivative, rightDerivative;
      NormalDirection normalType;

      for (int i = 0; i < _mesh.Nodes.Count; i++)
      {
         switch (_mesh.Nodes[i].NT)
         {
            case NodeType.Boundary:

               switch (_mesh.Nodes[i].BT)
               {
                  case BoundaryType.Dirichlet:

                     _matrix.Diags[0][i] = 1;
                     _pr[i] = _test.U(_mesh.Nodes[i]);

                     break;

                  case BoundaryType.Neumann:

                     lambda = _mesh.Areas[_mesh.Nodes[i].AreaNumber].Item2;

                     normalType = _mesh.Normal(_mesh.Nodes[i]);

                     switch (normalType)
                     {
                        case NormalDirection.LeftX:

                           hi = _mesh.AllLinesX[_mesh.Nodes[i].I + 1] - _mesh.AllLinesX[_mesh.Nodes[i].I];
                           _matrix.Diags[0][i] = lambda / hi;
                           _matrix.Diags[4][i] = -lambda / hi;
                           _pr[i] = -lambda * RightDerivativeX(_mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.BottomY:

                           hi = _mesh.AllLinesY[_mesh.Nodes[i].J + 1] - _mesh.AllLinesY[_mesh.Nodes[i].J];
                           _matrix.Diags[0][i] = lambda / hi;
                           _matrix.Diags[3][i] = -lambda / hi;
                           _pr[i] = -lambda * RightDerivativeY(_mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.RightX:

                           hi = _mesh.AllLinesX[_mesh.Nodes[i].I] - _mesh.AllLinesX[_mesh.Nodes[i].I - 1];
                           _matrix.Diags[0][i] = lambda / hi;
                           _matrix.Diags[2][i + _matrix.Indexes[2]] = -lambda / hi;
                           _pr[i] = lambda * LeftDerivativeX(_mesh.Nodes[i], hi);

                           break;

                        case NormalDirection.UpperY:

                           hi = _mesh.AllLinesY[_mesh.Nodes[i].J] - _mesh.AllLinesY[_mesh.Nodes[i].J - 1];
                           _matrix.Diags[0][i] = lambda / hi;
                           _matrix.Diags[1][i + _matrix.Indexes[1]] = -lambda / hi;
                           _pr[i] = lambda * LeftDerivativeY(_mesh.Nodes[i], hi);

                           break;

                        default:
                           throw new ArgumentOutOfRangeException(nameof(normalType),
                           $"Несуществующее направление нормали: {normalType}");
                     }

                     break;

                  case BoundaryType.Third:

                     lambda = _mesh.Areas[_mesh.Nodes[i].AreaNumber].Item2;

                     normalType = _mesh.Normal(_mesh.Nodes[i]);
                     us = _test.U(_mesh.Nodes[i]);

                     switch (normalType)
                     {
                        case NormalDirection.LeftX:

                           hi = _mesh.AllLinesX[_mesh.Nodes[i].I + 1] - _mesh.AllLinesX[_mesh.Nodes[i].I];
                           rightDerivative = RightDerivativeX(_mesh.Nodes[i], hi);
                           ubeta = -lambda * rightDerivative / _beta + us;
                           _matrix.Diags[0][i] = lambda / hi + _beta;
                           _matrix.Diags[4][i] = -lambda / hi;
                           _pr[i] = -lambda * rightDerivative + _beta * (us - ubeta) + _beta * ubeta;

                           break;

                        case NormalDirection.BottomY:

                           hi = _mesh.AllLinesY[_mesh.Nodes[i].J + 1] - _mesh.AllLinesY[_mesh.Nodes[i].J];
                           rightDerivative = RightDerivativeY(_mesh.Nodes[i], hi);
                           ubeta = -lambda * rightDerivative / _beta + us;
                           _matrix.Diags[0][i] = lambda / hi + _beta;
                           _matrix.Diags[3][i] = -lambda / hi;
                           _pr[i] = -lambda * rightDerivative + _beta * (us - ubeta) + _beta * ubeta;

                           break;

                        case NormalDirection.RightX:

                           hi = _mesh.AllLinesX[_mesh.Nodes[i].I] - _mesh.AllLinesX[_mesh.Nodes[i].I - 1];
                           leftDerivative = LeftDerivativeX(_mesh.Nodes[i], hi);
                           ubeta = lambda * leftDerivative / _beta + us;
                           _matrix.Diags[0][i] = lambda / hi + _beta;
                           _matrix.Diags[2][i + _matrix.Indexes[2]] = -lambda / hi;
                           _pr[i] = lambda * leftDerivative + _beta * (us - ubeta) + _beta * ubeta;

                           break;

                        case NormalDirection.UpperY:

                           hi = _mesh.AllLinesY[_mesh.Nodes[i].J] - _mesh.AllLinesY[_mesh.Nodes[i].J - 1];
                           leftDerivative = LeftDerivativeY(_mesh.Nodes[i], hi);
                           ubeta = lambda * leftDerivative / _beta + us;
                           _matrix.Diags[0][i] = lambda / hi + _beta;
                           _matrix.Diags[1][i + _matrix.Indexes[1]] = -lambda / hi;
                           _pr[i] = lambda * leftDerivative + _beta * (us - ubeta) + _beta * ubeta;

                           break;

                        default:
                           throw new ArgumentOutOfRangeException(nameof(normalType),
                           $"Несуществующее направление нормали: {normalType}");
                     }

                     break;


                  default:
                     throw new ArgumentOutOfRangeException(nameof(BoundaryType),
                     $"Несуществующий тип краевых условий: {_mesh.Nodes[i].BT}");
               }

               break;

            case NodeType.Inner:

               hx = _mesh.AllLinesX[_mesh.Nodes[i].I + 1] - _mesh.AllLinesX[_mesh.Nodes[i].I];
               hy = _mesh.AllLinesY[_mesh.Nodes[i].J + 1] - _mesh.AllLinesY[_mesh.Nodes[i].J];

               (lambda, gamma) =
               (_mesh.Areas[_mesh.Nodes[i].AreaNumber].Item2,
               _mesh.Areas[_mesh.Nodes[i].AreaNumber].Item3);

               _pr[i] = _test.F(_mesh.Nodes[i]);

               if (_mesh is RegularMesh)
               {
                  _matrix.Diags[0][i] = lambda * (2.0 / (hx * hx) + 2.0 / (hy * hy)) + gamma;
                  _matrix.Diags[3][i] = -lambda / (hy * hy);
                  _matrix.Diags[4][i] = -lambda / (hx * hx);
                  _matrix.Diags[1][i + _matrix.Indexes[1]] = -lambda / (hy * hy);
                  _matrix.Diags[2][i + _matrix.Indexes[2]] = -lambda / (hx * hx);
               }
               else
               {
                  hix = _mesh.AllLinesX[_mesh.Nodes[i].I] - _mesh.AllLinesX[_mesh.Nodes[i].I - 1];
                  hiy = _mesh.AllLinesY[_mesh.Nodes[i].J] - _mesh.AllLinesY[_mesh.Nodes[i].J - 1];

                  _matrix.Diags[0][i] = lambda * (2.0 / (hix * hx) + 2.0 / (hiy * hy)) + gamma;
                  _matrix.Diags[2][i + _matrix.Indexes[2]] = -lambda * 2.0 / (hix * (hx + hix));
                  _matrix.Diags[1][i + _matrix.Indexes[1]] = -lambda * 2.0 / (hiy * (hy + hiy));
                  _matrix.Diags[4][i] = -lambda * 2.0 / (hx * (hx + hix));
                  _matrix.Diags[3][i] = -lambda * 2.0 / (hy * (hy + hiy));
               }

               break;

            case NodeType.Fake:

               _matrix.Diags[0][i] = 1;
               _pr[i] = 0;

               break;

            default:
               throw new ArgumentOutOfRangeException(nameof(NodeType),
               $"Несуществующий тип узла: {_mesh.Nodes[i].NT}");
         }
      }
   }

   private double LeftDerivativeX(Node2D point, double h)
       => (_test.U(point) - _test.U(point - (h, 0))) / h;

   private double LeftDerivativeY(Node2D point, double h)
       => (_test.U(point) - _test.U(point - (0, h))) / h;

   private double RightDerivativeX(Node2D point, double h)
       => (_test.U(point + (h, 0)) - _test.U(point)) / h;

   private double RightDerivativeY(Node2D point, double h)
       => (_test.U(point + (0, h)) - _test.U(point)) / h;
}