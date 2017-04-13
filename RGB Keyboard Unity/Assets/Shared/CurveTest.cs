using System.Linq;
using UnityEngine;
#if !UNITY_5
using System;
#endif

namespace CodeChicken.RGBKeyboard
{
	public static class CurveTest {

		public static void Test() {
			Test(AnimationCurve.Linear(0, 0, 1, 1), 0, 0.2f, 0.66f, 1f);

			Test(AnimationCurve.EaseInOut(0, 0, 1, 1), 0f, 0.2f, 0.66f, 1f);

			Test(AnimationCurve.EaseInOut(0.2f, 2, 1.2f, -3), 0f, 0.3f, 0.5f, 0.8f, 1f, 1.2f);

			Test(new AnimationCurve(new Keyframe(0, 0, 0, 8), new Keyframe(0.5f, 0, -1, -1), new Keyframe(1, 0, 0, 0)), 
				0f, 0.01f, 0.25f, 0.3f,
				0.49f, 0.5f, 0.51f,
				0.6f, 0.8f, 0.99f, 1f);
		}

		public static void Test(AnimationCurve curve, params float[] times) {
			Log(string.Join(", ", times.Select(t => t + ":" + curve.Evaluate(t)).ToArray()));
		}

		private static void Log(string s) {
#if UNITY_5
			Debug.Log(s);
#else
			Console.WriteLine(s);
#endif
		}
	}
}
