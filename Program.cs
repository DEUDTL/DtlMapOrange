// See https://aka.ms/new-console-template for more information

using DtlMapOrange;

const string prefix =
#if DEBUG
	@"G:\WORKDEU\2023-12-01\2023-12-07\";
#else
	"";
#endif

var oa = new OrangeAccess();
if (oa.TestPython() == false)
{
	Console.WriteLine("Python이 없는거 같아요. 설치하고 PyOrange3를 구성해주세요");
	return -1;
}

if (args.Length != 3)
{
	Console.WriteLine("사용법: DtlMapOrange [pkcls] [tab] [항목이름]");
	return -2;
}

var pkclsFile = File.Exists(args[0]) ? args[0] : $@"{prefix}{args[0]}";
var tabFile = File.Exists(args[1]) ? args[1] : $@"{prefix}{args[1]}";
var tabName = args[2];

var pkclsInfo = new FileInfo(pkclsFile);
var pkclsName = pkclsInfo.Name[..pkclsInfo.Name.IndexOf('.')];

var outDtlMapFile = $@"{pkclsInfo.DirectoryName}\{pkclsName}_dtlmap_{tabName}.csv";

var lstTab = OrangeAccess.ReadTab(tabFile, tabName);
if (lstTab == null)
{
	Console.WriteLine("TAB 데이터를 얻을 수 없어요. TAB 이름이 맞나 확인하세요");
	return -3;
}
Console.WriteLine($"TAB 데이터 개수: {lstTab.Count}");

var csv = oa.CreateResult(pkclsFile, tabFile);
if (string.IsNullOrEmpty(csv))
{
	Console.WriteLine("PyOrange3 수행에 실패했어요!");
	return -4;
}

var lstCsv = OrangeAccess.ParseCsv(csv);
if (lstCsv == null)
{
	Console.WriteLine("CSV 데이터를 얻을 수 없어요");
	return -5;
}
Console.WriteLine($"CSV 데이터 개수: {lstCsv.Count}");

var ir = new InterfenceResult(lstTab, lstCsv);

Console.WriteLine();
Console.WriteLine($"Inferences of [{pkclsName}]:");
Console.WriteLine("  Positive:");
Console.WriteLine($"    TP = {ir.Tp}");
Console.WriteLine($"    FN = {ir.Fn}");
Console.WriteLine("  Negative:");
Console.WriteLine($"    FP = {ir.Fp}");
Console.WriteLine($"    TN = {ir.Tn}");
Console.WriteLine();
Console.WriteLine($"Results of [{tabName}]:");
Console.WriteLine($"  Accuracy =  {ir.Accuracy}");
Console.WriteLine($"  Precision = {ir.Precision}");
Console.WriteLine($"  Recall =    {ir.Recall}");

OrangeAccess.WriteCsv(outDtlMapFile, lstTab, lstCsv);

return 0;
