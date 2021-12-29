namespace MessageBox.Server.Tcp.Host.Shared;

public record QueueControlModel(Guid Id, string? Name);


public record ExchangeControModel(string Key, QueueControlModel[] Subscribers);