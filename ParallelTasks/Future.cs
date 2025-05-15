namespace ParallelTasks;

/// <summary>
/// A task struct which can return a result.
/// </summary>
/// <typeparam name="T">The type of result this future calculates.</typeparam>
public readonly struct Future<T>
{
    private readonly Task task;
    private readonly FutureWork<T>? work;
    private readonly int id;

    /// <summary>
    /// Gets a value which indicates if this future has completed.
    /// </summary>
    public bool IsComplete => task.IsComplete;

    /// <summary>
    /// Gets an array containing any exceptions thrown by this future.
    /// </summary>
    public Exception[] Exceptions => task.Exceptions ?? [];

    internal Future(Task task, FutureWork<T> work)
    {
        this.task = task;
        this.work = work;
        id = work.Id;
    }

    /// <summary>
    /// Gets the result. Blocks the calling thread until the future has completed execution.
    /// This can only be called once!
    /// </summary>
    public T GetResult()
    {
        if (work == null || work.Id != id)
        {
            throw new InvalidOperationException("The result of a future can only be retrieved once.");
        }

        task.Wait();
        var result = work.Result!;
        work.ReturnToPool();

        return result;
    }
}

internal class FutureWork<T>
    : IWork
{
    public int Id { get; private set; }
    public WorkOptions Options { get; set; }

    public Func<T>? Function { get; set; }
    public T? Result { get; private set; }

    public void DoWork()
    {
        Result = Function!();
    }

    public static FutureWork<T> GetInstance()
    {
        return Pool<FutureWork<T>>.Instance.Get();
    }

    public void ReturnToPool()
    {
        // Always increment ID, to invalidate any references to this work item
        Id++;

        // Only add it to the pool if it's not near overflowing
        if (Id < int.MaxValue - 10)
        {
            Function = null;
            Result = default;

            Pool<FutureWork<T>>.Instance.Return(this);
        }
    }
}
