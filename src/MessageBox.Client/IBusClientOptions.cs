namespace MessageBox
{
    public interface IBusClientOptions
    {
        string? Name { get; }

        TimeSpan DefaultCallTimeout { get; }

        int MaxDegreeOfParallelism { get; }
    }
}
