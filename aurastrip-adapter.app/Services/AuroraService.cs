using System.Net.Sockets;

namespace aurastrip_adapter.Services
{


    public class AuroraService
    {
        private TcpClient _tcpClient;
        private readonly SemaphoreSlim _semaphore;
        private readonly string _host = "127.0.0.1";
        private readonly int _port = 1130;

        public AuroraService()
        {
            this._tcpClient = new TcpClient();
            this._semaphore = new SemaphoreSlim(1, 1);
        }

        private async Task EnsureConnected()
        {
            if (_tcpClient.Connected) return;

            _tcpClient.Dispose();
            _tcpClient = new TcpClient();

            await _tcpClient
                .ConnectAsync(_host, _port)
                .WaitAsync(TimeSpan.FromSeconds(1));
        }

        public async Task<T> ExecuteTransaction<T>(Func<TcpClient, Task<T>> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                await EnsureConnected();
                await _tcpClient.GetStream().FlushAsync();
                return await action(_tcpClient);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ExecuteTransaction(Func<TcpClient, Task> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                await EnsureConnected();
                await action(_tcpClient);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _tcpClient.Dispose();
            _semaphore.Dispose();
        }
    }
}
