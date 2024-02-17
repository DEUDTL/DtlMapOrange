// See https://aka.ms/new-console-template for more information

using DtlMapOrange;

var orange = new OrangeAccess();
if (orange.TestPython() == false)
{
	Console.WriteLine("Python이 없는거 같아요. 설치하고 PyOrange3를 구성해주세요");
	return -1;
}

if (args.Length != 3)
{
	Console.WriteLine("사용법: DtlMapOrange [pkcls] [tab] [항목 이름] [번호셀 이름(기본값: No.)]");
	return -2;
}
if (File.Exists(args[0]) == false)
{
	Console.WriteLine($"PKCLS 파일({args[0]})을 찾을 수 없어요");
	return -3;
}
if (File.Exists(args[1]) == false)
{
	Console.WriteLine($"TAB 파일({args[1]})을 찾을 수 없어요");
	return -4;
}

var pkclsFile = args[0];
var tabFile = args[1];
var tabName = args[2];
var noCellName = args.Length == 4 ? args[3] : "No.";

var pkclsInfo = new FileInfo(pkclsFile);
var pkclsName = pkclsInfo.Name[..pkclsInfo.Name.IndexOf('.')];
var outFile = $@"{pkclsInfo.DirectoryName}\{pkclsName}_dtl_{tabName}_out.csv";
var mapFile = $@"{pkclsInfo.DirectoryName}\{pkclsName}_dtl_{tabName}_map.csv";

if (orange.ParseTabData(tabFile) == false)
{
	Console.WriteLine("TAB 데이터를 분석할 수 없어요");
	return -3;
}
Console.WriteLine($"TAB 갯수: {orange.TabList.Count}");
Console.WriteLine($"TAB 데이터 갯수: {orange.TabList.First().Values.Count}");

if (orange.SetTabName(tabName) == false)
{
	Console.WriteLine($"TAB 이름({tabName})을 찾을 수 없어요");
	return -4;
}

var csvmsg = orange.CreateResult(pkclsFile, tabFile, outFile);
if (!string.IsNullOrEmpty(csvmsg))
{
	Console.WriteLine(csvmsg);
	return -5;
}

var result = orange.GetInterfenceResult();
if (result == null)
{
	Console.WriteLine("최종 결과를 얻을 수 없어요!");
	return -6;
}

Console.WriteLine();
Console.WriteLine($"Inferences of [{pkclsName}]:");
Console.WriteLine("  Positive:");
Console.WriteLine($"    TP = {result.Tp}");
Console.WriteLine($"    FN = {result.Fn}");
Console.WriteLine("  Negative:");
Console.WriteLine($"    FP = {result.Fp}");
Console.WriteLine($"    TN = {result.Tn}");
Console.WriteLine();
Console.WriteLine($"Results of [{tabName}]:");
Console.WriteLine($"  Accuracy =  {result.Accuracy}");
Console.WriteLine($"  Precision = {result.Precision}");
Console.WriteLine($"  Recall =    {result.Recall}");

orange.WriteResult(mapFile, noCellName);

return 0;
