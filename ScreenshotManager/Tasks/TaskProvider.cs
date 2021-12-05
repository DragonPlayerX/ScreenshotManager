using System;
using System.Threading.Tasks;
using MelonLoader;

namespace ScreenshotManager.Tasks
{
    public static class TaskProvider
    {

        public static readonly AwaitProvider mainThreadQueue = new AwaitProvider("ScreenshotManager.MainThread");

        public static AwaitProvider.YieldAwaitable YieldToMainThread()
        {
            return mainThreadQueue.Yield();
        }

        public static async Task YieldToBackgroundTask()
        {
            await Task.Run(() => { }).ConfigureAwait(false);
        }

        public static async Task RunLater(Action action, int delay)
        {
            await YieldToBackgroundTask();
            await Task.Delay(delay);
            await YieldToMainThread();
            action?.Invoke();
        }

        public static void NoAwait(this Task task, Action onComplete = null)
        {
            task.ContinueWith(t =>
            {
                onComplete?.Invoke();
                if (t.IsFaulted)
                    MelonLogger.Error("Task raised exception: " + t.Exception);
            });
        }
    }
}
