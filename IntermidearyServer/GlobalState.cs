public static class GlobalState
{
    private static readonly object _lock = new object();
    private static string _host;

    public static string Host
    {
        get
        {
            lock (_lock)
            {
                return _host;
            }
        }
        set
        {
            lock (_lock)
            {
                _host = value;
            }
        }
    }

}