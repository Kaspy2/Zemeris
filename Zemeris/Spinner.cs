using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zemeris
{
    public class Spinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public Spinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        public void Start()
        {
            active = true;
            if (!thread.IsAlive)
                thread.Start();
            Console.CursorVisible = false;
        }

        public void Stop()
        {
            active = false;
            Draw(' ');
            Console.SetCursorPosition(Console.CursorLeft-1, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorVisible = true;
        }

        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        private void Draw(char c)
        {
            Random r = new Random();
            Console.SetCursorPosition(left, top);
            //Console.ForegroundColor = ConsoleColor.Green;
            Console.ForegroundColor = (ConsoleColor)r.Next(0,16);
            Console.Write(c);
        }

        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
