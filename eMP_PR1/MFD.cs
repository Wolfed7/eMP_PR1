namespace eMP_PR1;

public class MFD
{
   // Сетка, состоящая из точек.
   private readonly Mesh SettedMesh;

   // СЛАУ. На входе матрица, которая строится по сетке,
   // и правая часть. На выходе вектор значений в точках.
   private Matrix5Diags Matrix;
   private double[] RightPart;
   private double[] Solushion;

   // Метод решения полученной СЛАУ с 5-диагональной матрицей.
   private ISolver Solver;
}