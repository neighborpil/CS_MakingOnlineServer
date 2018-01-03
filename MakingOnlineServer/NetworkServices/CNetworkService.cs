using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServices
{
    public class CNetworkService
    {
        // 클라이언트의 접속을 받아들이기 위한 객체
        CListener client_listener;

        // 메세지 송수신시 필요한 객체
        SocketAsyncEventArgsPool receive_event_args_pool;
        SocketAsyncEventArgsPool send_event_args_pool;

        // 메시지 수신, 전송 시 닷넷 비동기 소켓에서 사용할 버퍼를 관리하는 객체
        BufferManager buffer_manager;

        // 클라이언트의 접속이 이루어졌을 때 호출되는 델리게이트
        public delegate void SessionHandler(CUserToken token);
        public SessionHandler session_created_callback { get; set; }

        public void listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callback_on_newclient += on_new_client;
            listener.start(host, port, backlog);
        }

        private void on_new_client(Socket client_socket, object token)
        {
            throw new NotImplementedException();
        }
    }
}
