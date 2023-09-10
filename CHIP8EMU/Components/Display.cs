using Silk.NET.Core.Contexts;
using Silk.NET.SDL;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIP8EMU.Components
{
    
    unsafe internal class Display
    {
        int _scale;
        int _width;
        int _height;
        bool[,] pixels = new bool[Constants.DisplayWidth,Constants.DisplayHeight];
        readonly Window* window;
        readonly Surface* _surface;
        readonly Sdl _sdl;
        public Display(int scale, Sdl sdl)
        {
            _scale = scale;
            _width = Constants.DisplayWidth * scale;
            _height = Constants.DisplayHeight * scale;
            _sdl = Sdl.GetApi();
            
            window = _sdl.CreateWindow("Chip 8 Instance",Sdl.WindowposUndefined,Sdl.WindowposUndefined, _width, _height, (uint)WindowFlags.Shown);
            _surface = _sdl.GetWindowSurface(window);

            var rectangle = Rectangle.FromLTRB(0, 0, Constants.DisplayWidth * _scale, Constants.DisplayHeight * _scale);
            _sdl.FillRect(_surface, rectangle, 0xFFFFFF);
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            _sdl.UpdateWindowSurface(window);
        }

        public void Clear()
        {
            var rectangle = Rectangle.FromLTRB(0, 0, Constants.DisplayWidth*_scale, Constants.DisplayHeight*_scale);
            _sdl.FillRect(_surface, rectangle, 0x000000);
            Array.Clear(pixels);
            UpdateDisplay();
        }

        /// <returns>true if a pixel was flipped to 0.</returns>
        public bool TogglePixel(int x, int y)
        {
            Console.WriteLine($"Drawing pixel at {x}, {y}");
            if (x >= Constants.DisplayWidth || y >= Constants.DisplayHeight)
            {
                return false;
            }

            // Set screen state
            var pixelState = pixels[x,y];
            pixels[x,y] = pixels[x, y] ^ true;

            var rectangle = Rectangle.FromLTRB(x * _scale, y * _scale, (x + 1) * _scale, (y + 1) * _scale);
            _sdl.FillRect(_surface, rectangle, 0xFFFFFF);

            UpdateDisplay();

            return !pixelState;
        }
        // Investigate a more direct way of doing this.
        void DrawPixel(Surface* surface, int x, int y, int scale, uint color)
        {
            var rectangle = Rectangle.FromLTRB(x*scale, y*scale, (x+1)*scale, (y+1)*scale);
            _sdl.FillRect(surface, rectangle, color);
        }

        ~Display()
        {
            _sdl.DestroyWindow(window);
        }
    }
}
