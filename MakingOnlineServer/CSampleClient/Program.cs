using NetworkServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CPacketBufferManager.initialize(2000);
            // CNetworkService객체는 메시지의 비동기 송,수신 처리를 수행한다.
            // 메시지 송,수신은 서버, 클라이언트 모두 동일한 로직으로 처리될 수 있으므로
            // CNetworkService객체를 생성하여 Connector객체에 넘겨준다.
            CNetworkService service = new CNetworkService();

            // endpoint정보를 갖고있는 Connector생성. 만들어둔 NetworkService객체를 넣어준다.
            CConnector connector = new CConnector();

        }
    }
}
