namespace MessageBox
{
    public interface IBusClientOptions
    {
        TimeSpan DefaultCallTimeout { get; set; }

        int MaxDegreeOfParallelism { get; set; }
    }
}
