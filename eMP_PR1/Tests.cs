namespace eMP_PR1;

public interface ITest
{
   public double U(Node2D point);
   public double F(Node2D node);
}

public class FirstTest : ITest
{
   public double U(Node2D node)
       => node.X;

   public double F(Node2D node)
       => 0;
}

public class SecondTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X - node.Y;

   public double F(Node2D node)
       => -1 + node.X * node.X - node.Y;
}

public class ThirdTest : ITest
{
   public double U(Node2D node)
       => node.X + node.Y;

   public double F(Node2D node)
       => 0;
}

public class FourthTest : ITest
{
   public double U(Node2D node)
       => Math.Log(node.X + node.Y);

   public double F(Node2D node)
       => 2 / ((node.X + node.Y) * (node.X + node.Y));
}

public class FifthTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X * node.X * node.X;
   public double F(Node2D node)
       => -12 * node.X * node.X;
}

public class SixthTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X * node.X;

   public double F(Node2D node)
       => -6 * node.X;
}

public class SeventhTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X;

   public double F(Node2D node)
       => -2;
}

public class EigthththTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X + node.Y * node.Y;

   public double F(Node2D node)
       => -4;
}

public class NinethTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X * node.X + node.Y * node.Y * node.Y;

   public double F(Node2D node)
       => -6 * (node.X + node.Y);
}