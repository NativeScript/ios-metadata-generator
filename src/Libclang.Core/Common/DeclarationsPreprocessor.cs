using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Types;

namespace Libclang.Core.Common
{
    public class DeclarationsPreprocessor
    {
        private class CustomDocumentResolver : Libclang.Core.Parser.IDocumentResolver
        {
            public CustomDocumentResolver(IList<DocumentDeclaration> documents)
            {
                this.documents = documents;
            }

            private IList<DocumentDeclaration> documents;

            public DocumentDeclaration GetDocumentForDeclaration(IDeclaration declaration)
            {
                System.Diagnostics.Debug.Assert(declaration is BaseDeclaration,
                    "Unexpected declaration type " + declaration);
                DocumentDeclaration document = (declaration as BaseDeclaration).Document;
                System.Diagnostics.Debug.Assert(document != null,
                    string.Format("Declaration {0} has no associated document", declaration.Name));

                DocumentDeclaration newDocument = this.documents.SingleOrDefault(c => c.Name == document.Name);
                if (newDocument == null)
                {
                    newDocument = new DocumentDeclaration(document.Name);
                    this.documents.Add(newDocument);
                }
                return newDocument;
            }
        }

        public IEnumerable<DocumentDeclaration> Process(IEnumerable<DocumentDeclaration> documents)
        {
            IEnumerable<DocumentDeclaration> finalDocuments;

            FixParameterNameCollisions(documents);
            RemoveAnonymousMembers(documents);
            finalDocuments = FixMissingReferences(documents);

            return finalDocuments;
        }

        private static void FixParameterNameCollisions(IEnumerable<DocumentDeclaration> documents)
        {
            foreach (DocumentDeclaration document in documents)
            {
                foreach (InterfaceDeclaration @interface in document.Interfaces)
                {
                    foreach (MethodDeclaration method in @interface.Methods)
                    {
                        FixParameterNameCollision(method);
                    }
                }

                foreach (ProtocolDeclaration protocol in document.Protocols)
                {
                    foreach (MethodDeclaration method in protocol.Methods)
                    {
                        FixParameterNameCollision(method);
                    }
                }
            }
        }

        private static void RemoveAnonymousMembers(IEnumerable<DocumentDeclaration> documents)
        {
            foreach (DocumentDeclaration document in documents)
            {
                var anonymousRecords = document.Declarations
                    .OfType<BaseRecordDeclaration>()
                    .Where(c => c.IsAnonymousWithoutTypedef())
                    .ToArray();

                foreach (IDeclaration @record in anonymousRecords)
                {
                    document.Declarations.Remove(@record);
                }
            }
        }

        private static void FixParameterNameCollision(MethodDeclaration method)
        {
            if (method.Parameters.Count == 2 && method.Parameters[0].Name == method.Parameters[1].Name)
            {
                method.Parameters[1].Name += 1;
            }
        }

        private static IEnumerable<DocumentDeclaration> FixMissingReferences(IEnumerable<DocumentDeclaration> documents)
        {
            HashSet<IDeclaration> allDeclarations = new HashSet<IDeclaration>(documents.SelectMany(c => c.Declarations));
            List<DocumentDeclaration> newDocuments = new List<DocumentDeclaration>();
            Parser.IDocumentResolver resolver = new CustomDocumentResolver(newDocuments);

            foreach (DocumentDeclaration document in documents)
            {
                foreach (BaseClass declaration in document.Declarations.OfType<BaseClass>())
                {
                    BaseClass @class = declaration as BaseClass;
                    foreach (ProtocolDeclaration protocol in @class.ImplementedProtocols)
                    {
                        AddIfDeclarationNotFound(protocol, allDeclarations, resolver);
                    }
                    foreach (MethodDeclaration @method in @class.Methods)
                    {
                        AddIfTypeNotFound(@method.ReturnType, allDeclarations, resolver);
                        foreach (ParameterDeclaration @param in @method.Parameters)
                        {
                            AddIfTypeNotFound(@param.Type, allDeclarations, resolver);
                        }
                    }

                    if (declaration is InterfaceDeclaration)
                    {
                        InterfaceDeclaration @interface = declaration as InterfaceDeclaration;
                        if (@interface.Base != null)
                        {
                            AddIfDeclarationNotFound(@interface.Base, allDeclarations, resolver);
                        }
                    }
                    else if (declaration is CategoryDeclaration)
                    {
                        CategoryDeclaration @category = declaration as CategoryDeclaration;
                        AddIfDeclarationNotFound(@category.ExtendedInterface, allDeclarations, resolver);
                    }
                }
            }

            List<DocumentDeclaration> finalDocuments = new List<DocumentDeclaration>(documents);
            foreach (DocumentDeclaration newDocument in newDocuments)
            {
                DocumentDeclaration oldDocument = documents.SingleOrDefault(c => c.Name == newDocument.Name);
                if (oldDocument != null)
                {
                    oldDocument.Declarations.AddRange(newDocument.Declarations);
                }
                else
                {
                    finalDocuments.Insert(0, newDocument);
                }
            }
            return finalDocuments;
        }

        private static void AddIfDeclarationNotFound(IDeclaration declaration, HashSet<IDeclaration> allDeclarations,
            Parser.IDocumentResolver resolver)
        {
            if ((declaration is BaseClass && !(declaration as BaseClass).IsContainer) ||
                (declaration is BaseRecordDeclaration &&
                 (declaration as BaseRecordDeclaration).IsAnonymousWithoutTypedef()))
            {
                return;
            }

            if (!allDeclarations.Contains(declaration))
            {
                if ((declaration as BaseDeclaration).Document != null)
                {
                    DocumentDeclaration newDocument = resolver.GetDocumentForDeclaration(declaration);
                    newDocument.Add(declaration);
                    allDeclarations.Add(declaration);    
                }
            }
        }

        private static void AddIfTypeNotFound(TypeDefinition type, HashSet<IDeclaration> allDeclarations,
            Parser.IDocumentResolver resolver)
        {
            PointerType pointer = type as PointerType;
            if (pointer != null)
            {
                AddIfTypeNotFound(pointer.Target, allDeclarations, resolver);
            }

            DeclarationReferenceType declRefType = type as DeclarationReferenceType;
            if (declRefType != null && declRefType.Target != null)
            {
                AddIfDeclarationNotFound(declRefType.Target, allDeclarations, resolver);
            }

            IncompleteArrayType arrayType = type as IncompleteArrayType;
            if (arrayType != null && arrayType.ElementType != null)
            {
                AddIfTypeNotFound(arrayType.ElementType, allDeclarations, resolver);
            }

            VectorType vectorType = type as VectorType;
            if (vectorType != null && vectorType.ElementType != null)
            {
                AddIfTypeNotFound(vectorType.ElementType, allDeclarations, resolver);
            }

            ComplexType complexType = type as ComplexType;
            if (complexType != null && complexType.Type != null)
            {
                AddIfTypeNotFound(complexType.Type, allDeclarations, resolver);
            }
        }
    }
}
