using System;
using System.Collections.Generic;
using Astrolabe.Twinkle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Astrolabe.Scripts.Operations
{
	/// <summary>
	///  Permet de gerer un clavier physique et virtuel et d'envoyer les evenements aux LogicalElement qui possède le Focus
	/// </summary>
	public class UnityKeyboardOperations : IKeyboardOperations
	{
		// Keyboard polling: 9.504352E-06 s / frame
		// (as timed, avg of 1000 frames, profiler reports 0.00 ms)

		private readonly int[] _values;

		private readonly Dictionary<int, bool> _keysPressedStatus = new();
		private readonly Dictionary<int, int> _keyCodeByIndexValue = new();

		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;

		public UnityKeyboardOperations()
		{
			_values = (int[])Enum.GetValues(typeof(KeyCode));

			// Pour SendVirtualKey
			for (var i = 0; i < _values.Length; i++)
			{
				var keyCode = _values[i];

				// Possible voir KeyCode 309 Commande Apple
				if (_keyCodeByIndexValue.ContainsKey(keyCode) == false)
				{
					_keyCodeByIndexValue.Add(keyCode, i);

					_keysPressedStatus.Add(i, false);
				}
			}

			// keys = new bool[values.Length];

			IsEnabled = true;
		}

		public bool IsEnabled
		{
			get => _isEnabled;

			set
			{
				if (_isEnabled != value)
				{
					_isEnabled = value;

					if (value == true)
					{
						TwinkleApplication.Instance.Rendering += OnRendering;
					}
					else
					{
						TwinkleApplication.Instance.Rendering -= OnRendering;
					}
				}
			}
		}

		private bool _isEnabled = false;

		private void OnRendering(object sender, EventArgs e)
		{
			for (var i = 0; i < _values.Length; i++)
			{
				var keyCode = _values[i];
				var key = (Key)keyCode;

				// KeyCode qu'on ne gère pas
				if (Keyboard.current == null || keyCode > Keyboard.current.allKeys.Count - 1
				                             || keyCode == 0 || key == Key.None)
				{
					continue;
				}

				var isPressed = Keyboard.current[key].isPressed;

				ExecutePressKeyIfNecessary(i, keyCode, isPressed);
			}
		}

		private void ExecutePressKeyIfNecessary(int unityKeyCodeIndex, int keyCode, bool isPressed)
		{
			bool isOldKeyPressed;


			if (!_keysPressedStatus.ContainsKey(unityKeyCodeIndex))
			{
				// Pas dans le dico, toujours à faux
				// Cas des virtualkey hors giron unity
				isOldKeyPressed = false;

				// On le rajoute
				_keysPressedStatus.Add(unityKeyCodeIndex, isOldKeyPressed);
			}

			isOldKeyPressed = _keysPressedStatus[unityKeyCodeIndex];

			_keysPressedStatus[unityKeyCodeIndex] = isPressed;

			if (isPressed != isOldKeyPressed)
			{
				if (isPressed == true)
				{
					KeyDown?.Invoke(this, new ValueEventArgs<VirtualKey>((VirtualKey)keyCode));
				}
				else
				{
					KeyUp?.Invoke(this, new ValueEventArgs<VirtualKey>((VirtualKey)keyCode));
				}
			}
		}

		public void SendVirtualKey(VirtualKey virtualKey, bool isPressed)
		{
			var keyCode = (int)virtualKey;

			var unityKeyCodeIndex = -1;

			// Attention, les virtualkey custom ne sont pas pris en compte par les keycode unity
			// On va donc bypasser la récupération, ce n'est pas grave, il faudra juste que le récepteur de l'event contrôle le virtualkey
			if (_keyCodeByIndexValue.ContainsKey(keyCode))
			{
				unityKeyCodeIndex = _keyCodeByIndexValue[keyCode];
			}
			else
			{
				// On va mettre la valeur du keycode du virtualkey
				unityKeyCodeIndex = keyCode;
			}

			ExecutePressKeyIfNecessary(unityKeyCodeIndex, keyCode, isPressed);
		}
	}
}