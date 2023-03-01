using eMP_PR1;

public readonly record struct Boundary(BoundaryType BoundaryType, int X1, int X2, int Y1, int Y2)
{
   // Парсит данные с файла по строке, формат:
   // тип краевых,
   // левая граница отрезка с КУ по X,
   // правая граница отрезка с КУ по X,
   // левая граница отрезка с КУ по Y,
   // правая граница отрезка с КУ по Y.
   public static Boundary BoundaryParse(string Str)
   {
      var data = Str.Split();
      Boundary boundary = new((BoundaryType)Enum.Parse(typeof(BoundaryType), data[0]),
      int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));

      return boundary;
   }
}