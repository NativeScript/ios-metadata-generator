namespace TypeScript.Declarations.Writers
{
    using System;

    internal interface IIndentWriter : ITextWriter
    {
        void WriteIndent();

        IDisposable EnterIndentScope();
    }
}
