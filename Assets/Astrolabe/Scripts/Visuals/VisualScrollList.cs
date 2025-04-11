using System;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Object = UnityEngine.Object;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualScrollList : VisualElement, IVisualScrollList
	{
		private LayoutScrollList _scrollLayout { get; set; }

		public event CollectionChangeHandler<ILogicalElement> ElementAdding;
		public event CollectionChangeHandler<ILogicalElement> ElementAdded;

		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalScrollListComponent), typeof(VisualScrollList));
		}

		public VisualScrollList(ILogicalElement logicalElement) : base(logicalElement)
		{
			var scrollGo = Object.Instantiate(TwinklePrefabFactory.Instance.prefabScrollList);
			TwinkleComponent = scrollGo.GetComponent<LayoutScrollList>();
			_scrollLayout = scrollGo.GetComponent<LayoutScrollList>();

			_scrollLayout.ElementAdding += OnChildrenElementAdding;
			_scrollLayout.ElementAdded += OnChildrenElementAdded;
		}

		public Color HoverColor
		{
			get
			{
				var color = _scrollLayout.HoverColor;
				return new Color((byte)(color.a * 255), (byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255));
			}
			set => _scrollLayout.HoverColor = value.ToUnityColor();
		}

		public bool ColorOnHover
		{
			get => _scrollLayout.ColorOnHover;
			set => _scrollLayout.ColorOnHover=value;
		}

		public float Width
		{
			get => _scrollLayout.Width;
			set => _scrollLayout.Width = value;
		}

		public float Height
		{
			get => _scrollLayout.Height;
			set => _scrollLayout.Height = value;
		}

		public Type ItemSourceType
		{
			get => _scrollLayout.ItemSourceType;
			set => _scrollLayout.ItemSourceType = value;
		}

		public void SendMainGridToScroll(LogicalGrid grid)
		{
			_scrollLayout.SetMainGrid(grid);
		}

		public bool ShowVerticalScrollBar
		{
			get => _scrollLayout.ShowVerticalScrollBar; 
			set => _scrollLayout.ShowVerticalScrollBar = value;
		}

		public bool ShowHorizontalScrollBar
		{
			get => _scrollLayout.ShowHorizontalScrollBar; 
			set => _scrollLayout.ShowHorizontalScrollBar = value;
		}

		public bool ShowNavButtons
		{
			get => _scrollLayout.ShowNavButtons; 
			set => _scrollLayout.ShowNavButtons = value;
		}

		public int RowOrColumnCount
		{
			get => _scrollLayout.RowOrColumnCount;
			set => _scrollLayout.RowOrColumnCount = value;
		}

		public Orientation Orientation
		{
			get => _scrollLayout.Orientation;
			set => _scrollLayout.Orientation = value;
		}

		public int ItemCount
		{
			get => _scrollLayout.ItemCount;
			set => _scrollLayout.ItemCount = value;
		}

		public int InternalMargin 
		{ 
			get => _scrollLayout.ItemCount; 
			set => _scrollLayout.ItemCount = value;
		}

		public int ItemMargin
		{
			get => _scrollLayout.ItemCount; 
			set => _scrollLayout.ItemCount = value;
		}

		public int TrailingSpace
		{
			get => _scrollLayout.ItemCount; 
			set => _scrollLayout.ItemCount = value;
		}

		public Vector2 ItemSize
		{
			get => _scrollLayout.ItemSize.ToVector2();
			set => _scrollLayout.ItemSize = value.ToVector2();
		}

		private void OnChildrenElementAdding(object sender, ILogicalElement logicalElement)
		{
			ElementAdding?.Invoke(sender, logicalElement);
		}

		private void OnChildrenElementAdded(object sender, ILogicalElement logicalElement)
		{
			ElementAdded?.Invoke(sender, logicalElement);
		}
	}
}