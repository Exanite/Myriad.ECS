namespace ParallelTasks;

internal class ForLoopWork
    : IWork
{
    private Action<int>? action;

    private int length;
    private int stride;
    private volatile int index;

    public WorkOptions Options => new() { MaximumThreads = int.MaxValue };

    public void Prepare(Action<int> action, int startInclusive, int endExclusive, int stride)
    {
        this.action = action;
        index = startInclusive;
        length = endExclusive;
        this.stride = stride;
    }

    public void DoWork()
    {
        int start;
        while ((start = IncrementIndex()) < length)
        {
            var end = Math.Min(start + stride, length);
            for (var i = start; i < end; i++)
            {
                action!(i);
            }
        }
    }

    private int IncrementIndex()
    {
        return Interlocked.Add(ref index, stride) - stride;
    }

    public static ForLoopWork Get()
    {
        return Pool<ForLoopWork>.Instance.Get();
    }

    public void Return()
    {
        Pool<ForLoopWork>.Instance.Return(this);
    }
}

internal class ForEachLoopWork<T>
    : IWork
{
    private Action<T>? action;
    private IEnumerator<T>? enumerator;

    private volatile bool notDone;
    private readonly object syncLock = new();

    public WorkOptions Options => new() { MaximumThreads = int.MaxValue };

    public void Prepare(Action<T> action, IEnumerator<T> enumerator)
    {
        this.action = action;
        this.enumerator = enumerator;
        notDone = true;
    }

    public void DoWork()
    {
        var item = default(T);
        while (notDone)
        {
            var haveValue = false;
            lock (syncLock)
            {
                // ReSharper disable once AssignmentInConditionalExpression
                if (notDone = enumerator!.MoveNext())
                {
                    item = enumerator.Current;
                    haveValue = true;
                }
            }

            if (haveValue)
            {
                action!(item!);
            }
        }
    }

    public static ForEachLoopWork<T> Get()
    {
        return Pool<ForEachLoopWork<T>>.Instance.Get();
    }

    public void Return()
    {
        Pool<ForEachLoopWork<T>>.Instance.Return(this);
    }
}