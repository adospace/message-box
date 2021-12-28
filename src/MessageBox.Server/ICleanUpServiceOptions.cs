namespace MessageBox;

public interface ICleanUpServiceOptions
{
    TimeSpan KeepAliveTimeout { get; }
}