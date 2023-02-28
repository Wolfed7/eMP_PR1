namespace eMP_PR1;

/*    
 *    Индексы диагоналей, где 
 *    m - количество нулевых диагоналей.
 *    
 *          0 1         2 + m
 *   
 *         |1 1 -     ... 1 -|
 *     -1  |1 1 1 -   ... - 1|
 *         |- 1 1 1 - ... - -| 
 *         |...  ...  ... ...|
 * -2 - m  |1 -       ...    |
 *         |- 1 -     ...    |
 */

public class Matrix5Diags
{
   public double[][] Diags { get; set; }
   public int[] Indexes { get; init; }
   public int Size { get; init; }
   public int ZeroDiags { get; init; }

   public Matrix5Diags(int nodesNumber, int zeroDiagsNumber)
   {
      Size = nodesNumber;
      ZeroDiags = zeroDiagsNumber;

      Diags = new double[5][];
      Diags[0] = new double[nodesNumber];
      Diags[1] = new double[nodesNumber - 1];
      Diags[2] = new double[nodesNumber - zeroDiagsNumber - 2];
      Diags[3] = new double[nodesNumber - 1];
      Diags[4] = new double[nodesNumber - zeroDiagsNumber - 2];
      Indexes = new int[] { 0, -1, -2 - zeroDiagsNumber, 1, 2 + zeroDiagsNumber };
   }
}