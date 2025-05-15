using System.Collections.Concurrent;

namespace ParallelTasks;

internal class Worker
{
    private readonly Thread thread;
    private readonly ConcurrentBag<Task> tasks = [];
    private readonly WorkStealingScheduler scheduler;

    public AutoResetEvent Gate { get; } = new AutoResetEvent(false);

    private static readonly ConcurrentDictionary<Thread, Worker> Workers = new();

    public static Worker? CurrentWorker
    {
        get
        {
            var currentThread = Thread.CurrentThread;
            return Workers.GetValueOrDefault(currentThread);
        }
    }

    public Worker(WorkStealingScheduler scheduler, int index)
    {
        thread = new Thread(Work)
        {
            Name = "ParallelTasks Worker " + index,
            IsBackground = true,
        };

        this.scheduler = scheduler;

        Workers[thread] = this;
    }

    public void Start()
    {
        thread.Start();
    }

    public void AddWork(Task task)
    {
        tasks.Add(task);
    }

    private void Work()
    {
        while (true)
        {
            if (tasks.TryTake(out var task))
            {
                task.DoWork();
            }
            else
            {
                FindWork();
            }
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private void FindWork()
    {
        Task task;
        var foundWork = false;
        do
        {
            if (scheduler.TryGetTask(out task))
            {
                break;
            }

            var replicable = WorkItem.Replicable;
            if (replicable.HasValue)
            {
                replicable.Value.DoWork();
                WorkItem.SetReplicableNull(replicable);

                // MartinG@DigitalRune: Continue checking local queue and replicables.
                // No need to steal work yet.
                continue;
            }

            for (var i = 0; i < scheduler.Workers.Count; i++)
            {
                var worker = scheduler.Workers[i];
                if (worker == this)
                {
                    continue;
                }

                if (worker.tasks.TryTake(out task))
                {
                    foundWork = true;
                    break;
                }
            }

            if (!foundWork)
            {
                // Wait until a new task gets scheduled.
                Gate.WaitOne();
            }
        } while (!foundWork);

        tasks.Add(task);
    }
}
