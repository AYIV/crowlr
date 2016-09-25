namespace crowlr.contracts
{
    public interface INotifier
    {
        bool Notify<T>(T @params);
    }
}
