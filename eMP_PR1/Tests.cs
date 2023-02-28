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
       => 3 * node.X * node.X * node.X + 2 * node.Y * node.Y * node.Y;

   public double F(Node2D node)
       => (node.AreaNumber == 0) ? -9 * node.X - 6 * node.Y + 0.5 *
       (3 * node.X * node.X * node.X + 2 * node.Y * node.Y * node.Y) :
       -36 * node.X - 24 * node.Y + 2 * (3 * node.X * node.X * node.X + 2 * node.Y * node.Y * node.Y);
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
       => 4 * node.X * node.X * node.X * node.X;
   public double F(Node2D node)
       => -48 * node.X * node.X;
}

public class SixthTest : ITest
{
   public double U(Node2D node)
       => 4 * node.X * node.X * node.X * node.X + 2 * node.Y * node.Y * node.Y * node.Y;

   public double F(Node2D node)
       => -48 * node.X * node.X - 24 * node.Y * node.Y;
}

public class SeventhTest : ITest
{
   public double U(Node2D node)
       => Math.Exp(node.X) + node.Y;

   public double F(Node2D node)
       => node.Y;
}

public class EighthTest : ITest
{
   public double U(Node2D node)
       => node.X * node.X * node.X + node.Y;

   public double F(Node2D node)
       => -6 * node.X;
}