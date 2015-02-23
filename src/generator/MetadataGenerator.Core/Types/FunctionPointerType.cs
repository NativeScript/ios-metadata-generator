using System;
using System.Collections.Generic;
using System.Linq;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Generator;

namespace MetadataGenerator.Core.Types
{
    public class FunctionPointerType : TypeDefinition, IFunction
    {
        public ulong Id { get; set; }

        public TypeDefinition ReturnType { get; set; }

        public IList<ParameterDeclaration> Parameters { get; private set; }

        public bool IsBlock { get; set; }

        public bool IsVariadic { get; set; }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                foreach (TypeDefinition type in base.ReferedTypes)
                {
                    yield return type;
                }

                yield return this.ReturnType;

                foreach (var param in Parameters)
                {
                    yield return param.Type;
                }
            }
        }

        public FunctionPointerType(TypeDefinition returnType, ulong id)
        {
            this.ReturnType = returnType;
            this.Id = id;

            this.Parameters = new List<ParameterDeclaration>();
        }

        public void AddParameter(ParameterDeclaration parameter)
        {
            this.Parameters.Add(parameter);
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            List<string> formattedParameters = this.Parameters.Select(x => x.Type.ToString()).ToList();
            if (this.IsVariadic)
            {
                formattedParameters.Add("...");
            }

            string typeString = string.Format("({0}{1})({2})", (this.IsBlock ? "^" : "*"), identifier,
                string.Join(", ", formattedParameters));
            return this.ReturnType.ToStringInternal(typeString);
        }

        public override TypeEncoding ToTypeEncoding()
        {
            var returnTypeEncoding = this.ReturnType.ToTypeEncoding();
            var parameterTypeEncodings = this.Parameters.Select(p => p.Type.ToTypeEncoding());
            if (this.IsBlock)
            {
                return TypeEncoding.Block(returnTypeEncoding, parameterTypeEncodings);
            }
            else
            {
                return TypeEncoding.Function(returnTypeEncoding, parameterTypeEncodings);
            }
        }
    }
}
