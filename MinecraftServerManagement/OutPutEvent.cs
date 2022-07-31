using System;

namespace MinecraftServerManagement
{
    public class OutPutEvent
    {
        public delegate void OutputEventHandler(OutputEventArgs e);
        public class OutputEventArgs : EventArgs
        {
            public string Output = "";

            public OutputEventArgs(string output)
                : base()
            {
                Output = output;
            }
        }

        public delegate void StatusEventHandler(StatusEventArgs e);
        public class StatusEventArgs : EventArgs
        {
            public StatusEventArgs()
                : base()
            {
            }
        }
    }
}
