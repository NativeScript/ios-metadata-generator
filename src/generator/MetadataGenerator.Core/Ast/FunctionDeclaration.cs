using System;
using System.Linq;
using System.Collections.Generic;
using MetadataGenerator.Core.Types;

namespace MetadataGenerator.Core.Ast
{
    public class FunctionDeclaration : BaseDeclaration, IFunction
    {
        public TypeDefinition ReturnType { get; set; }

        public IList<ParameterDeclaration> Parameters { get; private set; }

        public bool IsVariadic { get; set; }

        /// <summary>
        /// True if the declaraton is annotated with "ns_returns_retained", false if the declaration is annotated with "ns_returns_not_retained" and null if none of the attributes is set.
        /// More info: http://clang-analyzer.llvm.org/annotations.html#attr_ns_returns_retained
        /// </summary>
        public bool? NsReturnsRetained { get; set; }

        /// <summary>
        /// True if the function retains the returned value, false if the function doesn't retain the returned value and null if it is not specified.
        /// More info: http://clang-analyzer.llvm.org/annotations.html#attr_ns_returns_retained
        /// </summary>
        public virtual bool? OwnsReturnedCocoaObject
        {
            get
            {
                return this.NsReturnsRetained;
            }
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.ReturnType }).Union(this.Parameters.SelectMany(p => p.ReferedTypes));
            }
        }

        public FunctionDeclaration(string name, TypeDefinition returnType) : base(name)
        {
            this.ReturnType = returnType;

            this.Parameters = new List<ParameterDeclaration>();
        }

        public bool HasVaListParameter()
        {
            return this.Parameters.FirstOrDefault(par => par.Type.Resolve() is VaListType) != null;
        }

        protected override bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if (this.IsVariadic || this.HasVaListParameter() || this.IsDefinition)
            {
                return false;
            }

            return null;
        }

        public virtual void AddParameter(ParameterDeclaration parameter)
        {
            // new ParameterDeclaration("__varArgs", new PointerType(new DeclarationReferenceType(new InterfaceDeclaration("NSArray"))))
            this.Parameters.Add(parameter);
        }

#if DEBUG
        protected string ToStringHelper()
        {
            var formattedParameters = this.Parameters.Select(x => x.ToString()).ToList();
            if (this.IsVariadic)
            {
                formattedParameters.Add("...");
            }

            return string.Format("{0} {1}({2})",
                this.ReturnType, this.Name, string.Join(", ", formattedParameters));
        }

        public override string ToString()
        {
            return "FUNCTION_DECLARATION: " + ToStringHelper();
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
