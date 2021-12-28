using System.Net.Http.Json;
using MessageBox.Server.Tcp.Host.Shared;

namespace MessageBox.Server.Tcp.Host.Client.Services.Implementation;

public class MessageStatisticService : IMessageStatisticService
{
    private readonly HttpClient _httpClient;

    public MessageStatisticService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServerMessageCountStatistic> GetServerMessageStatistic()
    {
        return await _httpClient.GetFromJsonAsync<ServerMessageCountStatistic>($"MessageStatistic/message-count") ?? throw new InvalidOperationException();
    }
}