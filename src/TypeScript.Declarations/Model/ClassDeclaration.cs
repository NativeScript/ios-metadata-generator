namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("class {Name}")]
    public class ClassDeclaration : Declaration, IType
    {
        public ClassDeclaration()
        {
            this.Implements = new List<IType>();
            this.Properties = new List<PropertySignature>();
            this.Constructors = new List<ConstructSignature>();
            this.Methods = new List<MethodSignature>();
        }

        public string Name { get; set; }

        public IType Extends { get; set; }

        public IList<IType> Implements { get; private set; }

        public IList<ConstructSignature> Constructors { get; private set; }

        public IList<PropertySignature> Properties { get; private set; }

        public IList<MethodSignature> Methods { get; private set; }

        public TypeScriptObject GetInstanceMember(string name)
        {
            var method = this.Methods.FirstOrDefault(m => m.Name == name && !m.IsStatic);
            if (method != null)
            {
                return method;
            }

            var property = this.Properties.FirstOrDefault(p => p.Name == name && !p.IsStatic);
            if (property != null)
            {
                return property;
            }

            return null;
        }

        public bool HasInstanceMember(string name)
        {
            return this.GetInstanceMember(name) != null;
        }

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
