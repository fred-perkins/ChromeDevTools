namespace MasterDevs.ChromeDevTools
{
    public class CommandResponse : ICommandResponse
    {
        public long Id
        {
            get;
            set;
        }

        public string Method
        {
            get;
            set;
        }
    }

    public class CommandResponse<T> : CommandResponse
    {
        public CommandResponse()
        {
        }

        public CommandResponse(T result)
        {
            this.Result = result;
        }

        public T Result
        {
            get;
            private set;
        }
    }
}