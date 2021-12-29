using System.Net.Http.Json;
using MessageBox.Server.Tcp.Host.Shared;

namespace MessageBox.Server.Tcp.Host.Client.Services.Implementation;

internal class BusServerService : IBusServerService
{
    private readonly HttpClient _httpClient;

    public BusServerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<QueueControlModel>> GetQueues()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<QueueControlModel>>($"BusServer/queues") ?? throw new InvalidOperationException();
    }

    public async Task<IEnumerable<ExchangeControModel>> GetExchanges()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<ExchangeControModel>>($"BusServer/exchanges") ?? throw new InvalidOperationException();
    }
}