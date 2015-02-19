using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Meta.Visitors
{
    public class ExtendedEncodingVisitor : IDeclarationVisitor
    {
        public void Visit(InterfaceDeclaration declaration)
        {
        }

        public void Visit(ProtocolDeclaration declaration)
        {
        }

        public void Visit(CategoryDeclaration declaration)
        {
        }

        private void VisitBaseRecord(BaseRecordDeclaration declaration)
        {
            declaration.SetExtendedEncoding(declaration.Fields.Select(f => f.Type.ToTypeEncoding()));
        }

        public void Visit(StructDeclaration declaration)
        {
            VisitBaseRecord(declaration);
        }

        public void Visit(UnionDeclaration declaration)
        {
            VisitBaseRecord(declaration);
        }

        public void Visit(FieldDeclaration declaration)
        {
            TypeEncoding extendedEncoding = declaration.Type.ToTypeEncoding();
            declaration.SetExtendedEncoding(extendedEncoding);
        }

        public void Visit(EnumDeclaration declaration)
        {
        }

        public void Visit(EnumMemberDeclaration declaration)
        {
        }

        private void VisitFunction(FunctionDeclaration declaration)
        {
            List<TypeEncoding> extendedEncoding = new List<TypeEncoding>();
            extendedEncoding.Add(declaration.GetReturnTypeEncoding());
            extendedEncoding.AddRange(declaration.Parameters.Select(p => p.Type.ToTypeEncoding()));
            declaration.SetExtendedEncoding(extendedEncoding);
        }

        public void Visit(FunctionDeclaration declaration)
        {
            VisitFunction(declaration);
        }

        public void Visit(MethodDeclaration declaration)
        {
            VisitFunction(declaration);
        }

        public void Visit(ParameterDeclaration declaration)
        {
        }

        public void Visit(PropertyDeclaration declaration)
        {
            TypeEncoding extendedEncoding;
            if (declaration.Getter != null)
            {
                extendedEncoding = declaration.Getter.GetReturnTypeEncoding();
            }
            else
            {
                if (!declaration.Setter.GetReturnTypeEncoding().IsVoid())
                {
                    throw new Exception("Invalid property setter. The setter should return void.");
                }

                if (declaration.Setter.Parameters.Count() != 1)
                {
                    throw new Exception("Invalid property setter. The setter should have only one parameter.");
                }

                extendedEncoding = declaration.Setter.Parameters.First().Type.ToTypeEncoding();
            }
            declaration.SetExtendedEncoding(extendedEncoding);
        }

        public void Visit(ModuleDeclaration declaration)
        {
        }

        public void Visit(VarDeclaration declaration)
        {
            TypeEncoding extendedEncoding = declaration.Type.ToTypeEncoding();
            declaration.SetExtendedEncoding(extendedEncoding);
        }

        public void Visit(TypedefDeclaration declaration)
        {
        }
    }
}
