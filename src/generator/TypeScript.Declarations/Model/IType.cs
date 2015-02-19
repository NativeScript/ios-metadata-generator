namespace TypeScript.Declarations.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "This is marking interface for the TS types. We preffer this instead of plain 'objcet' as return type at few places.")]
    public interface IType
    {
        void Accept(TypeVisitor visitor);
    }
}
