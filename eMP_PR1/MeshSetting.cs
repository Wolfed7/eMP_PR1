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
   public Mesh SetMesh(MeshType MT, string filePath)
   {

   }
}