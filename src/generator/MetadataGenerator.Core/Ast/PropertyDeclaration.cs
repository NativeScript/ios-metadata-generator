using System;
using System.Linq;
using System.Collections.Generic;
using MetadataGenerator.Core.Types;

namespace MetadataGenerator.Core.Ast
{
    public class PropertyDeclaration : BaseDeclaration
    {
        public override string FullName
        {
            get { return string.Format("{0}.{1}", this.Parent.FullName, this.Name); }
        }

        public BaseClass Parent { get; set; }

        public TypeDefinition Type { get; set; }

        public MethodDeclaration Getter { get; set; }
        public MethodDeclaration Setter { get; set; }

        public override ModuleDeclaration Module
        {
            get
            {
                return this.Parent.Module;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool IsReadonly { get; set; }
        public bool HasCustomGetter { get; set; }
        public bool IsAssign { get; set; }
        public bool IsReadwrite { get; set; }
        public bool IsRetain { get; set; }
        public bool IsCopy { get; set; }
        public bool IsNonatomic { get; set; }
        public bool HasCustomSetter { get; set; }
        public bool IsAtomic { get; set; }
        public bool IsWeak { get; set; }
        public bool IsStrong { get; set; }
        public bool IsUnsafeUnretained { get; set; }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.Type });
            }
        }

        /// <summary>
        /// When the property is in a protocol declaration
        /// </summary>
        public bool IsOptional { get; set; }

        public PropertyDeclaration(BaseClass parent, string name, TypeDefinition type)
            : base(name)
        {
            this.Parent = parent;
            this.Type = type;
        }

#if DEBUG
        public override string ToString()
        {
            var attributes = new List<string>();

            if (this.IsReadonly) attributes.Add("readonly");
            if (this.HasCustomGetter) attributes.Add("getter=" + Getter.Name);
            if (this.IsAssign) attributes.Add("assign");
            if (this.IsReadwrite) attributes.Add("readwrite");
            if (this.IsRetain) attributes.Add("retain");
            if (this.IsCopy) attributes.Add("copy");
            if (this.IsNonatomic) attributes.Add("nonatomic");
            if (this.HasCustomSetter) attributes.Add("setter=" + Setter.Name);
            if (this.IsAtomic) attributes.Add("atomic");
            if (this.IsWeak) attributes.Add("weak");
            if (this.IsStrong) attributes.Add("strong");
            if (this.IsUnsafeUnretained) attributes.Add("unsafe_unretained");

            return string.Format("@property {0}{1} {2};",
                (attributes.Any() ? string.Format("({0}) ", string.Join(", ", attributes)) : ""),
                this.Type, this.Name);
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
