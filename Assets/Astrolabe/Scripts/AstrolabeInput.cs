using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Astrolabe.Scripts
{
	public class AstrolabeInput : PressableButton
	{
		protected override void OnFocusEntered(FocusEnterEventArgs args)
		{
			base.OnFocusEntered(args);
			PointerRouter.Instance.ExecuteFocusEnterEvent(args);
		}

		protected override void OnFocusExited(FocusExitEventArgs args)
		{
			base.OnFocusExited(args);
			PointerRouter.Instance.ExecuteFocusExitEvent(args);
		}

		protected override void OnSelectExited(SelectExitEventArgs args)
		{
			base.OnSelectExited(args);
			PointerRouter.Instance.ExecutePointerEvent(args, LogicalElementHandledEvent.PointerReleased);
		}

		protected override void OnSelectEntered(SelectEnterEventArgs args)
		{
			base.OnSelectEntered(args);
			PointerRouter.Instance.ExecutePointerEvent(args, LogicalElementHandledEvent.PointerPressed);
		}
	}
}