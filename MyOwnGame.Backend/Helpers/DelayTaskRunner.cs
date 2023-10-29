namespace MyOwnGame.Backend.Helpers;

public static class DelayTaskRunner
{
    public static void Run(int time, Func<Task> action)
    {
        Task.Run(async () =>
        {
            await Task.Delay(time * 1000);
            
            await action();
        });
    }
}