using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Utils;
using System;

namespace MetadataGenerator.Core.Meta.Visitors
{
    public interface IDeclarationVisitor
    {
        void Visit(InterfaceDeclaration declaration);

        void Visit(ProtocolDeclaration declaration);

        void Visit(CategoryDeclaration declaration);

        void Visit(StructDeclaration declaration);

        void Visit(UnionDeclaration declaration);

        void Visit(FieldDeclaration declaration);

        void Visit(EnumDeclaration declaration);

        void Visit(EnumMemberDeclaration declaration);

        void Visit(FunctionDeclaration declaration);

        void Visit(MethodDeclaration declaration);

        void Visit(ParameterDeclaration declaration);

        void Visit(PropertyDeclaration declaration);

        void Visit(ModuleDeclaration declaration);

        void Visit(VarDeclaration declaration);

        void Visit(TypedefDeclaration declaration);
    }

    public interface IMetaContainerDeclarationVisitor : IDeclarationVisitor
    {
        void Begin(ModuleDeclarationsContainer metaContainer);

        void End(ModuleDeclarationsContainer metaContainer);
    }
}
