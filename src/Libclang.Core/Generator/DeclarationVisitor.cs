﻿using System;
using System.Linq;
using Libclang.Core.Ast;

namespace Libclang.Core.Generator
{
    public abstract class DeclarationVisitor
    {
        protected DocumentDeclaration documentDeclaration;

        protected DeclarationVisitor(DocumentDeclaration documentDeclaration)
        {
            this.documentDeclaration = documentDeclaration;
        }

        public virtual void Visit(IDeclaration node)
        {
            if (node is DocumentDeclaration)
            {
                VisitDocumentDeclaration(node as DocumentDeclaration);
            }
            else if (node is TypedefDeclaration)
            {
                VisitTypedefDeclaration(node as TypedefDeclaration);
            }
            else if (node is VarDeclaration)
            {
                VisitVarDeclaration(node as VarDeclaration);
            }
            else if (node is StructDeclaration)
            {
                VisitStructDeclaration(node as StructDeclaration);
            }
            else if (node is UnionDeclaration)
            {
                VisitUnionDeclaration(node as UnionDeclaration);
            }
            else if (node is EnumDeclaration)
            {
                VisitEnumDeclaration(node as EnumDeclaration);
            }
            else if (node is FunctionDeclaration)
            {
                VisitFunctionDeclaration(node as FunctionDeclaration);
            }
            else if (node is InterfaceDeclaration)
            {
                VisitInterfaceDeclaration(node as InterfaceDeclaration);
            }
            else if (node is ProtocolDeclaration)
            {
                VisitProtocolDeclaration(node as ProtocolDeclaration);
            }
            else if (node is CategoryDeclaration)
            {
                VisitCategoryDeclaration(node as CategoryDeclaration);
            }
            else if (node is PropertyDeclaration)
            {
                VisitPropertyDeclaration(node as PropertyDeclaration);
            }
            else if (node is MethodDeclaration)
            {
                VisitMethodDeclaration(node as MethodDeclaration);
            }
            else
            {
                throw new ArgumentException("Unknown type: " + node.GetType().Name);
            }
        }

        public virtual void VisitAll()
        {
            Visit(this.documentDeclaration);
        }

        protected virtual void VisitDocumentDeclaration(DocumentDeclaration documentDeclaration)
        {
            foreach (var declaration in documentDeclaration.Declarations)
            {
                Visit(declaration);
            }
        }

        protected virtual void VisitTypedefDeclaration(TypedefDeclaration typedefDeclaration)
        {
        }

        protected virtual void VisitVarDeclaration(VarDeclaration varDeclaration)
        {
        }

        protected virtual void VisitStructDeclaration(StructDeclaration structDeclaration)
        {
        }

        protected virtual void VisitUnionDeclaration(UnionDeclaration unionDeclaration)
        {
        }

        protected virtual void VisitEnumDeclaration(EnumDeclaration enumDeclaration)
        {
        }

        protected virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
        }

        protected virtual void VisitInterfaceDeclaration(InterfaceDeclaration classDeclaration)
        {
            VisitBaseClassDeclaration(classDeclaration);
        }

        protected virtual void VisitProtocolDeclaration(ProtocolDeclaration protocolDeclaration)
        {
            VisitBaseClassDeclaration(protocolDeclaration);
        }

        protected virtual void VisitCategoryDeclaration(CategoryDeclaration categoryDeclaration)
        {
            VisitBaseClassDeclaration(categoryDeclaration);
        }

        protected virtual void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        {
        }

        protected virtual void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
        }

        private void VisitBaseClassDeclaration(BaseClass baseClass)
        {
            foreach (var propertyDeclaration in baseClass.Properties)
            {
                Visit(propertyDeclaration);
            }

            foreach (var methodDeclaration in baseClass.Methods)
            {
                Visit(methodDeclaration);
            }
        }
    }
}
