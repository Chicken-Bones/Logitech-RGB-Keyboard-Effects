using LedCSharp;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace CodeChicken.RGBKeyboard
{
	public class Program
	{
		public static LightingEffect Effect = new RippleEffect();

		public static void Main(string[] args) {
			Init(args.Length > 0 ? args[0] : "");
			new Thread(Loop) {
				IsBackground = true,
				Name = "Effect Thread"
			}.Start();
			Application.Run();
		}

		private static void Init(string path) {
			Config.path = path;

			LogitechGSDK.LogiLedInit();
			LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

			KeyLocations.Load();
			LLKeyboardHook.Hook();

			Effect.Init();
			Effect.Load();
		}

		private static void Loop() {
			var sw = new Stopwatch();
			sw.Start();
			try {
				while (true) {
					Thread.Sleep(1000 / LightingEffect.RATE);
					Update(sw.Elapsed.TotalSeconds);
				}
			}
			finally {
				Shutdown();
			}
		}

		private static void Update(double time) {
			Effect.time = time;
			KeyboardState.ProcessLLInput(Effect.KeyChanged);
			Effect.Update();
			Effect.UpdateLighting();
		}

		private static void Shutdown() {
			LogitechGSDK.LogiLedShutdown();
			LLKeyboardHook.Unhook();
		}
	}
}
