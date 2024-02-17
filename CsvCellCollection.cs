using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtlMapOrange;

internal class CsvCellCollection
{
	public string Name { get; }
	public List<double> Values { get; } = [];
	public bool IsDataCell {get; }

	public CsvCellCollection(string name)
	{
		Name = name;
		try
		{
			IsDataCell = double.TryParse(name, out _);
		}
		catch (Exception)
		{
			IsDataCell = false;
		}
	}

	public void Add(double value)
	{
		Values.Add(value);
	}

	public bool Add(string value)
	{
		if (double.TryParse(value, out var d))
		{
			Values.Add(d);
			return true;
		}
		if (value == "X")
		{
			Values.Add(1);
			return true;
		}
		if (value == "Y")
		{
			Values.Add(2);
			return true;
		}
		if (value == "Z")
		{
			Values.Add(3);
			return true;
		}
		return false;
	}

	override public string ToString()
	{
		if (IsDataCell)
			return $"[{Name}]({Values.Count})";
		return $"{Name}({Values.Count})";
	}
}
