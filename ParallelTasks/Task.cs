namespace ParallelTasks;

/// <summary>
/// A struct which represents a single execution of an IWork instance.
/// </summary>
public readonly struct Task
{
    private readonly bool valid;

    internal WorkItem Item { get; }
    internal int Id { get; }

    /// <summary>
    /// Gets a value which indicates if this task has completed.
    /// </summary>
    public bool IsComplete => !valid || Item.RunCount != Id;

    /// <summary>
    /// Gets an array containing any exceptions thrown by this task.
    /// </summary>
    public Exception[]? Exceptions
    {
        get
        {
            if (valid && IsComplete)
            {
                Item.Exceptions.TryGetValue(Id, out var e);
                return e;
            }
            return null;
        }
    }

    internal Task(WorkItem item)
        : this()
    {
        Id = item.RunCount;
        Item = item;
        valid = true;
    }

    /// <summary>
    /// Waits for the task to complete.
    /// </summary>
    public void Wait()
    {
        if (valid)
        {
            var currentTask = WorkItem.CurrentTask;
            if (currentTask.HasValue && currentTask.Value.Item == Item && currentTask.Value.Id == Id)
            {
                throw new InvalidOperationException("A task cannot wait on itself.");
            }

            Item.Wait(Id);
        }
    }

    internal void DoWork()
    {
        if (valid)
        {
            Item.DoWork(Id);
        }
    }
}