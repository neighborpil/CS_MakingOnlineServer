﻿using NetworkServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSampleClient
{
    class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }

        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            CPacket msg = new CPacket(buffer.Value, this);
            PROTOCOL protoco_id = (PROTOCOL)msg.pop_protocol_id();
            switch (protoco_id)
            {
                case PROTOCOL.CHAT_MSG_ACK:
                    {
                        string text = msg.pop_string();
                        Console.WriteLine($"ack text {text}");
                    }
                    break;
            }
        }

        void IPeer.on_removed()
        {
            Console.WriteLine("Server removed");
        }

        void IPeer.send(CPacket msg)
        {
            this.token.send(msg);
        }

        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        void IPeer.process_user_operation(CPacket msg) { }
    }
}
