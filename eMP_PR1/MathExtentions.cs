namespace eMP_PR1;

public static class MathExtentions
{
   public static double Norm(this double[] array)
   {
      double result = 0;

      for (int i = 0; i < array.Length; i++)
         result += array[i] * array[i];

      return Math.Sqrt(result);
   }
}