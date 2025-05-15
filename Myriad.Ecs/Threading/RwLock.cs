using System.Threading;

namespace Exanite.Myriad.Ecs.Threading;

internal class RwLock<T>(T value) where T : class
{
    private readonly ReaderWriterLockSlim rwLock = new(LockRecursionPolicy.SupportsRecursion);

    public ReadLockHandle EnterReadLock()
    {
        rwLock.EnterReadLock();
        return new ReadLockHandle(rwLock, value);
    }

    public WriteLockHandle EnterWriteLock()
    {
        rwLock.EnterWriteLock();
        return new WriteLockHandle(rwLock, value);
    }

    public readonly ref struct ReadLockHandle
    {
        private readonly ReaderWriterLockSlim rwLock;
        public readonly T Value;

        internal ReadLockHandle(ReaderWriterLockSlim rwLock, T value)
        {
            this.rwLock = rwLock;
            Value = value;
        }

        public void Dispose()
        {
            rwLock.ExitReadLock();
        }
    }

    public readonly ref struct WriteLockHandle
    {
        private readonly ReaderWriterLockSlim @lock;
        public readonly T Value;

        internal WriteLockHandle(ReaderWriterLockSlim @lock, T value)
        {
            this.@lock = @lock;
            Value = value;
        }

        public void Dispose()
        {
            @lock.ExitWriteLock();
        }
    }
}
