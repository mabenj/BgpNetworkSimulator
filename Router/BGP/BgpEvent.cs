namespace Router
{
    //https://www.freesoft.org/CIE/RFC/1771/52.htm
    public enum BgpEvent
    {
        Start,
        Stop,
        TransportConnectionOpen,
        TransportConnectionClosed,
        TransportConnectionFailed,
        TransportFatalError,
        ConnectRetryTimerExpired,
        HoldTimerExpired,
        KeepAliveTimerExpired,
        ReceiveOpenMessage,
        ReceiveKeepAliveMessage,
        ReceiveUpdateMessage,
        ReceiveNotificationMessage
    }
}