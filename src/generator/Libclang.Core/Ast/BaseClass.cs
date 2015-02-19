using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public abstract class BaseClass : BaseDeclaration, IProtocolImplementer
    {
        public ICollection<ProtocolDeclaration> ImplementedProtocols { get; private set; }

        public ICollection<PropertyDeclaration> Properties { get; private set; }

        public ICollection<MethodDeclaration> Methods { get; private set; }

        public IEnumerable<MethodDeclaration> Constructors
        {
            get { return this.Methods.Where(x => x.IsConstructor); }
        }

        protected BaseClass(string name)
            : base(name)
        {
            this.ImplementedProtocols = new List<ProtocolDeclaration>();
            this.Properties = new List<PropertyDeclaration>();
            this.Methods = new List<MethodDeclaration>();
        }

#if DEBUG
        protected string ToStringHelper()
        {
            return (ImplementedProtocols.Any()
                ? string.Format(" <{0}>", string.Join(", ", ImplementedProtocols.Select(x => x.Name)))
                : "") +
                   string.Concat(this.Properties.Select(x => Environment.NewLine + "|--" + x)) +
                   string.Concat(this.Methods.Select(x => Environment.NewLine + "|--" + x));
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            foreach (PropertyDeclaration property in this.Properties)
            {
                property.Accept(visitor);
            }
            foreach (MethodDeclaration method in this.Methods)
            {
                method.Accept(visitor);
            }
        }
    }
}
