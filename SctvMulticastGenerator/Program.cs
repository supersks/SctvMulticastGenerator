namespace SctvMulticastGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Generator generator = new Generator();
            generator.Run().Wait();
        }
    }
}