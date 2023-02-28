namespace eMP_PR1;

public interface ISolver
{
   public int MaxIters { get; init; }
   public double Eps { get; init; }
   public double W { get; init; }

   public double[] Compute(Matrix5Diags diagMatrix, double[] pr);
}

public record GaussSeidel(int MaxIters, double Eps, double W) : ISolver
{
   public double[] Compute(Matrix5Diags matrix, double[] rightPart)
   {
      double[] qk = new double[matrix.Size];
      double[] qk1 = new double[matrix.Size];
      double[] residual = new double[matrix.Size];
      double rightPartNorm = rightPart.Norm();

      for (int i = 0; i < MaxIters; i++)
      {
         for (int k = 0; k < matrix.Size; k++)
         {
            double fstSum = MultLine(matrix, k, qk1, 1);
            double scdSum = MultLine(matrix, k, qk, 2);

            residual[k] = rightPart[k] - (fstSum + scdSum);
            qk1[k] = qk[k] + W * residual[k] / matrix.Diags[0][k];
         }

         Array.Copy(qk1, qk, qk1.Length);
         Array.Clear(qk1, 0, qk1.Length);

         if (residual.Norm() / rightPartNorm < Eps)
            break;
      }

      return qk;
   }

   private static double MultLine(Matrix5Diags diagMatrix, int i, double[] vector, int method)
   {
      double sum = 0;

      if (method == 0 || method == 1)
      {
         if (i > 0)
         {
            sum += diagMatrix.Diags[1][i - 1] * vector[i - 1];

            if (i > diagMatrix.ZeroDiags + 1)
               sum += diagMatrix.Diags[2][i - diagMatrix.ZeroDiags - 2] * vector[i - diagMatrix.ZeroDiags - 2];
         }
      }

      if (method == 0 || method == 2)
      {
         sum += diagMatrix.Diags[0][i] * vector[i];

         if (i < diagMatrix.Size - 1)
         {
            sum += diagMatrix.Diags[3][i] * vector[i + 1];

            if (i < diagMatrix.Size - diagMatrix.ZeroDiags - 2)
               sum += diagMatrix.Diags[4][i] * vector[i + diagMatrix.ZeroDiags + 2];
         }
      }

      return sum;
   }
}