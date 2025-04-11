using UnityEngine;

namespace MixedReality.Toolkit.Input.Astrolabe.Scripts.MRTK_Enhanced
{
	/// <summary>
	/// Overriding MRTK line visual to access line data provider (internal class and private property)
	/// </summary>
	public class AstrolabeLineVisual : MRTKLineVisual
	{
		private BaseMixedRealityLineDataProvider _lineDataProvider = null;

		// Start is called before the first frame update
		void Start()
		{
			_lineDataProvider = this.GetComponentInChildren<BezierDataProvider>();
		}

		public void SetLineStartPoint(Vector3 startPoint)
		{
			if (_lineDataProvider == null)
			{
				_lineDataProvider = this.GetComponentInChildren<BezierDataProvider>();
			}

			if (_lineDataProvider != null)
			{
				_lineDataProvider.SetPoint(0, startPoint);
			}
		}

		public void SetLineEndPoint(Vector3 endPoint)
		{
			if (_lineDataProvider == null)
			{
				_lineDataProvider = this.GetComponentInChildren<BezierDataProvider>();
			}

			if (_lineDataProvider != null)
			{
				_lineDataProvider.SetPoint(_lineDataProvider.PointCount - 1, endPoint);
			}
		}
	}
}
