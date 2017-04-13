using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LedCSharp;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	public class RippleEffect : LightingEffect
	{
		private static string Path { get { return System.IO.Path.Combine(Config.path, "ripple.json"); } }

		public Color baseColor;
		//HSL colour modifying curve based on wave height, 0.5 is baseline, with a range of complimentary troughs at 0 and peaks at 1
		public AnimationCurve lightnessCurve;

		//base wave, height between -1 and 1, peak roughly 1 unit in width, positioned with peak at 0
		public AnimationCurve waveCurve;
		//in units of keys, scales wave height as a function of distance
		//wave width is multiplied by 2-wave_height
		public AnimationCurve concentrationCurve;

		//keys
		public float depressionRadius;
		//keys
		public float waveWidth;
		//keys per second
		public float waveSpeed;

		private class Wave
		{
			public readonly Func<Vector2, float> distanceFunc;
			public double time;

			public Wave(keyboardNames key, double time) {
				this.distanceFunc = KeyLocations.DistanceFunction(key);
				this.time = time;
			}
		}

		private Queue<Wave> waves = new Queue<Wave>();

		public override void Init() {
			SetKeyboardLighting(baseColor);
		}

		private Color32 HeightToColor(float height) {
			float h, s, l;
			RGB2HSL(baseColor.r, baseColor.g, baseColor.b, out h, out s, out l);
			l += lightnessCurve.Evaluate(height / 4 + 0.5f);
			return HSL2RGB(h, s, Mathf.Clamp01(l));
		}

		public override Color Calculate(Vector2 pos) {
			float h = 0;
			foreach (var w in waves)
				h += WaveHeight(w, pos, time);

			foreach (var k in KeyboardState.HeldKeys)
				h += DepressionHeight(k, pos);

			return HeightToColor(h);
		}

		private float WaveHeight(Wave wave, Vector2 pos, double time) {
			var r = wave.distanceFunc(pos) / KeyLocations.Scale;
			var c = concentrationCurve.Evaluate(r);
			var w = waveWidth * (2 - c);
			var dt = time - wave.time;
			var p = (r - dt * waveSpeed) / w;
			return c * waveCurve.Evaluate((float)p);
		}

		private float DepressionHeight(keyboardNames key, Vector2 pos) {
			var r = KeyLocations.DistanceFunction(key)(pos) / KeyLocations.Scale;
			return -waveCurve.Evaluate(r / depressionRadius);
		}

		public override void KeyChanged(keyboardNames key, bool down) {
			if (!down)
				waves.Enqueue(new Wave(key, time));
		}

		public override void Update() {
			float timeout = 1.5f / KeyLocations.Scale / waveSpeed;
			while (waves.Count > 0 && (time - waves.Peek().time) > timeout)
				waves.Dequeue();
		}

		public override void UpdateLighting() {
			if (waves.Count == 0 && !KeyboardState.HeldKeys.Any())//performance optimisation
				SetKeyboardLighting(baseColor);
			else
				base.UpdateLighting();
		}

		public override void Save() {
			File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public override void Load() {
			JsonConvert.PopulateObject(File.ReadAllText(Path), this);
		}
	}
}
