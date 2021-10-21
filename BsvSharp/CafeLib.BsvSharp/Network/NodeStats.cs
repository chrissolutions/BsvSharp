namespace CafeLib.BsvSharp.Network
{
    internal class NodeStats
    {
#if false
        long nodeid;
        ServiceFlags nServices;
        bool fRelayTxes;
        long nLastSend;
        long nLastRecv;
        long nTimeConnected;
        long nTimeOffset;
        string addrName;
        int nVersion;
        string cleanSubVer;
        bool fInbound;
        bool fAddnode;
        int nStartingHeight;
        long nSendBytes;
        //mapMsgCmdSize mapSendBytesPerMsgCmd;
        long nRecvBytes;
        //mapMsgCmdSize mapRecvBytesPerMsgCmd;
        bool fWhitelisted;
        double dPingTime;
        double dPingWait;
        double dMinPing;
        // What this peer sees as my address
        string addrLocal;
        //CAddress addr;
        uint nInvQueueSize;
#endif
    };
}