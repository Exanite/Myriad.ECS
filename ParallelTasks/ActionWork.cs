namespace ParallelTasks;

internal class ActionWork
    : IWork
{
    private static readonly Pool<ActionWork> Instances = Pool<ActionWork>.Instance;

    public Action? Action { get; set; }
    public WorkOptions Options { get; set; }

    public void DoWork()
    {
        Action?.Invoke();
        Action = null;

        Instances.Return(this);
    }

    internal static ActionWork GetInstance()
    {
        return Instances.Get();
    }
}