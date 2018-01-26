using NetworkServices;
using System;

namespace CSampleServer
{
    class CGameUser : IPeer
    {
        CUserToken token;

        /// <summary>
        /// 메시지 송 수신에 사용할 CUserToken 객체를 멤버 변수로 보관
        /// CUserToken 객체에 IPeer 인터페이스를 구현한 자기 자신의 인스턴스를ㄴ 넘겨줌
        /// </summary>
        /// <param name="token"></param>
        public CGameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }

        /// <summary>
        /// 클라이언트로부터 메세지가 수신되었을 때 호출
        /// </summary>
        /// <param name="buffer"></param>
        void IPeer.on_message(Const<byte[]> buffer)
        {
            // ex)
            CPacket msg = new CPacket(buffer.Value, this);
            PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"Protocol id {protocol}");
            switch (protocol)
            {
                case PROTOCOL.CHAT_MSG_REQ: // 에코서버
                    {
                        string text = msg.pop_string();
                        Console.WriteLine($"text {text}");

                        CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK); // 응답 표시
                        response.push(text);
                        send(response);
                    }
                    break;
            }
        }

        void IPeer.on_removed()
        {
            Console.WriteLine("The client disconnected");
            Program.remove_user(this);
        }

        public void send(CPacket msg)
        {
            this.token.send(msg);
        }

        /// <summary>
        /// 서버에서 클라이언트의 연결을 강제로 끊을때 사용
        /// </summary>
        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        void IPeer.process_user_operation(CPacket msg)
        {
        }

        
    }
}