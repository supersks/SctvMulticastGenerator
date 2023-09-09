namespace SctvMulticastGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string outputfile = string.Empty;
            if (args.Count() == 1)
            {
                outputfile = args[0];
            }
            else
            {
                outputfile = "/output/iptv.m3u";
            }
            Generator generator = new Generator(outputfile);
            generator.Run().Wait();
        }
    }
}