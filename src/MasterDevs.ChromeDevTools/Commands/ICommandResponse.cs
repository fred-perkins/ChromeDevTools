namespace MasterDevs.ChromeDevTools
{
    public interface ICommandResponse
    {
        long Id { get; }

        string Method { get; }
    }
}