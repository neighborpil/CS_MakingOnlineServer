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

        // configs
        int max_connections;
        int buffer_size;
        readonly int pre_alloc_count = 2; //read, write


        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public void initialize()
        {
            this.max_connections = 1000;
            this.buffer_size = 1024;

            this.buffer_manager = new BufferManager(this.max_connections * this.buffer_size * this.pre_alloc_count, this.buffer_size);
            this.receive_event_args_pool = new SocketAsyncEventArgsPool(this.max_connections);
            this.send_event_args_pool = new SocketAsyncEventArgsPool(this.max_connections);

            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            this.buffer_manager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs arg;

            for (int i = 0; i < this.max_connections; i++)
            {
                // 동일한 소켓에 대고 send, receive를 하므로
                // user token은 세션별로 하나씩만 만들어 놓고 
                // receive, send EventArgs에서 동일한 token을 참조하도록 구성한다.
                CUserToken token = new CUserToken();

                // receive pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    arg.UserToken = token;

                    // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                    this.buffer_manager.SetBuffer(arg);

                    // add SocketAsyncEventArg to the pool
                    this.receive_event_args_pool.Push(arg);
                }

                // send pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
                    arg.UserToken = token;

                    // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                    this.buffer_manager.SetBuffer(arg);

                    // add SocketAsyncEventArg to the pool
                    this.send_event_args_pool.Push(arg);
                }
            }
        }

        public void listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callback_on_newclient += on_new_client;
            listener.start(host, port, backlog);
        }

        private void on_new_client(Socket client_socket, object token)
        {
            

        }
    }
}
