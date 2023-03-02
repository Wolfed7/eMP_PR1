using eMP_PR1;

const string BoundariesPath = "Input/Boundaries.txt";
const string RegularMeshPath = "Input/RegularMesh.txt";
const string IrregularMeshPath = "Input/IrregularMesh.txt";

const int iterations = 1000000;
const double epsilon = 1e-14;
const double w = 1.23;

MeshSetting meshSetting = new();


MFD mfd = new(meshSetting.SetMesh(MeshType.Regular, RegularMeshPath), BoundariesPath);
//MFD mfd = new(meshSetting.SetMesh(MeshType.Irregular, IrregularMeshPath), BoundariesPath);


mfd.SetTest(new FirstTest());
//mfd.SetTest(new SecondTest());
//mfd.SetTest(new ThirdTest());
//mfd.SetTest(new FourthTest());
//mfd.SetTest(new FifthTest());
//mfd.SetTest(new SixthTest()); 
//mfd.SetTest(new SeventhTest());
//mfd.SetTest(new EighthTest());


mfd.SetMethodSolvingSLAE(new GaussSeidel(iterations, epsilon, w));
mfd.Compute();