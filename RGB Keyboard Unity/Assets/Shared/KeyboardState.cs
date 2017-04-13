using System;
using System.Collections.Generic;
using LedCSharp;

namespace CodeChicken.RGBKeyboard
{
	public class KeyboardState
	{
		private static List<keyboardNames> heldList = new List<keyboardNames>();
		private static HashSet<keyboardNames> heldSet = new HashSet<keyboardNames>();

		public static IEnumerable<keyboardNames> HeldKeys {
			get { return heldList; }
		}

		public static bool Held(keyboardNames key) {
			return heldSet.Contains(key);
		}

		public static void ProcessLLInput(Action<keyboardNames, bool> callback) {
			VK vk;
			bool down;
			while (LLKeyboardHook.Read(out vk, out down)) {
				keyboardNames key;
				if (!KeyCodeMap.map.TryGetValue(vk, out key))
					continue;

				if (down && heldSet.Add(key)) {
					heldList.Add(key);
					callback(key, true);
				}
				else if (!down && heldSet.Remove(key)) {
					heldList.Remove(key);
					callback(key, false);
				}
			}
		}
	}
}
