using System;
using System.Diagnostics;
using System.Threading;

namespace better_logcat
{
	public class ADBProcessWrapper
	{
		private static ADBProcessWrapper _instance = null;
		public static ADBProcessWrapper Instance => _instance ?? (_instance = new ADBProcessWrapper());

		private ADBProcessWrapper()
		{
			Init();
		}

		private const string LOGCAT_COMMAND = "adb logcat -v threadtime";

		public static event Action<string> OnOutputReceived;

		private void Init()
		{

		}

		public bool IsRunning()
		{
			return _activeProcess != null && !_activeProcess.HasExited && _activeThread != null && _activeThread.IsAlive;
		}

		public void StartMainProcess(bool isBackground = false)
		{
			if (IsRunning())
			{
				Console.WriteLine("Unable to start Main Process, already running");
				return;
			}
			_activeThread = new Thread(CreateAndStartProcess)
			{
				IsBackground = isBackground
			};

			_activeThread.Start();
		}

		public void Stop()
		{
			if (IsRunning())
			{
				_activeProcess.Kill();
				_activeThread.Abort();
			}
		}

		private void CreateAndStartProcess()
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = GetAdbPath(),
				Arguments = "logcat -v threadtime",
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
			};

			_activeProcess = new Process
			{
				StartInfo = startInfo,
			};

			_activeProcess.OutputDataReceived += DataReceived;
			_activeProcess.ErrorDataReceived += DataReceived;
			_activeProcess.Start();

			_activeProcess.BeginErrorReadLine();
			_activeProcess.BeginOutputReadLine();

			Console.WriteLine("Started ADB connection");
			_activeProcess.WaitForExit();
			_activeProcess.Close();
			_activeProcess = null;
			Console.WriteLine("Stopped ADB connection");

			CreateAndStartProcess();
		}

		private string GetAdbPath()
		{
			// TODO: Non-hardcoded path, detect Environment Variable - if not set up, prompt about this
			return "adb";
		}

		private void DataReceived(object sender, DataReceivedEventArgs eventArgs)
		{
			if (!string.IsNullOrEmpty(eventArgs?.Data))
			{
				OnOutputReceived?.Invoke(eventArgs.Data);
			}
		}

		public void GetDevices(Action<string> onDeviceReceived)
		{

		}

		public void ClearBuffer(string deviceId)
		{

		}

#pragma warning disable 169
		private Thread _activeThread;
		private Process _activeProcess;
#pragma warning restore 169
	}
}