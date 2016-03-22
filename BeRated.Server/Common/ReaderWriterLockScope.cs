using System;
using System.Threading;

namespace BeRated.Common
{
    enum ReaderWriterLockMode
    {
        Reader,
        Upgradeable,
        Writer,
    }

    class ReaderWriterLockScope : IDisposable
    {
        private ReaderWriterLockSlim _Lock;

        private Action _DisposeHandler;

        public ReaderWriterLockScope(ReaderWriterLockSlim @lock, Action disposeHandler)
        {
            _Lock = @lock;
            _DisposeHandler = disposeHandler;
        }

        void IDisposable.Dispose()
        {
            _DisposeHandler();
        }
    }
}
