using System;
using System.Collections.Generic;
using Libclang.Core.Ast;
using Libclang.Core.Parser;
using Libclang.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace Libclang.Tests
{
    [TestFixture]
    public class StructTests
    {
        [Test]
        public void VisitStruct()
        {
            string declaration = @"struct lconv {
                                        char point;
                                        int sep;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitAnonymousStruct()
        {
            string declaration = @"struct {
                                        char point;
                                        int sep;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.IsTrue(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructDeclaration()
        {
            string declaration = @"struct lconv;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitStructDeclaration2()
        {
            string declaration = @"struct lconv;
                                   struct lconv;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitStructDeclarationFollowedByActualStruct()
        {
            string declaration = @"struct lconv;
                                   struct lconv {
                                        char point;
                                        int sep;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.AreEqual(2, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructStructFollowedByDeclaration()
        {
            string declaration = @"struct lconv {
                                        char point;
                                        int sep;
                                   };
                                   struct lconv;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructDeclarationFollowedByActualStruct2()
        {
            string declaration = @"struct lconv;
                                   enum { DB_BTREE, DB_HASH, DB_RECNO };
                                   struct lconv {
                                        char point;
                                        int sep;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.AreEqual(3, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructInStruct()
        {
            string declaration = @"struct lconv {
                                        char point;
                                        int sep;
                                   };
                                   struct secondStruct {
                                        struct lconv cc;
                                        int another;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("point", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("sep", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration structDeclaration2 = document.Declarations[1] as StructDeclaration;
            Assert.AreEqual("secondStruct", structDeclaration2.Name);
            Assert.IsFalse(structDeclaration2.IsAnonymous);
            Assert.IsFalse(structDeclaration2.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration2.TypedefName);
            Assert.AreEqual(2, structDeclaration2.Fields.Count);

            field = structDeclaration2.Fields[0];
            Assert.AreEqual("cc", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.AreSame(structDeclaration, (field.Type as DeclarationReferenceType).Target);

            field = structDeclaration2.Fields[1];
            Assert.AreEqual("another", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructInStruct2()
        {
            string declaration = @"struct myStruct {
                                        struct {
                                            int v;
                                        };
                                        int another;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("myStruct", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.IsNullOrEmpty(field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);

            StructDeclaration innerStruct = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.IsNotNullOrEmpty(innerStruct.Name);
            Assert.IsTrue(innerStruct.IsAnonymous);
            Assert.IsFalse(innerStruct.IsOpaque);
            Assert.IsNullOrEmpty(innerStruct.TypedefName);
            Assert.AreEqual(1, innerStruct.Fields.Count);

            field = innerStruct.Fields[0];
            Assert.AreEqual("v", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("another", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructInStruct3()
        {
            string declaration = @"struct myStruct {
                                        struct nested {
                                            int v;
                                        };
                                        int another;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("myStruct", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(1, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("another", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            StructDeclaration secondStruct = document.Declarations[1] as StructDeclaration;
            Assert.AreEqual("nested", secondStruct.Name);
            Assert.IsFalse(secondStruct.IsAnonymous);
            Assert.IsFalse(secondStruct.IsOpaque);
            Assert.IsNullOrEmpty(secondStruct.TypedefName);
            Assert.AreEqual(1, secondStruct.Fields.Count);

            field = secondStruct.Fields[0];
            Assert.AreEqual("v", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitStructInStruct4()
        {
            string declaration = @"struct myStruct {
                                        struct nested {
                                            double h;
                                        } nestedStruct; 
                                        struct {
                                            int v;
                                        };
                                        int another;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration nestedStruct = document.Declarations[1] as StructDeclaration;
            Assert.AreEqual("nested", nestedStruct.Name);
            Assert.IsFalse(nestedStruct.IsAnonymous);
            Assert.IsFalse(nestedStruct.IsOpaque);
            Assert.IsNullOrEmpty(nestedStruct.TypedefName);
            Assert.AreEqual(1, nestedStruct.Fields.Count);

            FieldDeclaration field = nestedStruct.Fields[0];
            Assert.AreEqual("h", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (field.Type as PrimitiveType).Type);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[0]);
            StructDeclaration structDeclaration = document.Declarations[0] as StructDeclaration;
            Assert.AreEqual("myStruct", structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(3, structDeclaration.Fields.Count);

            field = structDeclaration.Fields[0];
            Assert.AreEqual("nestedStruct", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);
            Assert.AreSame(document.Declarations[1], (field.Type as DeclarationReferenceType).Target);

            field = structDeclaration.Fields[1];
            Assert.IsNullOrEmpty(field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);
            StructDeclaration innerStruct = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.IsNotNullOrEmpty(innerStruct.Name);
            Assert.IsTrue(innerStruct.IsAnonymous);
            Assert.IsFalse(innerStruct.IsOpaque);
            Assert.IsNullOrEmpty(innerStruct.TypedefName);
            Assert.AreEqual(1, innerStruct.Fields.Count);

            field = innerStruct.Fields[0];
            Assert.AreEqual("v", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[2];
            Assert.AreEqual("another", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitAnonymousStructs()
        {
            string declaration = @"struct {
                                       int ax;
                                       int bx;
                                   } w_T;
                                   struct {
                                       int ay;
                                       int by;
                                       int cy;
                                   } w_S;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(4, document.Declarations.Count);

            // Var 1
            Assert.IsInstanceOf<VarDeclaration>(document.Declarations[0]);
            VarDeclaration varDecl = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("w_T", varDecl.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(varDecl.Type);

            // Struct 1
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame((varDecl.Type as DeclarationReferenceType).Target, structDeclaration);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            // Check fields
            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("ax", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("bx", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            // Var 2
            Assert.IsInstanceOf<VarDeclaration>(document.Declarations[2]);
            varDecl = document.Declarations[2] as VarDeclaration;
            Assert.AreEqual("w_S", varDecl.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(varDecl.Type);

            // Struct 2
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[3]);
            structDeclaration = document.Declarations[3] as StructDeclaration;
            Assert.AreSame((varDecl.Type as DeclarationReferenceType).Target, structDeclaration);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            Assert.IsFalse(structDeclaration.IsOpaque);
            Assert.IsTrue(structDeclaration.IsDefinition);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(3, structDeclaration.Fields.Count);

            // Check fields
            field = structDeclaration.Fields[0];
            Assert.AreEqual("ay", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("by", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("by", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }
    }
}
