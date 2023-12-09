using System.Diagnostics;

namespace DtlMapOrange;

public class OrangeAccess
{
	private readonly string _python = "python";

	public bool TestPython()
	{
		return RunProcess("--version");
	}

	public string? CreateResult(string pkcls, string tab)
	{
		var csvName = Path.GetTempFileName();
		var scriptName = Path.GetTempFileName();
		var code = $"""
					import Orange;
					import pickle
					import pandas as pd

					model = pickle.load(open('{pkcls.Replace('\\', '/')}', 'rb'))
					test = Orange.data.Table('{tab.Replace('\\', '/')}');
					pred_ind = model(test)
					Pred = pd.DataFrame(model.domain.class_var.str_val(i) for i in pred_ind)
					Pred_list = [model.domain.class_var.str_val(i) for i in pred_ind]
					prob = pd.DataFrame(model(test, model.Probs))
					prob['prediction'] = Pred
					prob.to_csv('{csvName.Replace('\\', '/')}')

					""";
		File.WriteAllText(scriptName, code);

		if (!RunProcess(scriptName))
		{
			File.Delete(csvName);
			File.Delete(scriptName);
			return null;
		}

		var s = File.ReadAllText(csvName);
		File.Delete(csvName);
		File.Delete(scriptName);

		return s;
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

	public static List<double>? ParseCsv(string? csv)
	{
		if (string.IsNullOrEmpty(csv))
			return null;

		var ll = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var cnt = ll.Length;
		if (cnt < 2)
			return null;

		var res = new List<double>();
		for (var i = 1; i < cnt; i++)
		{
			var ss = ll[i].Split(',');
			if (ss.Length != 4)
				continue;
			res.Add(Convert.ToDouble(ss[3].Trim()));
		}

		return res;
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
			if (ss.Length<index)
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
}
