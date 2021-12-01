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

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    MelonLogger.Msg(processName + " >> " + e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    MelonLogger.Error(processName + " >> " + e.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool hasExited = process.WaitForExit(30000);

                if (!hasExited)
                    process.Kill();

                await TaskProvider.YieldToMainThread();

                onComplete(hasExited, process.ExitCode);
            }
            catch (Exception e)
            {
                await TaskProvider.YieldToMainThread();
                MelonLogger.Error(processName + " >> " + e);
                onComplete(false, -1);
            }
        }
    }
}
