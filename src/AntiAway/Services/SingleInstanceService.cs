using System.Threading;

namespace AntiAway.Services;

public sealed class SingleInstanceService(string mutexName) : IDisposable
{
    private readonly Mutex _mutex = new(initiallyOwned: false, $"Local\\{mutexName}");
    private bool _ownsMutex;
    private bool _isDisposed;

    public bool TryAcquire()
    {
        try
        {
            _ownsMutex = _mutex.WaitOne(0, false);
        }
        catch (AbandonedMutexException)
        {
            _ownsMutex = true;
        }

        return _ownsMutex;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_ownsMutex)
        {
            _mutex.ReleaseMutex();
            _ownsMutex = false;
        }

        _mutex.Dispose();
        _isDisposed = true;
    }
}
