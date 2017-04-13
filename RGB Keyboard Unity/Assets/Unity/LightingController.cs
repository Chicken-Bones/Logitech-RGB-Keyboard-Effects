using LedCSharp;
using UnityEditor;
using UnityEngine;

namespace CodeChicken.RGBKeyboard
{
	public class LightingController : MonoBehaviour
	{
		public LightingEffect Effect { get; private set; }

		public bool show_texture;
		public int res = 128;
		public Texture2D texture;
		private Color32[] data;

		public bool showKeys;
		public bool live = true;
		public bool seed;
		[Range(-5, 5)]
		public float simTime;

		private bool updating_location;
		private int alt_location;
		private Vector2 location;

		private float simStartTime;
		private float timeLost;
		public float Time {
			get {
				return live ? UnityEngine.Time.time - timeLost : simTime + simStartTime;
			}
		}

		void Awake() {
			Application.targetFrameRate = LightingEffect.RATE;

			Effect = GetComponent<ILightingEffectScript>().GetEffect();

			LogitechGSDK.LogiLedInit();
			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

			Config.path = "../build";
			
			KeyLocations.Load();
			LLKeyboardHook.Hook();

			Effect.Init();
		}

		private void OnDestroy() {
			LogitechGSDK.LogiLedShutdown();
			Destroy(texture);
			LLKeyboardHook.Unhook();
		}

		private void Update() {
			Effect.time = Time;
			KeyboardState.ProcessLLInput(KeyChanged);
			Effect.Update();
			Effect.UpdateLighting();

			UpdateTexture();
			HandlePositionInput();
		}

		private void KeyChanged(keyboardNames key, bool down) {
			if (live || seed)
				Effect.KeyChanged(key, down);

			if (updating_location) {
				if (key == keyboardNames.SPACE)
					key = alt_location == 1 ? KeyLocations.SPACEBAR_RIGHT :
						alt_location == -1 ? KeyLocations.SPACEBAR_LEFT : keyboardNames.SPACE;

				if (key == keyboardNames.G && alt_location != 0)
					key = keyboardNames.G_LOGO;

				KeyLocations.Set(key, location);
				updating_location = false;
			}
		}

		private void UpdateTexture() {
			if (!show_texture)
				return;

			if (texture == null || texture.width != res || texture.height != res) {
				texture = new Texture2D(res, res, TextureFormat.ARGB32, false, true);
				GetComponent<MeshRenderer>().material.mainTexture = texture;
				data = new Color32[res * res];
			}

			for (int y = 0; y < res; y++)
				for (int x = 0; x < res; x++)
					data[y * res + x] = Effect.Calculate(new Vector2((x + 0.5f) / res - 0.5f, (y + 0.5f) / res - 0.5f));

			texture.SetPixels32(data);
			texture.Apply();
		}

		private void HandlePositionInput() {
			if (Input.GetMouseButtonDown(1)) {
				location = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				updating_location = true;
				alt_location = 0;
				Debug.Log("Updating Location " + location);
			}

			if (Input.mouseScrollDelta.y != 0) {
				alt_location = (int)Mathf.Sign(Input.mouseScrollDelta.y);
			}
		}

		public void ToggleLive() {
			if (live) {
				simStartTime = Time;
				seed = true;
				simTime = 0;
			} else {
				timeLost = UnityEngine.Time.time - Time;
			}
			live ^= true;
		}

		private void OnDrawGizmos() {
			if (showKeys && EditorApplication.isPlaying) {
				foreach (var kv in KeyLocations.Locations) {
					var key = kv.Key;
					if (key == KeyLocations.SPACEBAR_LEFT || key == KeyLocations.SPACEBAR_RIGHT)
						key = keyboardNames.SPACE;

					Gizmos.color = KeyboardState.Held(key) ? Color.red : Color.green;
					Gizmos.DrawSphere(kv.Value, 0.005f);
				}
			}
		}
	}
}
