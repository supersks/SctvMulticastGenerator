namespace SctvMulticastGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                throw new ArgumentException("args is not correct! must be output file full path!!");
            }
            string outputfile = args[0];
            Generator generator = new Generator(outputfile);
            generator.Run().Wait();
        }
    }
}