namespace TypeScript.Declarations.Writers
{
    using TypeScript.Declarations.Model;

    internal interface IDeclarationWriter
    {
        void WriteDeclaration(Declaration declaration);
    }
}
