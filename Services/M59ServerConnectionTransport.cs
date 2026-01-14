using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Meridian59EventManager.Core.Transports;

namespace M59AdminTool.Services
{
    public class M59ServerConnectionTransport : IAdminCommandTransport
    {
        private readonly ConcurrentQueue<string> _responseBuffer = new();
        private readonly object _sync = new();
        private M59ServerConnection? _connection;

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsConnected => _connection?.IsConnected == true;

        public void AttachConnection(M59ServerConnection? connection)
        {
            lock (_sync)
            {
                if (_connection != null)
                {
                    _connection.ResponseReceived -= Server_ResponseReceived;
                }

                _connection = connection;

                if (_connection != null)
                {
                    _connection.ResponseReceived += Server_ResponseReceived;
                    MessageReceived?.Invoke(this, "Shared admin connection attached");
                }
                else
                {
                    _responseBuffer.Clear();
                }
            }
        }

        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(IsConnected);
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<string> SendCommandAsync(string command)
        {
            if (_connection == null || !IsConnected)
            {
                var message = "Shared server connection is not ready.";
                ErrorOccurred?.Invoke(this, message);
                throw new InvalidOperationException(message);
            }

            ClearBuffer();

            bool sent = await _connection.SendAdminCommandAsync(command);
            if (!sent)
            {
                var message = $"Failed to send command: {command}";
                ErrorOccurred?.Invoke(this, message);
                throw new InvalidOperationException(message);
            }

            var response = await CollectResponsesAsync();
            return response;
        }

        private void Server_ResponseReceived(object? sender, string response)
        {
            _responseBuffer.Enqueue(response);
            MessageReceived?.Invoke(this, response);
        }

        private async Task<string> CollectResponsesAsync()
        {
            var responses = new List<string>();
            var timeout = TimeSpan.FromSeconds(3);
            var idle = TimeSpan.FromMilliseconds(200);
            var stopwatch = Stopwatch.StartNew();
            DateTime lastReceived = DateTime.UtcNow;

            while (stopwatch.Elapsed < timeout)
            {
                while (_responseBuffer.TryDequeue(out var line))
                {
                    responses.Add(line);
                    lastReceived = DateTime.UtcNow;
                }

                if (responses.Count > 0 && DateTime.UtcNow - lastReceived > idle)
                {
                    break;
                }

                await Task.Delay(50);
            }

            return string.Join("\n", responses);
        }

        private void ClearBuffer()
        {
            while (_responseBuffer.TryDequeue(out _))
            {
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_connection != null)
                {
                    _connection.ResponseReceived -= Server_ResponseReceived;
                }
                _connection = null;
                ClearBuffer();
            }
        }
    }
}
