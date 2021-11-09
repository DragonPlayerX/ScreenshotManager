using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MelonLoader;

namespace ScreenshotManager.Tasks
{
    public static class AsyncProcessProvider
    {

        public static async Task StartProcess(ProcessStartInfo processStartInfo, Action<bool, int> onComplete, string processName = "Background Process")
        {
            await TaskProvider.YieldToBackgroundTask();

            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    MelonLogger.Error("[" + processName + "] " + e.Data);
            };

            try
            {
                process.Start();
                process.BeginErrorReadLine();

                bool hasExited = process.WaitForExit(30000);

                if (!hasExited)
                    process.Kill();

                await TaskProvider.YieldToMainThread();

                onComplete(hasExited, process.ExitCode);
            }
            catch (Exception e)
            {
                MelonLogger.Error("[" + processName + "] " + e);
                onComplete(false, -1);
            }
        }
    }
}
