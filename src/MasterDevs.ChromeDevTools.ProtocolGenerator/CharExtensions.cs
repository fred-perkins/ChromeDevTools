namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public static class CharExtensions
    {
        public static string Repeat(this char character, int times)
        {
            return new string(character, times);
        }
    }
}
