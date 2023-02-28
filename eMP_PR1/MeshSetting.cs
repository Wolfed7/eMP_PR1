namespace eMP_PR1;
    
public enum MeshType
{
   Regular,
   Irregular
}

public interface IMeshSetting
{
   public Mesh SetMesh(MeshType MT, string filePath);
}

public class MeshSetting : IMeshSetting
{
   // Создаёт сетку из файла, входные данные:
   // Тип сетки, путь к файлу
   public Mesh SetMesh(MeshType MT, string filePath)
   {
      return MT switch
      {
         MeshType.Regular => new RegularMesh(filePath),

         //MeshType.Irregular => new IrregularMesh(filePath),

         _ => throw new ArgumentOutOfRangeException(nameof(MT),
         $"Неизвестный тип сетки: {MT}")
      };
   }
}