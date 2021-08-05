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

        public static void NoAwait(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    MelonLogger.Error("Task raised exception: " + t.Exception);
            });
        }
    }
}
