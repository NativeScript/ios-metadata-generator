using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public abstract class BaseRecordDeclaration : BaseDeclaration
    {
        public string TypedefName { get; set; }

        public string PublicName
        {
            get
            {
                return (this.TypedefName != null) ? this.TypedefName : this.Name;
            }
        }

        public bool IsAnonymous
        {
            get { return char.IsNumber(this.Name[0]); }
        }

        public bool IsOpaque
        {
            get { return !this.Fields.Any(); }
        }

        public IList<FieldDeclaration> Fields { get; private set; }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(this.Fields.SelectMany(f => f.ReferedTypes));
            }
        }

        protected BaseRecordDeclaration(string name)
            : base(name)
        {
            this.Fields = new List<FieldDeclaration>();
        }

#if DEBUG
        protected string ToStringHelper()
        {
            return this.Name +
                   string.Concat(this.Fields.Select(x => Environment.NewLine + "|--" + x));
        }
#endif
    }
}
