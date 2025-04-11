using System;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public abstract class VisualFluentElement : VisualElement, IVisualFluentElement
	{
		public VisualFluentElement(ILogicalElement logicalElement) : base(logicalElement)
		{
		}

		public FluentShader FluentShader { get; set; }

		public override void SetHitTestVisible(bool value)
		{
			FluentShader.IsHitTestVisible = value;
		}

		/// <summary>
		/// Brush
		/// </summary>
		public void SetBackground(Brush brush, Action<Brush> sizeChanged)
		{
			FluentShader.SetBackground(brush, sizeChanged);
		}

		public Vector2 TileScale
		{
			get => FluentShader.TileScale;
			set => FluentShader.TileScale = value;
		}

		private Vector2 _tileScale = Vector2.One;

		public Vector2 TileOffset
		{
			get => FluentShader.TileOffset;
			set => FluentShader.TileOffset = value;
		}

		public TileMode TileMode
		{
			get => FluentShader.TileMode;

			set => FluentShader.TileMode = value;
		}

		public AlignmentX AlignmentX
		{
			get => FluentShader.AlignmentX;

			set => FluentShader.AlignmentX = value;
		}

		public AlignmentY AlignmentY
		{
			get => FluentShader.AlignmentY;

			set => FluentShader.AlignmentY = value;
		}

		public float AlignmentOffsetX => FluentShader.AlignmentOffsetX;

		public float AlignmentOffsetY => FluentShader.AlignmentOffsetY;

		public virtual float Opacity
		{
			get => FluentShader.Opacity;

			set => FluentShader.Opacity = value;
		}

		public virtual ShaderMode ShaderMode
		{
			get => FluentShader.ShaderMode;

			set => FluentShader.ShaderMode = value;
		}

		public void SetLightColor(SolidColorBrush color)
		{
			FluentShader.SetLightColor(color, true);
		}

		public void SetLightEnabled(bool isLightEnabled)
		{
			FluentShader.SetLightEnabled(isLightEnabled, true);
		}

		public void SetLightIntensity(float lightIntensity)
		{
			FluentShader.SetLightIntensity(lightIntensity, true);
		}

		public void SetCullMode(CullMode cullMode)
		{
			FluentShader.SetCullMode(cullMode, true);
		}
	}
}