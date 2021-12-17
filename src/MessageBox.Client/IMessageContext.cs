namespace MessageBox
{
    public interface IMessageContext<out T>
    { 
        T Model { get; }
    }


}
