using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IBoard : IMessageSink
    {
        string Key { get; }

        void Subscribe(IBox box);

        void Start();

        void Stop();
    }
}
