using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkServices
{
    public struct Const<T>
    {
        public T Value { get; }

        public Const(T value) : this()
        {
            this.Value = value;
        }
    }
}
