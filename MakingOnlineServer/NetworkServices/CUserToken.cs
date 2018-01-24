namespace NetworkServices
{
    public class CUserToken
    {
        public void on_receive(byte[] buffer, int offset, int transfered)
        {
            this.message_resolver.on_receive(buffer, offset, transfered, on_message);
        }
    }
}