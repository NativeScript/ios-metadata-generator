using System;
using System.Linq;
using System.Collections.Generic;
using MetadataGenerator.Core.Types;
using System.Text;

namespace MetadataGenerator.Core.Ast
{
    public class MethodDeclaration : FunctionDeclaration
    {
        public override string FullName
        {
            get { return string.Format("{0}.{1}{2}", this.Parent.FullName, (this.IsStatic ? "+" : "-"), this.Name); }
        }

        public string Selector
        {
            get
            {
                return this.Name;
            }
        }

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

        public string TypeEncoding { get; private set; }

        public BaseClass Parent { get; set; }

        /// <summary>
        /// Base Interface or Category
        /// </summary>
        public ICollection<IDeclaration> Overrides { get; private set; }

        public bool IsStatic { get; set; }

        public bool IsNilTerminatedVariadic { get; set; }

        /// <summary>
        /// Whether the declaration exists in code or was created implicitly
        /// by the compiler, e.g. implicit objc methods for properties.
        /// </summary>
        public bool IsImplicit { get; set; }

        /// <summary>
        /// When the method is in a protocol declaration
        /// </summary>
        public bool IsOptional { get; set; }

        public bool IsConstructor
        {
            get
            {
                if (this.IsStatic)
                {
                    return false;
                }

                const string Init = "init";
                if (!this.Name.StartsWith(Init))
                {
                    return false;
                }

                if (!(this.Name == Init || char.IsUpper(this.Name[Init.Length])))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Conform to this convention http://clang.llvm.org/docs/LanguageExtensions.html#related-result-types
        /// </summary>
        public bool HasRelatedResultType
        {
            get
            {
                if (this.ReturnType is InstanceType)
                {
                    return true;
                }

                StringBuilder firstWord = new StringBuilder();
                for (int i = 0; i < this.Name.Length; i++)
                {
                    if (Char.IsUpper(this.Name[i]))
                    {
                        break;
                    }
                    firstWord.Append(this.Name[i]);
                }

                switch (firstWord.ToString())
                {
                    case "init":
                    case "autorelease":
                    case "retain":
                        return this.Name != "retainCount";
                    case "self":
                        return !this.IsStatic;
                    case "alloc":
                    case "new":
                        return this.IsStatic;
                    default:
                        return false;
                }
            }
        }

        public override bool? OwnsReturnedCocoaObject
        {
            get
            {
                bool? baseValue = base.OwnsReturnedCocoaObject;
                return baseValue.HasValue ?
                    baseValue.Value :
                    (this.Name.StartsWith("alloc") || this.Name.StartsWith("new") || this.Name.StartsWith("copy") || this.Name.StartsWith("mutableCopy"));
            }
        }

        protected override bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if ((this.IsVariadic && !this.IsNilTerminatedVariadic) || this.HasVaListParameter())
            {
                return false;
            }

            if (this.Name == "copy:" && this.Parent.Name == "UIResponderStandardEditActions")
            {
                return false;
            }

            return null;
        }

        public MethodDeclaration(BaseClass parent, string name, TypeDefinition returnType, string typeEncoding)
            : base(name, returnType)
        {
            this.Parent = parent;
            this.TypeEncoding = typeEncoding;
            this.Overrides = new List<IDeclaration>();
        }

#if DEBUG
        public override string ToString()
        {
            var formattedParameters = this.Parameters.Select(x => x.ToString()).ToList();
            if (this.IsVariadic)
            {
                formattedParameters.Add("...");
            }

            return "METHOD_DECLARATION: " + (this.IsStatic ? "+" : "-") + " " +
                   ToStringHelper();
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            foreach (ParameterDeclaration parameter in this.Parameters)
            {
                parameter.Accept(visitor);
            }
            visitor.Visit(this);
        }
    }
}
