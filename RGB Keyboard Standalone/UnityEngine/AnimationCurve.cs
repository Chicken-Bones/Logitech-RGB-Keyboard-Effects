using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class AnimationCurve
	{
		public Keyframe[] keys;

		public Keyframe this[int index] => keys[index];
		public int length => keys.Length;

		public AnimationCurve(params Keyframe[] keys)
		{
			this.keys = (Keyframe[])keys.Clone();
		}
		
		public AnimationCurve()
		{
			keys = new Keyframe[0];
		}
		
		public float Evaluate(float time) {
			if (keys.Length == 0) return 0;
			
			//index of upper key
			int i;
			for (i = 0; i < keys.Length; i++)
				if (keys[i].time >= time)
					break;

			if (i == 0) return keys[0].value;
			if (i == keys.Length) return keys[keys.Length - 1].value;

			var k0 = keys[i - 1];
			var k1 = keys[i];
			return Evaluate(Mathf.InverseLerp(k0.time, k1.time, time), k0, k1);
		}

		private static float Evaluate(float t, Keyframe keyframe0, Keyframe keyframe1) {
			float dt = keyframe1.time - keyframe0.time;

			float m0 = keyframe0.outTangent * dt;
			float m1 = keyframe1.inTangent * dt;

			float t2 = t * t;
			float t3 = t2 * t;

			float a = 2 * t3 - 3 * t2 + 1;
			float b = t3 - 2 * t2 + t;
			float c = t3 - t2;
			float d = -2 * t3 + 3 * t2;

			return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
		}

		public int AddKey(float time, float value) => AddKey(new Keyframe(time, value));

		public int AddKey(Keyframe key)
		{
			int p;
			for (p = 0; p < keys.Length; p++) {
				var t = keys[p].time;
				if (t > key.time) break;
				if (t == key.time) return -1;
			}

			var new_keys = new Keyframe[keys.Length + 1];
			for (int i = 0, j = 0; i < new_keys.Length; i++)
				if (i != p)
					new_keys[i] = keys[j++];

			new_keys[p] = key;
			keys = new_keys;

			return p;
		}

		public int MoveKey(int index, Keyframe key)
		{
			//slow naive implementation
			RemoveKey(index);
			return AddKey(key);
		}
		
		public void RemoveKey(int index) {
			var new_keys = new Keyframe[keys.Length - 1];
			for (int i = 0, j = 0; i < keys.Length; i++)
				if (i != index)
					new_keys[j++] = keys[i];

			keys = new_keys;
		}
		
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SmoothTangents(int index, float weight);

		public static AnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
		{
			float num = (valueEnd - valueStart) / (timeEnd - timeStart);
			Keyframe[] keys = new Keyframe[]
			{
				new Keyframe(timeStart, valueStart, 0f, num),
				new Keyframe(timeEnd, valueEnd, num, 0f)
			};
			return new AnimationCurve(keys);
		}

		public static AnimationCurve EaseInOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
		{
			Keyframe[] keys = new Keyframe[]
			{
				new Keyframe(timeStart, valueStart, 0f, 0f),
				new Keyframe(timeEnd, valueEnd, 0f, 0f)
			};
			return new AnimationCurve(keys);
		}
	}
}
