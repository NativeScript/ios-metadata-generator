namespace TypeScript.Declarations.Model
{
    using System.Diagnostics;

    [DebuggerDisplay("{Type}[]")]
    public class ArrayType : TypeScriptObject, IType
    {
        public IType ComponentType { get; set; }

        public void Accept(TypeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new System.ArgumentNullException("visitor");
            }

            visitor.Visit(this);
        }
    }
}
