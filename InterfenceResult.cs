//https://a292run.tistory.com/entry/mean-Average-PrecisionmAP-%EA%B3%84%EC%82%B0%ED%95%98%EA%B8%B0-1
namespace DtlMapOrange;

internal class InterfenceResult
{
	// true positive
	public int Tp { get; }
	// False negative
	public int Fn { get; }
	// False positive
	public int Fp { get; }
	// True negative
	public int Tn { get; }

	public InterfenceResult(IReadOnlyList<double> real, IReadOnlyList<double> det)
	{
		var max = Math.Max(real.Count, det.Count);

		for (var i = 0; i < max; i++)
		{
			var r = real[i];
			var d = det[i];

			if (r != 0.0)
			{
				if (d != 0.0)
					Tp++;
				else
					Fn++;
			}
			else
			{
				if (d != 0.0)
					Fp++;
				else
					Tn++;
			}
		}
	}

	// accuracy
	public double Accuracy
	{
		get
		{
			var top = Tp + Tn;
			var bottom = Tp + Fn + Fp + Tn;
			return (double)top / bottom;
		}
	}

	// Precision
	public double Precision
	{
		get
		{
			var top = Tp;
			var bottom = Tp + Fp;
			return (double)top / bottom;
		}
	}

	// Recall
	public double Recall
	{
		get
		{
			var top = Tp;
			var bottom = Tp + Fn;
			return (double)top / bottom;
		}
	}
}
