public static class GlobalState
{
    private static readonly object _lock = new object();
    private static int _peerCount;

    public static int PeerCount
    {
        get
        {
            lock (_lock)
            {
                return _peerCount;
            }
        }
        set
        {
            lock (_lock)
            {
                _peerCount = value;
            }
        }
    }

    public static void IncrementPeerCount()
    {
        lock (_lock)
        {
            _peerCount++;
        }
    }

    public static void DecrementPeerCount()
    {
        lock (_lock)
        {
            if (_peerCount > 0)
                _peerCount--;
        }
    }
}