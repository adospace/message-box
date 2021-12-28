namespace MessageBox.Testing
{
    public class InMemoryBusClientOptions : IBusClientOptions
    {
        public string? Name { get; set; }
        
        public TimeSpan DefaultCallTimeout { get ; set; } = TimeSpan.FromSeconds(5);

        public int MaxDegreeOfParallelism { get; set; } = 1;
    }
}
