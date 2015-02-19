namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("interface {Name}")]
    public class InterfaceDeclaration : Declaration, IType
    {
        public InterfaceDeclaration()
        {
            this.Extends = new List<IType>();
            this.Properties = new List<PropertySignature>();
            this.Methods = new List<MethodSignature>();
        }

        public string Name { get; set; }

        public IList<IType> Extends { get; private set; }

        public IList<PropertySignature> Properties { get; private set; }

        public IList<MethodSignature> Methods { get; private set; }

        public override void Accept(DeclarationVisitor visitor)
        {
            if (visitor == null)
            {
                throw new System.ArgumentNullException("visitor");
            }

            visitor.Visit(this);
        }

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
