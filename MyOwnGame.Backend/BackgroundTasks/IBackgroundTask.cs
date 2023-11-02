namespace MyOwnGame.Backend.BackgroundTasks;

public interface IBackgroundTask
{
    public Task Invoke();

    public int Timeout { get; }
}