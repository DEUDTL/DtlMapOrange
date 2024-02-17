using System.Diagnostics;
using System.Text;

namespace DtlMapOrange;

internal class OrangeAccess
{
	private readonly string _python = "python";

	public string TabName { get; private set; } = string.Empty;
	public List<CsvCellCollection> TabList { get; } = [];
	public List<double> PredictedList { get; } = [];

	public bool TestPython()
	{
		return RunProcess("--version");
	}

	private bool RunProcess(string? argument = null)
	{
		if (string.IsNullOrEmpty(_python))
			return false;

		try
		{
			var ps = new Process();
			ps.StartInfo.FileName = _python;
			ps.StartInfo.Arguments = argument ?? "";
			ps.StartInfo.UseShellExecute = false;
			ps.StartInfo.CreateNoWindow = true;
			ps.StartInfo.RedirectStandardOutput = true;
			ps.EnableRaisingEvents = true;
			ps.Start();
			ps.WaitForExit();
			return true;
		}
		catch (Exception e)
		{
			Debug.WriteLine($"오우... {e}");
		}

		return false;
	}

	private bool RunOrange(string pickleFile, string tabFile, string csvFile)
	{
		pickleFile = pickleFile.Replace('\\', '/');
		tabFile = tabFile.Replace('\\', '/');
		csvFile = csvFile.Replace('\\', '/');
		var scriptName = Path.GetTempFileName().Replace('\\', '/') + ".py";
		var code = $"""
					import Orange;
					import pickle
					import pandas as pd

					model = pickle.load(open('{pickleFile}', 'rb'))
					test = Orange.data.Table('{tabFile}');
					pred_ind = model(test)
					Pred = pd.DataFrame(model.domain.class_var.str_val(i) for i in pred_ind)
					Pred_list = [model.domain.class_var.str_val(i) for i in pred_ind]
					prob = pd.DataFrame(model(test, model.Probs))
					prob['prediction'] = Pred
					prob.to_csv('{csvFile}')

					""";
		File.WriteAllText(scriptName, code);
		if (!RunProcess(scriptName))
		{
			File.Delete(scriptName);
			return false;
		}
		File.Delete(scriptName);
		return true;
	}

	public string CreateResult(string pickleFile, string tabFile, string outFile)
	{
		if (string.IsNullOrEmpty(TabName))
			return "TAB 이름이 없어요!";
		if (TabList.Count < 1)
			return "TAB 데이터가 없어요!";

		var pickleInfo = new FileInfo(pickleFile);
		var tabInfo = new FileInfo(tabFile);
		if (!RunOrange(pickleInfo.FullName, tabInfo.FullName, outFile))
			return "Python 또는 Orange3를 실행할 수 없어요!";

		var csv = File.ReadAllText(outFile);
		if (string.IsNullOrEmpty(csv))
			return "CSV 결과가 만들어지지 않았어요!";
		if (!ParseResultCsv(csv))
			return "CSV 결과를 분석할 수 없어요!";

		return string.Empty;
	}

	private bool ParseResultCsv(string csv)
	{
		PredictedList.Clear();

		if (string.IsNullOrEmpty(csv))
			return false;

		var ll = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var cnt = ll.Length;
		if (cnt < 2)
			return false;

		for (var i = 1; i < cnt; i++)
		{
			var ss = ll[i].Split(',');
			if (ss.Length != 4)
				continue;
			PredictedList.Add(Convert.ToDouble(ss[3].Trim()));
		}

		return true;
	}

	public bool ParseTabData(string filename)
	{
		TabList.Clear();

		var tab = File.ReadAllLines(filename);
		if (tab.Length < 2)
			return false;

		var ss = tab[0].Split('\t');
		foreach (var s in ss)
			TabList.Add(new CsvCellCollection(s));

		var count = ss.Length;
		for (var i = 1; i < tab.Length; i++)
		{
			ss = tab[i].Split('\t');
			if (ss.Length != count)
				continue;
			for (var u = 0; u < count; u++)
			{
				if (!TabList[u].Add(ss[u]))
					return false;
			}
		}

		return true;
	}

	public bool SetTabName(string name)
	{
		var found = TabList.SingleOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		if (found == null)
			return false;
		TabName = name;
		return true;
	}

	public InterfenceResult? GetInterfenceResult()
	{
		var collection = TabList.SingleOrDefault(c => c.Name.Equals(TabName, StringComparison.InvariantCultureIgnoreCase));
		if (collection == null)
			return null;
		return new InterfenceResult(collection.Values, PredictedList);
	}

	public static List<double>? ReadTab(string filename, string name)
	{
		var tab = File.ReadAllLines(filename);
		var index = IndexOfTabName(tab[0], name);

		if (index < 0)
			return null;

		var res = new List<double>();
		for (var i = 1; i < tab.Length; i++)
		{
			var ss = tab[i].Split('\t');
			if (ss.Length < index)
				continue;
			res.Add(Convert.ToDouble(ss[index].Trim()));
		}

		return res;
	}

	private static int IndexOfTabName(string line, string name)
	{
		var ss = line.Split('\t');
		for (var i = 0; i < ss.Length; i++)
		{
			if (name.Equals(ss[i], StringComparison.InvariantCultureIgnoreCase))
				return i;
		}

		return -1;
	}

	public static bool WriteCsv(string filename, List<double> lstTab, List<double> lstPrd)
	{
		try
		{
			StreamWriter sw = new StreamWriter(filename);
			sw.WriteLine("Tab,Predicted");
			for (var i = 0; i < lstTab.Count; i++)
			{
				sw.WriteLine($"{lstTab[i]},{lstPrd[i]}");
			}
			sw.Close();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public bool WriteResult(string filename, string noName)
	{
		List<CsvCellCollection> collections = [];

		var dest = TabList.SingleOrDefault(c => c.Name.Equals(TabName, StringComparison.InvariantCultureIgnoreCase));
		if (dest == null)
			return false;
		var nocell = TabList.SingleOrDefault(c => c.Name.Equals(noName, StringComparison.InvariantCultureIgnoreCase));
		if (nocell != null)
			collections.Add(nocell);
		foreach (var c in TabList)
		{
			if (c.IsDataCell)
				collections.Add(c);
		}

		try
		{
			if (File.Exists(filename))
				File.Delete(filename);
			using var sw = new StreamWriter(filename);
			var sb = new StringBuilder($"{dest.Name},Predicted");
			foreach (var c in collections)
				sb.Append($",{c.Name}");
			sw.WriteLine(sb.ToString());
			for (var i = 0; i < PredictedList.Count; i++)
			{
				sb = new StringBuilder($"{dest.Values[i]},{PredictedList[i]}");
				foreach (var c in collections)
					sb.Append($",{c.Values[i]}");
				sw.WriteLine(sb.ToString());
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
