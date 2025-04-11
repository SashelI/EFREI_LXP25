#if UNITY_2020_OR_NEWER
using UnityEngine.InputSystem.XInput;
#endif

namespace Assets.Astrolabe.Scripts.Tools
{
	public class XBoxControllerManager
	{
		public void CheckStates()
		{
#if UNITY_2020_OR_NEWER
        if(XInputController.current == null)
        {
            return;
        }

        if (TwinkleApplication.Instance.XBoxController.IsEnabled == true)
        {
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.A, XInputController.current.aButton.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.B, XInputController.current.bButton.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.X, XInputController.current.xButton.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Y, XInputController.current.yButton.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Start, XInputController.current.startButton.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Select, XInputController.current.selectButton.isPressed);

            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Up, XInputController.current.dpad.up.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Down, XInputController.current.dpad.down.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Right, XInputController.current.dpad.right.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.Left, XInputController.current.dpad.left.isPressed);

            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftShoulder, XInputController.current.leftShoulder.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftTrigger, XInputController.current.leftTrigger.isPressed);
            
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightShoulder, XInputController.current.rightShoulder.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightTrigger, XInputController.current.rightTrigger.isPressed);

            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftStickUp, XInputController.current.leftStick.up.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftStickDown, XInputController.current.leftStick.down.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftStickRight, XInputController.current.leftStick.right.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.LeftStickLeft, XInputController.current.leftStick.left.isPressed);

            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightStickUp, XInputController.current.rightStick.up.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightStickDown, XInputController.current.rightStick.down.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightStickRight, XInputController.current.rightStick.right.isPressed);
            TwinkleApplication.Instance.ExecuteButtonEvent(XBoxButton.RightStickLeft, XInputController.current.rightStick.left.isPressed);
        }
#endif
		}
	}
}