// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using MixedReality.Toolkit.UX.Experimental;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	/// <summary>
	/// MODIFIED FOR ASTROLABE ©SYNERGIZ 2024
	/// A helper for rendering large amounts of data within a Unity <see cref="ScrollRect"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="VirtualizedScrollRectList"/> is a helper component that 
	/// can represent a very large lists in a Unity <see cref="ScrollRect"/>,
	/// without paying the cost for a very large number of <see cref="GameObject"/>
	/// instances. This works by maintaining a number of <see cref="GameObject"/>
	/// instances that completely cover the visible area of the <see cref="ScrollRect"/>,
	/// and reusing them as the list scrolls up and down.
	/// </para>
	/// <para>
	/// Using this is not quite as simple as adding it as a component to your
	/// <see cref="ScrollRect"/>, it also requires a bit of code to get going.
	/// The process for using <see cref="VirtualizedScrollRectList"/> is as
	/// follows:
	/// </para>
	/// <list type="number">
	///   <item>
	///     <description>
	///         Add the <see cref="VirtualizedScrollRectList"/> component to a
	///         same <see cref="GameObject"/> containing a <see cref="ScrollRect"/>
	///         component.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///         Add callbacks to <see cref="VirtualizedScrollRectList.OnVisible"/>
	///         and <see cref="VirtualizedScrollRectList.OnInvisible"/>.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///         Call <see cref="VirtualizedScrollRectList.SetItemCount"/>
	///         to let the component know how many items are in the virtualized list.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///         As the list loads items into view, the
	///         <see cref="VirtualizedScrollRectList.OnVisible"/> callback wll be
	///         called. This callback should populate a Unity prefab with the data
	///         associated with the provided index.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///         As the list unloads items from view, the
	///         <see cref="VirtualizedScrollRectList.OnInvisible"/> callback wll be
	///         called. This callback should perform any cleanup for the data
	///         associated with the provided index.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///         Call <see cref="VirtualizedScrollRectList.SetItemCount"/>
	///         as the size of the data changes.
	///     </description>
	///   </item>
	/// </list>
	/// <para>
	/// This is an experimental feature. This class is early in the cycle, it has 
	/// been labeled as experimental to indicate that it is still evolving, and 
	/// subject to change over time. Parts of the MRTK, such as this class, appear 
	/// to have a lot of value even if the details haven't fully been fleshed out. 
	/// For these types of features, we want the community to see them and get 
	/// value out of them early enough so to provide feedback.
	/// </para>
	/// </remarks>
	[AddComponentMenu("MRTK/UX/Virtualized Scroll Rect List")]
	public class AstrolabeVirtualizedScrollList : MonoBehaviour
	{
		#region Inspector and Private Fields

		/// <summary>
		/// The direction the cell layout should flow in, top to bottom, or
		/// left to right.
		/// </summary>
		public enum Layout
		{
			/// <summary>
			/// This layout flows from top to bottom along the Y axis.
			/// </summary>
			Vertical,

			/// <summary>
			/// This layout flows from left to right along the X axis.
			/// </summary>
			Horizontal,
		}

		// Header Objects

		[Tooltip("The ScrollRect used to display this list. If unspecified, VirtualizedScrollRectList will look for a ScrollRect on the current GameObject.")]
		[SerializeField]
		private ScrollRect scrollRect;

		// Header Layout

		[Header("Layout")]
		[Tooltip("Should the layout cells scroll horizontally on the X axis, or vertically on the Y axis?")]
		[SerializeField]
		private Layout layoutDirection = Layout.Vertical;

		[Tooltip("If using a Vertical layout, this represents the number of Columns for the layout. When using Horizontal, this represents Rows.")]
		[SerializeField]
		private int layoutRowsOrColumns = 1;

		[Tooltip("(Optional) The size of each layout cell. If an axis is 0, VirtualizedList will pull this dimension directly from the prefab's RectTransform.")]
		[SerializeField]
		public Vector2 cellSize;

		[Tooltip("This is the spacing between each layout cell in local units.")]
		[SerializeField]
		private float gutter;

		[Tooltip("This is the spacing between the area where the layout cells are, and the boundaries of the ScrollRect's Content area.")]
		[SerializeField]
		private float margin;

		[Tooltip("When scrolling to the end of the list, it can be nice to show some empty space to indicate the end of the list. This is in local units.")]
		[SerializeField]
		private float trailingSpace;

		// Header State

		[Header("State")]
		[Tooltip("This is mostly for debug and inspection. Item Count should be driven by using SetItemCount in the API.")]
		[SerializeField]
		private int itemCount = 10;

		private float scroll = 0;
		private float requestScroll;

		private int screenCount;
		private float viewSize;
		private float contentStart;
		private float layoutPrefabSize;
		private bool initialized = false;
		private bool resetCalled = false;

		private int visibleStart;
		private int visibleEnd;
		private bool visibleValid;

		private const float DEFAULT_CELL_WIDTH = 10;
		private const float DEFAULT_CELL_HEIGHT = 10;

		private Queue<LogicalElement> pool = new Queue<LogicalElement>();
		private Dictionary<int, LogicalElement> poolDict = new Dictionary<int, LogicalElement>();

		private Action<LogicalElement, int> onVisible;
		private Action<LogicalElement, int> onInvisible;
		#endregion

		#region Public properties and fields

		/// <summary>
		/// This is the index based scroll value of the list. You can set this,
		/// and it will snap the ScrollRect content to the correct location. If
		/// you want to scroll to halfway through item #3, you can just set the
		/// Scroll to 3.5.
		///
		/// Things to note here, if you have multiple RowsOrColumns, like 2,
		/// then there are 2 indices per Row, and 3 would be halfway through
		/// the second row. Due to the scroll viewport size, also may not be
		/// able scroll directly to the last element. Scroll will be clamped
		/// between 0 and MaxScroll.
		/// </summary>
		public float Scroll
		{
			get => scroll;
			set
			{
				requestScroll = value;
				UpdateScrollView(requestScroll);
			}
		}

		/// <summary>
		/// The pool of list item objects will be composed of this Control. When
		/// the value changes the items are all repopulated.
		/// <see cref="ResetLayout"/>
		/// </summary>
		public Type ItemSourceType
		{
			get => _itemSourceType;
			set
			{
				if (_itemSourceType != value && value.IsSubclassOf(typeof(LogicalElement)))
				{
					_itemSourceType = value;
					ResetLayout();
				}
			}
		}
		private Type _itemSourceType;

		/// <summary>
		/// Main grid used for logicalElement instantiation
		/// </summary>
		public LogicalGrid MainGrid
		{
			get => _mainGrid;
			set
			{
				if (value != null && _mainGrid != value)
				{
					_mainGrid = value;
				}
			}
		}
		private LogicalGrid _mainGrid;

		/// <summary>
		/// This is the current number of items that this list is handling for
		/// display.
		/// </summary>
		public int ItemCount => itemCount;

		/// <summary>
		/// If using a Vertical layout, this represents the number of Columns
		/// for the layout. When using Horizontal, this represents Rows.
		/// </summary>
		public int RowsOrColumns => layoutRowsOrColumns;

		/// <summary>
		/// This is the maximum number of potentially visible layout cells that
		/// may appear on the ScrollRect's viewport. This includes partially
		/// obscured cells.
		/// </summary>
		public int PartiallyVisibleCount => Mathf.CeilToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;

		/// <summary>
		/// This is the number of completely unobscured layout cells the
		/// ScrollRect's viewport can display. This is useful for things like
		/// paginated scrolling, where you want to advance to the next page of
		/// items, and want to start with whatever might be obscured right now.
		/// </summary>
		public int TotallyVisibleCount => Mathf.FloorToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;

		/// <summary>
		/// This is the number of layout cells that fit on the viewport in
		/// total, as a floating point value.
		/// </summary>
		public float ScreenItemCountF => (viewSize / layoutPrefabSize) * layoutRowsOrColumns;

		/// <summary>
		/// This is the maximum value for the Scroll. The Scroll value will be
		/// clamped to this value.
		/// </summary>
		public float MaxScroll => layoutDirection == Layout.Vertical
			? ((scrollRect.content.rect.height - viewSize) / layoutPrefabSize) * layoutRowsOrColumns
			: ((scrollRect.content.rect.width - viewSize) / layoutPrefabSize) * layoutRowsOrColumns;

		/// <summary>
		/// When a pooled prefab instance is just made visible on the scroll
		/// content area, this callback is called, with an integer representing
		/// the index of the item in the list. This callback is invoked after
		/// the GameObject has been positioned, but just before it is made
		/// visible. When the value is set the list of items get reset.
		/// <see cref="ResetLayout"/>.
		/// </summary>
		public Action<LogicalElement, int> OnVisible
		{
			get => onVisible;
			set
			{
				if (onVisible != value)
				{
					onVisible = value;
					// If this is changing, it means how the items are populated is changing, hence reset.
					ResetLayout();
				}
			}
		}

		/// <summary>
		/// When a pooled prefab instance is just removed from visibility on
		/// the scroll content area, this callback is called, with an integer
		/// representing the index of the item in the list. VirtualizedScrollRectList
		/// will make this GameObject invisible for you after this callback is
		/// invoked. When the value is set the list of items get reset
		/// <see cref="ResetLayout"/>.
		/// </summary>
		public Action<LogicalElement, int> OnInvisible
		{
			get => onInvisible;
			set
			{
				if (onInvisible != value)
				{
					onInvisible = value;
					// If this is changing, it means how the items are populated is changing, hence reset.
					ResetLayout();
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Given an index, this calculates the position of the associated
		/// layout cell based on the current layout direction and padding /
		/// gutter sizes.
		/// </summary>
		/// <param name="index">
		/// Index of the layout cell, this will still produce valid results
		/// outside of the 0-count range.
		/// </param>
		/// <returns>
		/// A ScrollRect relative location for the layout cell.
		/// </returns>
		private Vector3 ItemLocation(int index)
		{
			return layoutDirection == Layout.Vertical
				? new Vector3(
					scrollRect.content.rect.xMin + (margin + (cellSize.x + gutter) * (index % layoutRowsOrColumns)),
					-(contentStart - (margin + (index / layoutRowsOrColumns) * layoutPrefabSize)),
					0)
				: new Vector3(
					contentStart + (margin + (index / layoutRowsOrColumns) * layoutPrefabSize),
					-(scrollRect.content.rect.yMax - (margin + (cellSize.y + gutter) * (index % layoutRowsOrColumns))),
					0);

		}

		/// <summary>
		/// This converts a ScrollRect scroll position to an index based scroll
		/// value that we can use for managing our scroll in a more intelligent
		/// manner.
		/// </summary>
		/// <param name="pos">
		/// A position on the ScrollRect viewport, this value should match the
		/// axis indicated by layoutDirection.
		/// </param>
		/// <returns>
		/// A scroll index value representing the itemCount index at the
		/// position. Not bounded to the 0-itemCount range at all.
		/// </returns>
		private float PosToScroll(float pos) => layoutDirection == Layout.Vertical
			? ((pos - (margin - gutter)) / layoutPrefabSize) * layoutRowsOrColumns
			: ((-pos - (margin - gutter)) / layoutPrefabSize) * layoutRowsOrColumns;

		/// <summary>
		/// This converts an index based scroll value into a ScrollRect scroll
		/// position that we can use for positioning the ScrollRect content.
		/// </summary>
		/// <param name="scroll">
		/// A scroll index value representing the itemCount index at the
		/// position. Not bounded to the 0-itemCount range at all.
		/// </param>
		/// <returns>
		/// A position on the ScrollRect viewport, this value matches the axis
		/// indicated by layoutDirection.
		/// </returns>
		private float ScrollToPos(float scroll) => layoutDirection == Layout.Vertical
			? (scroll * (layoutPrefabSize / layoutRowsOrColumns)) + (margin - gutter)
			: (-scroll * (layoutPrefabSize / layoutRowsOrColumns)) + (margin - gutter);

		/// <summary>
		/// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
		/// </summary>
		private void OnValidate()
		{
			ResetLayout();
		}

		/// <summary>
		/// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
		/// </summary>
		private void Start()
		{
			visibleValid = false;
			scrollRect = scrollRect == null ? GetComponent<ScrollRect>() : scrollRect;
			BakeCachedValues();

			// Unity RectTransforms don't know anything about sizes until after
			// a frame has passed after Start.
			StartCoroutine(InitializeInNextFrame());
		}

		private IEnumerator InitializeInNextFrame()
		{
			yield return null;

			Initialize();
		}

		private void BakeCachedValues()
		{
			if (cellSize.x == 0) cellSize.x = DEFAULT_CELL_WIDTH;
			if (cellSize.y == 0) cellSize.y = DEFAULT_CELL_HEIGHT;
			layoutPrefabSize = layoutDirection == Layout.Vertical ? cellSize.y + gutter : cellSize.x + gutter;
		}

		private void Initialize()
		{
			BakeCachedValues();

			if (layoutDirection == Layout.Vertical)
			{
				Vector2 topCenter = new Vector2(0.5f, 1);
				scrollRect.content.anchorMin = topCenter;
				scrollRect.content.anchorMax = topCenter;
				scrollRect.content.pivot = topCenter;
				scrollRect.content.sizeDelta = new Vector2(
					2 * margin + layoutRowsOrColumns * (cellSize.x/2) + (layoutRowsOrColumns - 1) * gutter,
					2 * margin + (itemCount / layoutRowsOrColumns) * layoutPrefabSize + trailingSpace);
				viewSize = GetComponent<RectTransform>().sizeDelta.y / 2;
				contentStart = scrollRect.content.rect.yMax - cellSize.y/2;
			}
			else
			{
				Vector2 midLeft = new Vector2(0, 0.5f);
				scrollRect.content.anchorMin = midLeft;
				scrollRect.content.anchorMax = midLeft;
				scrollRect.content.pivot = midLeft;
				scrollRect.content.sizeDelta = new Vector2(
					2 * margin + (itemCount / layoutRowsOrColumns) * layoutPrefabSize + trailingSpace,
					2 * margin + layoutRowsOrColumns * (cellSize.y/2) + (layoutRowsOrColumns - 1) * gutter);
				viewSize = GetComponent<RectTransform>().sizeDelta.x / 2;
				contentStart = Math.Max(0, scrollRect.content.rect.xMin - cellSize.x / 2);
			}
			screenCount = Mathf.CeilToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;

			InitializePool();

			initialized = true;
			scrollRect.onValueChanged.AddListener(v => UpdateScroll(PosToScroll(layoutDirection == Layout.Vertical
				? scrollRect.content.localPosition.y
				: scrollRect.content.localPosition.x)));

			UpdateScrollView(requestScroll);
		}

		private void InitializePool()
		{
			// Support resetting everything from ResetLayout
			foreach (int i in poolDict.Keys.ToArray())
			{
				MakeInvisible(i);
			}
			poolDict.Clear();
			while (pool.Count > 0)
			{
				var logical = pool.Dequeue();
				MainGrid.Children.Remove(logical);
				logical.Dispose();
			}
			visibleStart = -1;
			visibleEnd = -1;

			// Create the pool of prefabs
			int poolSize = itemCount>screenCount ? screenCount + layoutRowsOrColumns : itemCount;

			//Instanciate the elements from Logical mainGrid to here (otherwise Astrolabe instanciation is incomplete)
			for (int i = 0; i < poolSize; i++)
			{
				if(MainGrid != null)
				{
					var instance = (LogicalElement)Activator.CreateInstance(ItemSourceType);
					MainGrid.Children.Add(instance);

					var instanceTransform = instance.GetGameObject().transform;
					instanceTransform.SetParent(scrollRect.content);
					instance.Translation = ItemLocation(-(i + 1)).ToVector3();
					//instanceTransform.SetPositionAndRotation(ItemLocation(-(i + 1)), scrollRect.content.rotation);

					pool.Enqueue(instance);
				}
			}
		}

		public DataSource Test;

		private void UpdateScrollView(float newScroll)
		{
			newScroll = Mathf.Clamp(newScroll, 0, MaxScroll);

			Vector3 pos = scrollRect.content.localPosition;
			scrollRect.content.localPosition = layoutDirection == Layout.Vertical
				? new Vector3(pos.x, ScrollToPos(newScroll), pos.z)
				: new Vector3(ScrollToPos(newScroll), pos.y, pos.z);

			UpdateScroll(newScroll);
		}

		private void UpdateScroll(float newScroll)
		{
			if ((scroll == newScroll && visibleValid == true) || initialized == false) { return; }
			scroll = newScroll;
			visibleValid = true;

			// Based on this scroll, calculate the new relevant ranges of
			// indices
			float paddedScroll = (newScroll-1) - (margin / layoutPrefabSize);
			int newVisibleStart = Math.Max(0, Mathf.RoundToInt(paddedScroll) / layoutRowsOrColumns/2 * layoutRowsOrColumns);
			int newVisibleEnd = Math.Min(itemCount, Mathf.CeilToInt(paddedScroll / layoutRowsOrColumns + (viewSize / layoutPrefabSize)) * layoutRowsOrColumns);

			// If it's the same as we already have, then we can just stop here!
			if (newVisibleStart == visibleStart &&
				newVisibleEnd == visibleEnd) return;

			// Demote all items that are no longer relevant
			for (int i = visibleStart; i < visibleEnd; i++)
			{
				bool wasVisible = i >= visibleStart && i < visibleEnd;
				bool remainsVisible = i >= newVisibleStart && i < newVisibleEnd;
				if (wasVisible == true && remainsVisible == false) { MakeInvisible(i); }
			}

			// Promote all items that are now relevant
			for (int i = newVisibleStart; i < newVisibleEnd; i++)
			{
				bool wasVisible = i >= visibleStart && i < visibleEnd;
				bool nowVisible = i >= newVisibleStart && i < newVisibleEnd;
				if (wasVisible == false && nowVisible == true) { MakeVisible(i); }
			}

			// These are now the current index ranges!
			visibleStart = newVisibleStart;
			visibleEnd = newVisibleEnd;
		}

		private void MakeInvisible(int i)
		{
			if (TryGetVisible(i, out LogicalElement logicalElement) == false) { return; }

			OnInvisible?.Invoke(logicalElement, i);
			poolDict.Remove(i);
			logicalElement.GetGameObject().SetActive(false);
			pool.Enqueue(logicalElement);
		}

		private void MakeVisible(int i)
		{
			if (pool.Count > 0)
			{
				var logicalElement = pool.Dequeue();
				var go = logicalElement.GetGameObject();
				logicalElement.Translation = ItemLocation(i).ToVector3();
				go.SetActive(true);
				poolDict.Add(i, logicalElement);
				OnVisible?.Invoke(logicalElement, i);
			}
		}

		private IEnumerator ResetLayoutNextFrame()
		{
			yield return null;
			if (margin < gutter) { margin = gutter; }

			resetCalled = false;
			visibleValid = false;
			Initialize();
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// This will get the prefab instance representing the given index, if
		/// it's visible. You should NOT store this instance, as it will be
		/// re-used as the content scrolls.
		/// </summary>
		/// <param name="i">
		/// List index to retrieve.
		/// </param>
		/// <param name="visibleObject">
		/// The prefab instance representing the index, or null, if not visible.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the item was visible, <see langword="false"/> if not.
		/// </returns>
		public bool TryGetVisible(int i, out LogicalElement visibleObject)
		{
			if (i >= visibleStart && i < visibleEnd && poolDict.TryGetValue(i, out var value))
			{
				visibleObject = value;
				return true;
			}

			visibleObject = null;
			return false;
		}

		/// <summary>
		/// Resize the VirtualizedScrollRectList, this will adjust the size of
		/// the ScrollRect's content area, and may trigger removal of visible
		/// items that no longer exist, as well as creation of items that are
		/// now visible.
		/// </summary>
		/// <param name="newCount">
		/// The new number of items this virtualized list represents.
		/// </param>
		public void SetItemCount(int newCount)
		{
			itemCount = newCount;
			scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, margin + itemCount * layoutPrefabSize + trailingSpace);
			contentStart = scrollRect.content.rect.yMax;

			visibleValid = false;
			UpdateScrollView(scroll);
		}

		/// <summary>
		/// Resets the VirtualizedScrollRectList. This may remove items aready
		/// visible and create new items.
		/// </summary>
		public void ResetLayout()
		{
			// We only want to reset things if it has already been initialized,
			// we don't want to initialize it prematurely!
			if (initialized == false)
			{
				return;
			}

			// We don't want to reset multiple times in one frame.
			if (resetCalled)
			{
				return;
			}

			resetCalled = true;
			StartCoroutine(ResetLayoutNextFrame());
		}
		#endregion

		public event CollectionChangeHandler<ILogicalElement> ElementAdding;
		public event CollectionChangeHandler<ILogicalElement> ElementAdded;
	}
}
