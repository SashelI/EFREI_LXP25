using System;

namespace Assets.Scripts.Tools
{
	public class TaskProgression
	{
		public int Total { get; set; }

		public int Value { get; set; } = 0;

		public double GetPercent(int decimals = 2)
		{
			if (Total == 0)
			{
				return 0;
			}

			float percent = (float)Value / Total * 100;
			double rounded = Math.Round(percent, decimals);
			return rounded;
		}
	}
}