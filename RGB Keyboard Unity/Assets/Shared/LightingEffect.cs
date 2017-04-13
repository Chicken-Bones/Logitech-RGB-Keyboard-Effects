using LedCSharp;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	public abstract class LightingEffect
	{
		public const int RATE = 30;

		[JsonIgnore]
		public double time;
		
		public abstract void Init();

		public abstract Color Calculate(Vector2 pos);

		public virtual Color Calculate(keyboardNames key) {
			return Calculate(KeyLocations.Get(key));
		}
		
		public virtual void KeyChanged(keyboardNames key, bool down) { }

		public virtual void Update() {}

		public virtual void UpdateLighting() {
			foreach (var kv in KeyLocations.Locations)
				SetKeyLighting(kv.Key, Calculate(kv.Value));
		}

		public virtual void Save() {}
		public virtual void Load() {}

		// STATIC HELPERS

		public static void SetKeyboardLighting(Color color) {
			LogitechGSDK.LogiLedSetLighting((int)(color.r * 100), (int)(color.g * 100), (int)(color.b * 100));
		}

		public static void SetKeyLighting(keyboardNames key, Color color) {
			LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(key, (int)(color.r * 100), (int)(color.g * 100), (int)(color.b * 100));
		}

		public static void RGB2HSL(float r, float g, float b, out float h, out float s, out float l) {
			float max = Mathf.Max(r, g, b), min = Mathf.Min(r, g, b);
			l = (max + min) / 2;

			if (max == min) {
				h = s = 0; // achromatic
			} else {
				var d = max - min;
				s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
				if (max == r)
					h = (g - b) / d + (g < b ? 6 : 0);
				else if (max == g)
					h = (b - r) / d + 2;
				else
					h = (r - g) / d + 4;
			}
			h /= 6;
		}

		private static float HSL2RGBHelper(float p, float q, float t) {
			if (t < 0) t += 1;
			if (t > 1) t -= 1;
			if (t < 1 / 6f) return p + (q - p) * 6 * t;
			if (t < 1 / 2f) return q;
			if (t < 2 / 3f) return p + (q - p) * (2 / 3f - t) * 6;
			return p;
		}

		public static Color HSL2RGB(float h, float s, float l) {
			if (s == 0)
				return new Color(l, l, l);

			var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
			var p = 2 * l - q;
			return new Color(
				HSL2RGBHelper(p, q, h + 1 / 3f),
				HSL2RGBHelper(p, q, h),
				HSL2RGBHelper(p, q, h - 1 / 3f));
		}
	}
}
