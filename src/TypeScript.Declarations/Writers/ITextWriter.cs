namespace TypeScript.Declarations.Writers
{
    internal interface ITextWriter
    {
        void Write(string text);

        void WriteLine();

        void WriteLine(string text);
    }
}
