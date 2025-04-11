using System;
using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Astrolabe.Twinkle;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutScrollList : TwinkleComponent
	{
		private RectTransform _scrollListParentTransform;
		private RectTransform _scrollListTransform;
		private AstrolabeVirtualizedScrollList _scrollList;

		public event CollectionChangeHandler<ILogicalElement> ElementAdding;
		public event CollectionChangeHandler<ILogicalElement> ElementAdded;

		protected override void AwakeOverride()
		{
			_scrollListParentTransform = GetComponent<RectTransform>();
			_scrollList = GetComponentInChildren<AstrolabeVirtualizedScrollList>();
			_scrollListTransform = _scrollList.GetComponent<RectTransform>();

			_scrollList.ElementAdded += OnElementAdded;
			_scrollList.ElementAdding += OnElementAdding;
		}

		private void OnElementAdded(object sender, ILogicalElement element)
		{
			ElementAdded?.Invoke(this, element);
		}
		private void OnElementAdding(object sender, ILogicalElement element)
		{
			ElementAdding?.Invoke(this, element);
		}

		public Color HoverColor; //TODO

		public bool ColorOnHover; //TODO

		public new float Width
		{
			get => _scrollListParentTransform.sizeDelta.x;
			set
			{
				_scrollListParentTransform.sizeDelta = new Vector2(value, Height);
				_scrollListTransform.sizeDelta = new Vector2(value, Height);
			}
		}

		public new float Height
		{
			get => _scrollListParentTransform.sizeDelta.y;
			set
			{
				_scrollListParentTransform.sizeDelta = new Vector2(Width, value);
				_scrollListTransform.sizeDelta = new Vector2(Width, value);
			} 
		}

		/// <summary>
		/// Source type for the elements in the list. Has to be a LogicalElement.
		/// </summary>
		public Type ItemSourceType
		{
			get => _scrollList.ItemSourceType;
			set
			{
				if (_scrollList.ItemSourceType != value && value.IsSubclassOf(typeof(LogicalElement)))
				{
					_scrollList.ItemSourceType = value;
				}
			}
		}

		public void SetMainGrid(LogicalGrid grid)
		{
			_scrollList.MainGrid = grid;
		}

		public bool ShowVerticalScrollBar;

		public bool ShowHorizontalScrollBar;

		public bool ShowNavButtons;

		/// <summary>
		/// If using a Vertical layout, this represents the number of Columns for the layout. When using Horizontal, this represents Rows.
		/// </summary>
		public int RowOrColumnCount;

		/// <summary>
		/// Should the layout cells scroll horizontally on the X axis, or vertically on the Y axis?
		/// </summary>
		public Orientation Orientation;

		public int ItemCount;

		/// <summary>
		/// This is the spacing between the area where the layout cells are, and the boundaries of the ScrollRect's Content area.
		/// </summary>
		public int InternalMargin=4;

		/// <summary>
		/// This is the spacing between each layout cell in local units.
		/// </summary>
		public int ItemMargin=4;

		/// <summary>
		/// When scrolling to the end of the list, it can be nice to show some empty space to indicate the end of the list. This is in local units.
		/// </summary>
		public int TrailingSpace;

		public Vector2 ItemSize
		{
			get => _scrollList.cellSize;
			set => _scrollList.cellSize = value;
		}
	}
}