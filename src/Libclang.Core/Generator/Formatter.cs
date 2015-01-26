using System;
using System.Linq;
using System.IO;

namespace Libclang.Core.Generator
{
    public class Formatter : IFormatter
    {
        private const string IndentationString = "    ";
        private bool isNewLine = false;

        private uint indentationLevel = 0;

        private readonly TextWriter writer;

        public Formatter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Indent()
        {
            indentationLevel++;
        }

        public void Outdent()
        {
            if (!(indentationLevel > 0))
            {
                throw new InvalidOperationException("Indentation is negative");
            }

            indentationLevel--;
        }

        public void Write(string content, params object[] items)
        {
            if (isNewLine)
            {
                WriteIndentation();
            }

            if (items.Length == 0)
            {
                writer.Write(content);
            }
            else
            {
                writer.Write(content, items);
            }

            isNewLine = false;
        }

        public void WriteLine()
        {
            writer.WriteLine();
        }

        public void WriteLine(string format, params object[] items)
        {
            string content = (items.Length == 0) ? format : string.Format(format, items);
            var lines = content.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            if (string.IsNullOrEmpty(lines[lines.Length - 1]))
            {
                lines = lines.Take(lines.Length - 1).ToArray();
            }
            foreach (var line in lines)
            {
                if (isNewLine)
                {
                    WriteIndentation();
                }
                writer.WriteLine(line);
                isNewLine = true;
            }
        }

        private void WriteIndentation()
        {
            for (int i = 0; i < indentationLevel; i++)
            {
                writer.Write(IndentationString);
            }
        }

        public override string ToString()
        {
            return writer.ToString();
        }
    }
}
