using System;

namespace Libclang.Core.Generator
{
    public interface IFormatter
    {
        void Indent();

        void Outdent();

        void Write(string format, params object[] items);

        void WriteLine();

        void WriteLine(string format, params object[] items);
    }
}
