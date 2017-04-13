using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LedCSharp;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	//keyboard is width 1, centered on zero, lower left is negative
	public class KeyLocations
	{
		private static IDictionary<keyboardNames, Vector2> map = new Dictionary<keyboardNames, Vector2>();
		private static IDictionary<keyboardNames, string> names = new Dictionary<keyboardNames, string>();
		private static IDictionary<string, keyboardNames> byName = new Dictionary<string, keyboardNames>();
		private static string Path { get { return System.IO.Path.Combine(Config.path, "keyboardlayout.json"); } } 

		public const keyboardNames SPACEBAR_LEFT = (keyboardNames) 0xEFF1;
		public const keyboardNames SPACEBAR_RIGHT = (keyboardNames) 0xEFF2;

		//1 unit is the distance between a and s
		public static float Scale = 0.045f;

		static KeyLocations() {
			foreach (var key in Enum.GetValues(typeof(keyboardNames)))
				AddName((keyboardNames) key, Enum.GetName(typeof(keyboardNames), key));

			AddName(SPACEBAR_LEFT, "SPACEBAR_LEFT");
			AddName(SPACEBAR_RIGHT, "SPACEBAR_RIGHT");
		}

		public static IEnumerable<KeyValuePair<keyboardNames, Vector2>> Locations {
			get { return map; }
		}

		public static void Load() {
			map.Clear();

			if (File.Exists(Path))
				foreach (var kv in JsonConvert.DeserializeObject<IDictionary<string, Vector2>>(File.ReadAllText(Path)))
					map.Add(byName[kv.Key], kv.Value);
		}

		private static void AddName(keyboardNames key, string name) {
			names[key] = name;
			byName[name] = key;
		}

		public static Vector2 Get(keyboardNames key) {
			Vector2 v;
			return map.TryGetValue(key, out v) ? v : Vector2.zero;
		}

#if UNITY_5
		public static void Set(keyboardNames key, Vector2 v) {
			Debug.Log("Location Set: " + names[key] + " = " + v);
			map[key] = v;
			Save();
		}
#endif

		public static Func<Vector2, float> DistanceFunction(keyboardNames key) {
			if (key == keyboardNames.SPACE) {
				var a = Get(SPACEBAR_LEFT);
				var b = Get(SPACEBAR_RIGHT);
				return p => SegmentDistance(a, b, p);
			}

			var loc = Get(key);
			return p => Vector2.Distance(p, loc);
		}

		public static float SegmentDistance(Vector2 v, Vector2 w, Vector2 p) {
			// Return minimum distance between line segment vw and point p
			float l2 = Vector2.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
			if (l2 == 0.0) return Vector2.Distance(p, v);   // v == w case
															// Consider the line extending the segment, parameterized as v + t (w - v).
															// We find projection of point p onto the line. 
															// It falls where t = [(p-v) . (w-v)] / |w-v|^2
															// We clamp t from [0,1] to handle points outside the segment vw.
			float t = Mathf.Clamp01(Vector2.Dot(p - v, w - v) / l2);
			var projection = v + t * (w - v);  // Projection falls on the segment
			return Vector2.Distance(p, projection);
		}

		private static void Save() {
			File.WriteAllText(Path, JsonConvert.SerializeObject(Locations.ToDictionary(e => names[e.Key], e => e.Value), Formatting.Indented));
		}
	}
}