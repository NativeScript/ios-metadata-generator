namespace TypeScript.Declarations.Writers
{
    using TypeScript.Declarations.Model;

    internal interface ITypeWriter
    {
        void WriteType(IType type);
    }
}
