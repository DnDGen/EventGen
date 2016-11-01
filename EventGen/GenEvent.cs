using System;

namespace EventGen
{
    public class GenEvent
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public DateTime When { get; set; }

        public GenEvent()
            : this(string.Empty, string.Empty)
        {
        }

        public GenEvent(string source, string message)
        {
            Source = source;
            Message = message;
            When = DateTime.Now;
        }
    }
}
