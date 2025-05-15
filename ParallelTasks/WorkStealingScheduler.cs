namespace ParallelTasks;

/// <summary>
/// A "work stealing" work scheduler class.
/// </summary>
internal class WorkStealingScheduler
{
    internal List<Worker> Workers { get; }

    private int tasksCount;
    private readonly Queue<Task> tasks;

    /// <summary>
    /// Creates a new instance of the <see cref="WorkStealingScheduler"/> class.
    /// </summary>
    public WorkStealingScheduler()
        : this(Environment.ProcessorCount)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="WorkStealingScheduler"/> class.
    /// </summary>
    /// <param name="numThreads">The number of threads to create.</param>
    private WorkStealingScheduler(int numThreads)
    {
        tasks = new Queue<Task>();
        tasksCount = 0;

        Workers = new List<Worker>(numThreads);
        for (var i = 0; i < numThreads; i++)
        {
            Workers.Add(new Worker(this, i));
        }

        for (var i = 0; i < numThreads; i++)
        {
            Workers[i].Start();
        }
    }

    internal bool TryGetTask(out Task task)
    {
        if (tasksCount == 0)
        {
            task = default;
            return false;
        }

        lock (tasks)
        {
            if (tasks.Count > 0)
            {
                task = tasks.Dequeue();
                tasksCount--;
                return true;
            }

            task = default;
            return false;
        }
    }

    /// <summary>
    /// Schedules a task for execution.
    /// </summary>
    /// <param name="task">The task to schedule.</param>
    public void Schedule(Task task)
    {
        var threads = task.Item.Work.Options.MaximumThreads;
        var worker = Worker.CurrentWorker;
        if (worker != null)
        {
            worker.AddWork(task);
        }
        else
        {
            lock (tasks)
            {
                tasks.Enqueue(task);
                tasksCount++;
            }
        }

        if (threads > 1)
        {
            WorkItem.Replicable = task;
        }

        for (var i = 0; i < Workers.Count; i++)
        {
            Workers[i].Gate.Set();
        }
    }
}
