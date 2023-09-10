namespace CHIP8EMU
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Chip8 chip8;
            if (args.Length > 0)
            {
                chip8 = new Chip8(args[0]);
            }
            chip8 = new Chip8();
            
        }
    }
}