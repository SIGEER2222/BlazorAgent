using System.Threading;

namespace Factory.Web.Jincheng;

public class JinchengDataStore
{
    private JinchengDashboardData _data = new()
    {
        AlertCount = 67,
        CongestionIndex = 1.4,
        AvgSpeedKmH = 120
    };

    private readonly ReaderWriterLockSlim _lock = new();

    public JinchengDashboardData Get()
    {
        _lock.EnterReadLock();
        try { return _data; }
        finally { _lock.ExitReadLock(); }
    }

    public void Set(JinchengDashboardData data)
    {
        _lock.EnterWriteLock();
        try { _data = data; }
        finally { _lock.ExitWriteLock(); }
    }
}

