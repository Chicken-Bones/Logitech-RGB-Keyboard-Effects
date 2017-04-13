using System.Linq;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	[RequireComponent(typeof(LightingController))]
	public class RippleEffectScript : MonoBehaviour, ILightingEffectScript
	{
		private RippleEffect effect = new RippleEffect();

		public Color baseColor;
		public AnimationCurve lightnessCurve;
		public Gradient gradient; //purely for visualisation

		public AnimationCurve waveCurve;
		public AnimationCurve concentrationCurve;

		public float depressionRadius;
		public float waveWidth;
		public float waveSpeed;

		public LightingEffect GetEffect() {
			UpdateParameters();
			return effect;
		}

		void Update() {
			UpdateParameters();
			UpdateLightnessCurveVis();
		}

		private void UpdateParameters() {
			effect.baseColor = baseColor;
			effect.lightnessCurve = lightnessCurve;
			effect.waveCurve = waveCurve;
			effect.concentrationCurve = concentrationCurve;
			effect.depressionRadius = depressionRadius;
			effect.waveWidth = waveWidth;
			effect.waveSpeed = waveSpeed;
		}

		private void UpdateLightnessCurveVis() {
			float h, s, l;
			LightingEffect.RGB2HSL(baseColor.r, baseColor.g, baseColor.b, out h, out s, out l);

			var ckeys = lightnessCurve.keys.Select(k =>
				new GradientColorKey(LightingEffect.HSL2RGB(h, s, Mathf.Clamp01(l + k.value)), k.time)).ToArray();
		
			gradient.SetKeys(ckeys, gradient.alphaKeys);
		}
	}
}
