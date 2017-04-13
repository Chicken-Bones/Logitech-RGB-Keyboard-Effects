using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CodeChicken.RGBKeyboard
{
	public class LLKeyboardHook
	{
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;
		private const int LLKHF_EXTENDED = 1;

		private static IntPtr _hookID = IntPtr.Zero;
		private static LowLevelKeyboardProc procGCHandle;//prevent GC

		private static int[] buffer = new int[256];
		private static volatile int read_index;
		private static volatile int write_index;

		public const VK NUMPAD_RETURN = (VK) 0x0E;

		public static void Hook() {
			if (_hookID != IntPtr.Zero)
				Unhook();

			_hookID = SetHook(procGCHandle = HookCallback);
		}

		public static void Unhook() {

			procGCHandle = null;
			UnhookWindowsHookEx(_hookID);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc) {
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule) {
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
					GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static int ExtendedRemap(int keyCode) {
			switch ((VK) keyCode) {
				case VK.HOME:
					return (int) VK.NUMPAD7;
				case VK.UP:
					return (int) VK.NUMPAD8;
				case VK.PRIOR:
					return (int) VK.NUMPAD9;
				case VK.LEFT:
					return (int) VK.NUMPAD4;
				case VK.CLEAR:
					return (int) VK.NUMPAD5;
				case VK.RIGHT:
					return (int) VK.NUMPAD6;
				case VK.END:
					return (int) VK.NUMPAD1;
				case VK.DOWN:
					return (int) VK.NUMPAD2;
				case VK.NEXT:
					return (int) VK.NUMPAD3;
				case VK.INSERT:
					return (int) VK.NUMPAD0;
				case VK.DELETE:
					return (int) VK.DECIMAL;
			}
			return keyCode;
		}

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if (nCode >= 0) {
				int evtType = wParam.ToInt32();
				int keyCode = Marshal.ReadInt32(lParam);
				int flags = Marshal.ReadInt32((IntPtr) (lParam.ToInt64() + 8));
				bool extended = (flags & LLKHF_EXTENDED) != 0;

				if (!extended)
					keyCode = ExtendedRemap(keyCode);
				if (extended && (VK) keyCode == VK.RETURN)
					keyCode = (int) NUMPAD_RETURN;

				if (evtType == WM_KEYDOWN || evtType == WM_SYSKEYDOWN)
					keyCode |= 0x100;

				Thread.VolatileWrite(ref buffer[write_index], keyCode);
				write_index = (write_index + 1) & 0xFF;
			}

			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		public static bool Read(out VK vk, out bool down) {
			if (read_index != write_index) {
				int keyCode = Thread.VolatileRead(ref buffer[read_index]);
				read_index = (read_index + 1) & 0xFF;
				vk = (VK) (keyCode & 0xFF);
				down = (keyCode & 0x100) != 0;
				return true;
			}

			vk = 0;
			down = false;
			return false;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}