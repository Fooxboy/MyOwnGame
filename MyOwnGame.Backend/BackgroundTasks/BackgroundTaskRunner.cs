using MyOwnGame.Backend.Helpers;

namespace MyOwnGame.Backend.BackgroundTasks;

public class BackgroundTaskRunner(IEnumerable<IBackgroundTask> backgroundTasks)
{
    public void Run()
    {
        foreach (var backgroundTask in backgroundTasks)
        {
            var thread = new Thread(async () =>
            {
                await RunThread(backgroundTask);
            });

            thread.Name = nameof(backgroundTask);
            
            thread.Start();
        }
    }

    private async Task RunThread(IBackgroundTask task)
    {
        while (true)
        {
            await task.Invoke();
            
            Thread.Sleep(task.Timeout * 1000);
        }
    }
}