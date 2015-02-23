using System;
using System.Collections.Generic;
using System.Linq;
using MetadataGenerator.Core.Types;

namespace MetadataGenerator.Core.Ast
{
    public class EnumDeclaration : BaseDeclaration
    {
        public override string FullName
        {
            get { return "enum " + base.FullName; }
        }

        public TypeDefinition UnderlyingType { get; set; }

        public IList<EnumMemberDeclaration> Fields { get; private set; }

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

        public EnumDeclaration(string name, TypeDefinition underlyingType)
            : base(name)
        {
            this.UnderlyingType = underlyingType;

            this.Fields = new List<EnumMemberDeclaration>();
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.UnderlyingType });
            }
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("ENUM_DECLARATION: {0} : {1}", this.Name, this.UnderlyingType) +
                   string.Concat(this.Fields.Select(x => Environment.NewLine + "|--" + x));
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            foreach (EnumMemberDeclaration enumField in this.Fields)
            {
                enumField.Accept(visitor);
            }
            visitor.Visit(this);
        }
    }
}
