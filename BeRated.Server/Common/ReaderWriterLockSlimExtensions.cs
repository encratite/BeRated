using System;
using System.Threading;

namespace BeRated.Common
{
    static class ReaderWriterLockSlimExtensions
    {
        public static ReaderWriterLockScope ScopedReader(this ReaderWriterLockSlim @lock)
        {
            if (!@lock.TryEnterReadLock(TimeSpan.FromMinutes(1)))
                throw new ApplicationException("Unable to acquire read lock.");
            return new ReaderWriterLockScope(@lock, () => @lock.ExitReadLock());
        }

        public static ReaderWriterLockScope ScopedUpgradeableReader(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterUpgradeableReadLock();
            return new ReaderWriterLockScope(@lock, () => @lock.ExitUpgradeableReadLock());
        }

        public static ReaderWriterLockScope ScopedWriter(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterWriteLock();
            return new ReaderWriterLockScope(@lock, () => @lock.ExitWriteLock());
        }
    }
}
