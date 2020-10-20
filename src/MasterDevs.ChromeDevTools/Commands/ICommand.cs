namespace MasterDevs.ChromeDevTools
{
    public interface ICommand
    {
        long Id { get; }

        string Method { get; }
    }
}