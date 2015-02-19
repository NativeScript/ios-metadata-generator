namespace TypeScript.Declarations
{
    using System.IO;
    using TypeScript.Declarations.Model;
    using W = TypeScript.Declarations.Writers;

    /// <summary>
    /// This class is a facade used as minimalistic public API.
    /// </summary>
    public static class DeclarationWriter
    {
        public static void Write(TextWriter writer, Declaration source, TypeScript.Declarations.Writers.DocumentationProvider docs)
        {
            var sourceUnitWriter = new W.CompositeWriter();

            sourceUnitWriter.TextWriter = writer;
            sourceUnitWriter.Indent = new W.Indent();
            sourceUnitWriter.TypeWriter = new W.TypeWriter(sourceUnitWriter);
            sourceUnitWriter.DeclarationWriter = new W.DeclarationWriter(sourceUnitWriter, docs ?? TypeScript.Declarations.Writers.DocumentationProvider.Undocumented);

            sourceUnitWriter.WriteDeclaration(source);
        }
    }
}
