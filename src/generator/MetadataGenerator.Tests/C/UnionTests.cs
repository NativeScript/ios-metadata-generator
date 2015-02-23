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
    public class UnionTests
    {
        [Test]
        public void VisitUnion()
        {
            string declaration = @"union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[0]);
            UnionDeclaration UnionDeclaration = document.Declarations[0] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", UnionDeclaration.Name);
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsFalse(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitAnonymousUnion()
        {
            string declaration = @"union {
                                        int parts;
                                        int whole;
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[0]);
            UnionDeclaration UnionDeclaration = document.Declarations[0] as UnionDeclaration;
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsTrue(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitUnionDeclaration()
        {
            string declaration = @"union NumVersionVariant;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitUnionDeclaration2()
        {
            string declaration = @"union NumVersionVariant;
                                   union NumVersionVariant;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitUnionDeclarationFollowedByActualUnion()
        {
            string declaration = @"union NumVersionVariant;
                                   union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[0]);
            UnionDeclaration UnionDeclaration = document.Declarations[0] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", UnionDeclaration.Name);
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsFalse(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.AreEqual(2, UnionDeclaration.Location.Line);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitUnionFollowedByDeclaration()
        {
            string declaration = @"union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };
                                   union NumVersionVariant;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[0]);
            UnionDeclaration UnionDeclaration = document.Declarations[0] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", UnionDeclaration.Name);
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsFalse(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.AreEqual(1, UnionDeclaration.Location.Line);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitUnionDeclarationFollowedByActualUnion2()
        {
            string declaration = @"union NumVersionVariant;
                                   enum { DB_BTREE, DB_HASH, DB_RECNO };
                                   union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);
            UnionDeclaration UnionDeclaration = document.Declarations[1] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", UnionDeclaration.Name);
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsFalse(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.AreEqual(3, UnionDeclaration.Location.Line);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitUnionInUnion()
        {
            string declaration = @"union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };
                                   union secondUnion {
                                        union NumVersionVariant cc;
                                        int another;
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[0]);
            UnionDeclaration UnionDeclaration = document.Declarations[0] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", UnionDeclaration.Name);
            Assert.IsTrue(UnionDeclaration.IsDefinition);
            Assert.IsFalse(UnionDeclaration.IsAnonymous);
            Assert.IsFalse(UnionDeclaration.IsOpaque);
            Assert.IsNullOrEmpty(UnionDeclaration.TypedefName);
            Assert.AreEqual(2, UnionDeclaration.Fields.Count);

            FieldDeclaration field = UnionDeclaration.Fields[0];
            Assert.AreEqual("parts", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = UnionDeclaration.Fields[1];
            Assert.AreEqual("whole", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);
            UnionDeclaration UnionDeclaration2 = document.Declarations[1] as UnionDeclaration;
            Assert.AreEqual("secondUnion", UnionDeclaration2.Name);
            Assert.IsTrue(UnionDeclaration2.IsDefinition);
            Assert.IsFalse(UnionDeclaration2.IsAnonymous);
            Assert.IsFalse(UnionDeclaration2.IsOpaque);
            Assert.IsNullOrEmpty(UnionDeclaration2.TypedefName);
            Assert.AreEqual(2, UnionDeclaration2.Fields.Count);

            field = UnionDeclaration2.Fields[0];
            Assert.AreEqual("cc", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.AreSame(UnionDeclaration, (field.Type as DeclarationReferenceType).Target);

            field = UnionDeclaration2.Fields[1];
            Assert.AreEqual("another", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }
    }
}
