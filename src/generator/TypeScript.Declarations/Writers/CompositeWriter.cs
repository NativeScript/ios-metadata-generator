namespace TypeScript.Declarations.Writers
{
    using System;
    using System.IO;
    using TypeScript.Declarations.Model;

    internal class CompositeWriter : ISourceUnitWriter, ITextWriter, IIndentWriter, ITypeWriter, IDeclarationWriter
    {
        public ITypeWriter TypeWriter { get; set; }

        public IDeclarationWriter DeclarationWriter { get; set; }

        public TextWriter TextWriter { get; set; }

        public Indent Indent { get; set; }

        public void Write(string text)
        {
            this.TextWriter.Write(text);
        }

        public void WriteLine()
        {
            this.TextWriter.WriteLine();
        }

        public void WriteLine(string text)
        {
            this.TextWriter.WriteLine(text);
        }

        public void WriteIndent()
        {
            this.TextWriter.Write(this.Indent);
        }

        public IDisposable EnterIndentScope()
        {
            return this.Indent.EnterScope();
        }

        public void WriteType(IType type)
        {
            this.TypeWriter.WriteType(type);
        }

        public void WriteDeclaration(Declaration declaration)
        {
            this.DeclarationWriter.WriteDeclaration(declaration);
        }
    }
}
