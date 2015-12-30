using System;

namespace SimpleService.Protocol
{
    internal interface IClient
    {
        #region Properties
        Peer Peer { get; }
        #endregion

        #region Methods
        void Write(Packet p);
        Packet Read();
        #endregion
    }
}
