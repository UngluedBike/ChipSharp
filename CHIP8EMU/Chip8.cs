using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using CHIP8EMU.Components;
using Silk.NET.SDL;

namespace CHIP8EMU
{
    internal class Chip8
    {
        Stopwatch _stopWatch;
        double _stopWatchOverShoot;
        float frameRate = 700f;
        float InstructionsPerMs = 1000f/700f;
        public readonly Display display;
        ushort ProgramCounter = 0x200;
        ushort IndexRegister;
        byte[] Memory = new byte[4096];
        Stack<ushort> Stack = new Stack<ushort>(2);
        byte DelayTimer;
        byte soundTimer;
        byte[] VariableRegisters = new byte[16];
        Sdl _sdl;
        public Chip8(string filename = "Programs\\IBM Logo.ch8")
        {
            ReadFileIntoMemory("Font\\font.ch8font", 0x050);
            ReadFileIntoMemory(filename, 0x200);
            _stopWatch = Stopwatch.StartNew();
            _sdl = Sdl.GetApi();
            if (_sdl.Init(Sdl.InitVideo) < 0)
            {
                unsafe {
                    Console.WriteLine($"SDL failed to initialize! SDL Error: {(*_sdl.GetError())}");
                }              
            }
            display = new Display(10, _sdl);


            var quit = false;

            while (!quit)
            {
                
                Event e;
                unsafe
                {
                    while (_sdl.PollEvent(&e) > 0)
                    {
                        if (e.Type == (uint)EventType.Quit)
                        {
                            quit = true;
                        }

                        
                    }
                }

                var timespan = _stopWatch.Elapsed;
                if (timespan.TotalMilliseconds >= (16 + _stopWatchOverShoot))
                {
                    _stopWatchOverShoot = 16 - timespan.TotalMilliseconds;
                    if (_stopWatchOverShoot > 0)
                        _stopWatchOverShoot = 0;

                    _stopWatch.Restart();


                    for (int i = 0; i < 12 ; i++)
                    {
                        RunCpuCycle();
                    }                    
                }
                
            }

            _sdl.Quit();
        }


        private void ReadFileIntoMemory(string filename, ushort address)
        {
            using (var stream = File.OpenRead(filename))
            {
                // everything up to and including 1FF contains the interpreter in old machines.
                stream.Read(Memory, address, (int)stream.Length);
            }
        }

        private void RunCpuCycle()
        {
            // Fetch
            ushort instruction = Memory[ProgramCounter];
            instruction = (ushort)(instruction << 8);
            instruction |= Memory[ProgramCounter + 1];

            Console.WriteLine(instruction.ToString("X4"));

            ProgramCounter += 2;

            //Decode
            // These are nibbles but byte is the smallest size c# does.
            byte firstNumber = (byte)((0xF000 & instruction) >> 12);
            byte X = (byte)((0x0F00 & instruction) >> 8);
            byte Y = (byte)((0x00F0 & instruction) >> 4);
            byte N = (byte)(0x000F & instruction);

            byte NN = (byte)(0x00FF & instruction);
            ushort NNN = (ushort)(0x0FFF & instruction);

            switch (firstNumber) 
            { 
                case 0:
                    display.Clear();
                    break;
                case 1:
                    ProgramCounter = NNN;
                    break;
                case 6:
                    VariableRegisters[X] = NN;
                    break;
                case 7:
                    VariableRegisters[X] += NN;
                    break;
                case 0xA:
                    IndexRegister = NNN;
                    break;
                case 0xD:
                    DXYN(X, Y, N);
                    break;

            }

        }

        void DXYN(byte X, byte Y, byte N)
        {
            byte VX = (byte)(VariableRegisters[X] % Constants.DisplayWidth);
            byte VY = (byte)(VariableRegisters[Y] % Constants.DisplayHeight);
            VariableRegisters[0xF] = 0;
            bool collision = false;

            for (int i = 0; i < N; i++)
            {
                byte spriteByte = Memory[IndexRegister + i];
                int j = 7;
                while (j >= 0) 
                { 
                    var bit = (spriteByte & (1 << j)) !=0;
                    if (bit)
                    {
                        collision |= display.TogglePixel(VX, VY);                                          
                    }
                    VX++;
                    j--;                 
                }
                VX -= 8;
                VY++;
            }
            if (collision)
            {
                VariableRegisters[0xF] = 1;
            }
        }

    }
}
