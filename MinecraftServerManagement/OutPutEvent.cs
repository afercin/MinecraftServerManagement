using System;

namespace MinecraftServerManagement
{
    public class OutPutEvent
    {
        public delegate void OutputEventHandler(OutputEventArgs e);
        public class OutputEventArgs : EventArgs
        {
            public string[] Lines;

            public OutputEventArgs(string[] lines)
                : base()
            {
                Lines = lines;
            }
        }

        public delegate void CommandEventHandler(CommandEventArgs e);
        public class CommandEventArgs : EventArgs
        {
            public string Output;

            public CommandEventArgs(string output)
                : base()
            {
                Output = output;
            }
        }
    }
}
