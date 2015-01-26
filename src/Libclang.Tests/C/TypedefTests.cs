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
    public class TypedefTests
    {
        [Test]
        public void VisitTypedefPrimitiveType()
        {
            string declaration = @"typedef	unsigned char		u_int8_t;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("u_int8_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(typeDefDeclaration.OldType);
            Assert.AreEqual(PrimitiveTypeType.UChar, (typeDefDeclaration.OldType as PrimitiveType).Type);
        }

        [Test]
        public void VisitTypedefOfTypedef()
        {
            string declaration = @"typedef	unsigned long long	u_int64_t;
                                   typedef u_int64_t		user_addr_t;
                                   typedef u_int64_t		user_size_t;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(3, document.Declarations.Count);
            TypedefDeclaration baseTypeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("u_int64_t", baseTypeDefDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(baseTypeDefDeclaration.OldType);
            Assert.AreEqual(PrimitiveTypeType.ULongLong, (baseTypeDefDeclaration.OldType as PrimitiveType).Type);

            TypedefDeclaration typeDefDeclaration = document.Declarations[1] as TypedefDeclaration;
            Assert.AreEqual("user_addr_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            Assert.AreSame(baseTypeDefDeclaration, (typeDefDeclaration.OldType as DeclarationReferenceType).Target);

            typeDefDeclaration = document.Declarations[2] as TypedefDeclaration;
            Assert.AreEqual("user_size_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            Assert.AreSame(baseTypeDefDeclaration, (typeDefDeclaration.OldType as DeclarationReferenceType).Target);
        }

        [Test]
        public void VisitTypedefUnion()
        {
            string declaration = @"typedef union {
	                                   char		__mbstate8[128];
	                                   long long	_mbstateL;
                                   } __mbstate_t;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("__mbstate_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType unionDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<UnionDeclaration>(unionDeclarationRef.Target);

            UnionDeclaration unionDeclaration = document.Declarations[1] as UnionDeclaration;
            Assert.AreSame(unionDeclaration, unionDeclarationRef.Target);
            Assert.IsNotEmpty(unionDeclaration.Name);
            Assert.IsTrue(unionDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, unionDeclaration.TypedefName);

            FieldDeclaration field = unionDeclaration.Fields[0];
            Assert.AreEqual("__mbstate8", field.Name);
            Assert.IsInstanceOf<ConstantArrayType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS,
                ((field.Type as ConstantArrayType).ElementType as PrimitiveType).Type);
            Assert.AreEqual(128, (field.Type as ConstantArrayType).Size);

            field = unionDeclaration.Fields[1];
            Assert.AreEqual("_mbstateL", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.LongLong, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitTypedefUnion2()
        {
            string declaration = @"typedef union __mbstate_t __mbstate_t;
                                   union __mbstate_t {
	                                   char		__mbstate8[128];
	                                   long long	_mbstateL;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);

            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("__mbstate_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType unionDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<UnionDeclaration>(unionDeclarationRef.Target);

            UnionDeclaration unionDeclaration = document.Declarations[1] as UnionDeclaration;
            Assert.AreSame(unionDeclaration, unionDeclarationRef.Target);
            Assert.AreEqual("__mbstate_t", unionDeclaration.Name);
            Assert.AreEqual(2, unionDeclaration.Location.Line);
            Assert.IsFalse(unionDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, unionDeclaration.TypedefName);

            FieldDeclaration field = unionDeclaration.Fields[0];
            Assert.AreEqual("__mbstate8", field.Name);
            Assert.IsInstanceOf<ConstantArrayType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS,
                ((field.Type as ConstantArrayType).ElementType as PrimitiveType).Type);
            Assert.AreEqual(128, (field.Type as ConstantArrayType).Size);

            field = unionDeclaration.Fields[1];
            Assert.AreEqual("_mbstateL", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.LongLong, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void TestTypedefAnonymousUnionInUnion()
        {
            string declaration = @"typedef union TNSNestedAnonymousUnion {
                                        int x1;
                                        union {
                                            int x2;
                                            union {
                                                int x3;
                                            } y2;
                                        } y1;
                                    } TNSNestedAnonymousUnion;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);

            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("TNSNestedAnonymousUnion", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<UnionDeclaration>(structDeclarationRef.Target);

            UnionDeclaration structDeclaration = document.Declarations[1] as UnionDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("TNSNestedAnonymousUnion", structDeclaration.Name);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("x1", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("y1", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<UnionDeclaration>((field.Type as DeclarationReferenceType).Target);

            structDeclaration = (field.Type as DeclarationReferenceType).Target as UnionDeclaration;
            Assert.AreEqual(2, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(3, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x2", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("y2", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<UnionDeclaration>((field.Type as DeclarationReferenceType).Target);

            structDeclaration = (field.Type as DeclarationReferenceType).Target as UnionDeclaration;
            Assert.AreEqual(1, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(5, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x3", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void TestTypedefAnonymousEnumInStruct()
        {
            string declaration = @"typedef struct TNSNestedAnonymousEnum {
                                        int x1;
                                        enum { x, y, z} pos;
                                    } TNSNestedAnonymousEnum;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);

            // Typedef
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("TNSNestedAnonymousEnum", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            // Struct
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("TNSNestedAnonymousEnum", structDeclaration.Name);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            // Check fields
            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("x1", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("pos", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<EnumDeclaration>((field.Type as DeclarationReferenceType).Target);

            // Enum
            EnumDeclaration enumDeclaration = (field.Type as DeclarationReferenceType).Target as EnumDeclaration;
            Assert.IsNotEmpty(enumDeclaration.Name);
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(3, enumDeclaration.Fields.Count);

            // Enum members
            EnumMemberDeclaration enumMember = enumDeclaration.Fields[0];
            Assert.AreEqual("x", enumMember.Name);
            Assert.AreEqual(0, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[1];
            Assert.AreEqual("y", enumMember.Name);
            Assert.AreEqual(1, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[2];
            Assert.AreEqual("z", enumMember.Name);
            Assert.AreEqual(2, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);
        }

        [Test]
        public void TestTypedefAnonymousEnumInStruct2()
        {
            string declaration = @"typedef struct TNSNestedAnonymousEnum {
                                        int x1;
                                        enum { x, y, z} pos;
                                    } TNSNestedAnonymousEnum, *TNSAnonymousEnum;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(3, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[2]);

            // Typedef 1
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("TNSNestedAnonymousEnum", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            // Struct
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("TNSNestedAnonymousEnum", structDeclaration.Name);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            // Check fields
            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("x1", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("pos", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<EnumDeclaration>((field.Type as DeclarationReferenceType).Target);

            // Enum
            EnumDeclaration enumDeclaration = (field.Type as DeclarationReferenceType).Target as EnumDeclaration;
            Assert.IsNotEmpty(enumDeclaration.Name);
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(3, enumDeclaration.Fields.Count);

            // Enum members
            EnumMemberDeclaration enumMember = enumDeclaration.Fields[0];
            Assert.AreEqual("x", enumMember.Name);
            Assert.AreEqual(0, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[1];
            Assert.AreEqual("y", enumMember.Name);
            Assert.AreEqual(1, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[2];
            Assert.AreEqual("z", enumMember.Name);
            Assert.AreEqual(2, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            // Typedef 2
            typeDefDeclaration = document.Declarations[2] as TypedefDeclaration;
            Assert.AreEqual("TNSAnonymousEnum", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PointerType>(typeDefDeclaration.OldType);
            Assert.IsInstanceOf<DeclarationReferenceType>((typeDefDeclaration.OldType as PointerType).Target);
            structDeclarationRef = (typeDefDeclaration.OldType as PointerType).Target as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);
            Assert.AreSame(document.Declarations[1], structDeclarationRef.Target);
        }

        [Test]
        public void VisitTypedefStruct()
        {
            string declaration = @"typedef struct {
	                                   void	*data;
	                                   int	 size;
                                   } DBT;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("DBT", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType unionDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(unionDeclarationRef.Target);

            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, unionDeclarationRef.Target);
            Assert.IsNotEmpty(structDeclaration.Name);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("data", field.Name);
            Assert.IsInstanceOf<PointerType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Void, ((field.Type as PointerType).Target as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("size", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitTypedefStruct2()
        {
            string declaration = @"typedef struct CGAffineTransform CGAffineTransform;
                                   struct CGAffineTransform {
                                        float a, b, c, d;
                                        float tx, ty;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);

            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("CGAffineTransform", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("CGAffineTransform", structDeclaration.Name);
            Assert.AreEqual(2, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(6, structDeclaration.Fields.Count);

            string[] expectedNames = {"a", "b", "c", "d", "tx", "ty"};

            for (int i = 0; i < structDeclaration.Fields.Count; i++)
            {
                FieldDeclaration field = structDeclaration.Fields[i];
                Assert.AreEqual(expectedNames[i], field.Name);
                Assert.IsInstanceOf<PrimitiveType>(field.Type);
                Assert.AreEqual(PrimitiveTypeType.Float, (field.Type as PrimitiveType).Type);
            }
        }

        [Test]
        public void TestTypedefAnonymousStructInStruct()
        {
            string declaration = @"typedef struct TNSNestedAnonymousStruct {
                                        int x1;
                                        struct {
                                            int x2;
                                            struct {
                                                int x3;
                                            } y2;
                                        } y1;
                                    } TNSNestedAnonymousStruct;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);

            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("TNSNestedAnonymousStruct", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("TNSNestedAnonymousStruct", structDeclaration.Name);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("x1", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("y1", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);

            structDeclaration = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.AreEqual(2, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(3, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x2", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("y2", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);

            structDeclaration = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.AreEqual(1, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(5, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x3", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);
        }

        [Test]
        public void TestTypedefDuplicateStruct()
        {
            string declaration = @"typedef struct gss_OID_desc_struct {
                                         int length;
                                         void *elements;
                                   } gss_OID_desc, *gss_OID;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(3, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[2]);

            // Typedef 1
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("gss_OID_desc", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            // Struct
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("gss_OID_desc_struct", structDeclaration.Name);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);

            // Check fields
            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("length", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("elements", field.Name);
            Assert.IsInstanceOf<PointerType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Void, ((field.Type as PointerType).Target as PrimitiveType).Type);

            // Typedef 2
            typeDefDeclaration = document.Declarations[2] as TypedefDeclaration;
            Assert.AreEqual("gss_OID", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PointerType>(typeDefDeclaration.OldType);
            Assert.IsInstanceOf<DeclarationReferenceType>((typeDefDeclaration.OldType as PointerType).Target);
            structDeclarationRef = (typeDefDeclaration.OldType as PointerType).Target as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
        }

        [Test]
        public void TestTypedefAnonymousStructInStruct2()
        {
            string declaration = @"typedef struct gss_OID_desc_struct {
                                         int length;
                                         struct {
                                            int x2;
                                            struct {
                                                int x3;
                                            } y2;
                                        } y1;
                                         void *elements;
                                   } gss_OID_desc, *gss_OID;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(3, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[2]);

            // Typedef 1
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("gss_OID_desc", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType structDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);

            // Struct gss_OID_desc_struct
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreSame(structDeclaration, structDeclarationRef.Target);
            Assert.AreEqual("gss_OID_desc_struct", structDeclaration.Name);
            Assert.AreEqual(1, structDeclaration.Location.Line);
            Assert.IsFalse(structDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, structDeclaration.TypedefName);
            Assert.AreEqual(3, structDeclaration.Fields.Count);

            // Fields of gss_OID_desc_struct
            FieldDeclaration field = structDeclaration.Fields[0];
            Assert.AreEqual("length", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            field = structDeclaration.Fields[2];
            Assert.AreEqual("elements", field.Name);
            Assert.IsInstanceOf<PointerType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Void, ((field.Type as PointerType).Target as PrimitiveType).Type);

            field = structDeclaration.Fields[1];
            Assert.AreEqual("y1", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);

            // Struct y1
            structDeclaration = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.AreEqual(2, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(3, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x2", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            // Fields of y1
            field = structDeclaration.Fields[1];
            Assert.AreEqual("y2", field.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(field.Type);
            Assert.IsInstanceOf<StructDeclaration>((field.Type as DeclarationReferenceType).Target);

            // Struct y2
            structDeclaration = (field.Type as DeclarationReferenceType).Target as StructDeclaration;
            Assert.AreEqual(1, structDeclaration.Fields.Count);
            Assert.IsNotNullOrEmpty(structDeclaration.Name);
            Assert.AreEqual(5, structDeclaration.Location.Line);
            Assert.IsTrue(structDeclaration.IsAnonymous);

            // Fields of y2
            field = structDeclaration.Fields[0];
            Assert.AreEqual("x3", field.Name);
            Assert.IsInstanceOf<PrimitiveType>(field.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (field.Type as PrimitiveType).Type);

            // Typedef 2
            typeDefDeclaration = document.Declarations[2] as TypedefDeclaration;
            Assert.AreEqual("gss_OID", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PointerType>(typeDefDeclaration.OldType);
            Assert.IsInstanceOf<DeclarationReferenceType>((typeDefDeclaration.OldType as PointerType).Target);
            structDeclarationRef = (typeDefDeclaration.OldType as PointerType).Target as DeclarationReferenceType;
            Assert.IsInstanceOf<StructDeclaration>(structDeclarationRef.Target);
            Assert.AreSame(document.Declarations[1], structDeclarationRef.Target);
        }

        [Test]
        public void VisitTypedefEnum()
        {
            string declaration = @"typedef enum { DB_BTREE, DB_HASH, DB_RECNO } DBTYPE;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[1]);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("DBTYPE", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType unionDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<EnumDeclaration>(unionDeclarationRef.Target);

            EnumDeclaration enumDeclaration = document.Declarations[1] as EnumDeclaration;
            Assert.AreSame(enumDeclaration, unionDeclarationRef.Target);
            Assert.IsNotEmpty(enumDeclaration.Name);
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, enumDeclaration.TypedefName);

            EnumMemberDeclaration enumMember = enumDeclaration.Fields[0];
            Assert.AreEqual("DB_BTREE", enumMember.Name);
            Assert.AreEqual(0, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[1];
            Assert.AreEqual("DB_HASH", enumMember.Name);
            Assert.AreEqual(1, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[2];
            Assert.AreEqual("DB_RECNO", enumMember.Name);
            Assert.AreEqual(2, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);
        }

        [Test]
        public void VisitTypedefEnum2()
        {
            string declaration = @"typedef enum DBTYPE DBTYPE;
                                   enum DBTYPE { DB_BTREE, DB_HASH, DB_RECNO };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("DBTYPE", typeDefDeclaration.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(typeDefDeclaration.OldType);
            DeclarationReferenceType enumDeclarationRef = typeDefDeclaration.OldType as DeclarationReferenceType;
            Assert.IsInstanceOf<EnumDeclaration>(enumDeclarationRef.Target);

            EnumDeclaration enumDeclaration = document.Declarations[1] as EnumDeclaration;
            Assert.AreSame(enumDeclaration, enumDeclarationRef.Target);
            Assert.AreEqual("DBTYPE", enumDeclaration.Name);
            Assert.IsFalse(enumDeclaration.IsAnonymous);
            Assert.AreEqual(typeDefDeclaration.Name, enumDeclaration.TypedefName);

            EnumMemberDeclaration enumMember = enumDeclaration.Fields[0];
            Assert.AreEqual("DB_BTREE", enumMember.Name);
            Assert.AreEqual(0, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[1];
            Assert.AreEqual("DB_HASH", enumMember.Name);
            Assert.AreEqual(1, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);

            enumMember = enumDeclaration.Fields[2];
            Assert.AreEqual("DB_RECNO", enumMember.Name);
            Assert.AreEqual(2, enumMember.Value);
            Assert.AreSame(enumDeclaration, enumMember.Parent);
        }

        [Test]
        public void VisitTypedefFunctionPointer()
        {
            string declaration = @"typedef id (*IMP)(id, SEL, ...);";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("IMP", typeDefDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(typeDefDeclaration.OldType);
            FunctionPointerType functionPointer = typeDefDeclaration.OldType as FunctionPointerType;

            Assert.IsTrue(functionPointer.IsVariadic);
            Assert.IsFalse(functionPointer.IsBlock);
            Assert.AreEqual(1, functionPointer.Id);
            Assert.IsInstanceOf<IdType>(functionPointer.ReturnType);

            Assert.AreEqual(2, functionPointer.Parameters.Count);

            ParameterDeclaration parameter = functionPointer.Parameters[0];
            Assert.IsInstanceOf<IdType>(parameter.Type);

            parameter = functionPointer.Parameters[1];
            Assert.IsInstanceOf<SelectorType>(parameter.Type);
        }

        [Test]
        public void VisitTypedefFunctionPointer2()
        {
            string declaration = @"@class NSException;
                                   typedef void NSUncaughtExceptionHandler(NSException *exception);";

            DocumentDeclaration document = new DocumentDeclaration("test");
            var context = new FrameworkParser.ParserContext(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("NSUncaughtExceptionHandler", typeDefDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(typeDefDeclaration.OldType);
            FunctionPointerType functionPointer = typeDefDeclaration.OldType as FunctionPointerType;

            Assert.IsFalse(functionPointer.IsVariadic);
            Assert.IsFalse(functionPointer.IsBlock);
            Assert.AreEqual(1, functionPointer.Id);
            Assert.IsInstanceOf<PrimitiveType>(functionPointer.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (functionPointer.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(1, functionPointer.Parameters.Count);

            ParameterDeclaration parameter = functionPointer.Parameters[0];
            Assert.IsInstanceOf<PointerType>(parameter.Type);
            Assert.IsInstanceOf<DeclarationReferenceType>((parameter.Type as PointerType).Target);
            DeclarationReferenceType decl = (parameter.Type as PointerType).Target as DeclarationReferenceType;
            Assert.IsInstanceOf<InterfaceDeclaration>(decl.Target);
        }

        [Test]
        public void VisitTypedefFunctionPointer3()
        {
            string declaration = @"@class NSException;
                                   typedef void NSUncaughtExceptionHandler(NSException *exception);
                                   NSUncaughtExceptionHandler *NSGetUncaughtExceptionHandler(void);";

            DocumentDeclaration document = new DocumentDeclaration("test");

            LibclangHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(document),
                new ObjCDeclarationVisitor(document));

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[1]);

            FunctionDeclaration functionDeclaration = document.Declarations[1] as FunctionDeclaration;
            Assert.AreEqual("NSGetUncaughtExceptionHandler", functionDeclaration.Name);
            Assert.IsInstanceOf<PointerType>(functionDeclaration.ReturnType);
            Assert.IsInstanceOf<DeclarationReferenceType>((functionDeclaration.ReturnType as PointerType).Target);
            DeclarationReferenceType typedefRef =
                (functionDeclaration.ReturnType as PointerType).Target as DeclarationReferenceType;
            Assert.AreSame(document.Declarations[0], typedefRef.Target);
            Assert.AreEqual(0, functionDeclaration.Parameters.Count);
        }

        [Test]
        public void VisitStructAfterTypedef()
        {
            string declaration = @"typedef	unsigned char		u_int8_t;
                                   struct lconv {
                                        char point;
                                        int sep;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("u_int8_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(typeDefDeclaration.OldType);
            Assert.AreEqual(PrimitiveTypeType.UChar, (typeDefDeclaration.OldType as PrimitiveType).Type);

            Assert.IsInstanceOf<StructDeclaration>(document.Declarations[1]);
            StructDeclaration structDeclaration = document.Declarations[1] as StructDeclaration;
            Assert.AreEqual("lconv", structDeclaration.Name);
            Assert.IsNullOrEmpty(structDeclaration.TypedefName);
            Assert.AreEqual(2, structDeclaration.Fields.Count);
        }

        [Test]
        public void VisitUnionAfterTypedef()
        {
            string declaration = @"typedef	unsigned char		u_int8_t;
                                   union NumVersionVariant {
                                        int parts;
                                        int whole;
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("u_int8_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(typeDefDeclaration.OldType);
            Assert.AreEqual(PrimitiveTypeType.UChar, (typeDefDeclaration.OldType as PrimitiveType).Type);

            Assert.IsInstanceOf<UnionDeclaration>(document.Declarations[1]);
            UnionDeclaration unionDeclaration = document.Declarations[1] as UnionDeclaration;
            Assert.AreEqual("NumVersionVariant", unionDeclaration.Name);
            Assert.IsNullOrEmpty(unionDeclaration.TypedefName);
            Assert.AreEqual(2, unionDeclaration.Fields.Count);
        }

        [Test]
        public void VisitEnumAfterTypedef()
        {
            string declaration = @"typedef	unsigned char		u_int8_t;
                                   enum myEnum {
                                        kDNSServiceFlagsMoreComing          = 0x1,
                                        kDNSServiceFlagsAdd                 = 0x2,
                                   };";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("u_int8_t", typeDefDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(typeDefDeclaration.OldType);
            Assert.AreEqual(PrimitiveTypeType.UChar, (typeDefDeclaration.OldType as PrimitiveType).Type);

            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[1]);
            EnumDeclaration enumDeclaration = document.Declarations[1] as EnumDeclaration;
            Assert.AreEqual("myEnum", enumDeclaration.Name);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(2, enumDeclaration.Fields.Count);
        }

        /*
         * Blocks
         */

        [Test]
        public void VisitSimpleBlockDeclaration()
        {
            string declaration = @"void (^blockReturningVoidWithVoidArgument)(void);";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<VarDeclaration>(document.Declarations[0]);
            VarDeclaration blockDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("blockReturningVoidWithVoidArgument", blockDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(blockDeclaration.Type);
            FunctionPointerType functionPointer = blockDeclaration.Type as FunctionPointerType;

            Assert.IsFalse(functionPointer.IsVariadic);
            Assert.IsTrue(functionPointer.IsBlock);
            Assert.AreEqual(1, functionPointer.Id);
            Assert.IsInstanceOf<PrimitiveType>(functionPointer.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (functionPointer.ReturnType as PrimitiveType).Type);
            Assert.AreEqual(0, functionPointer.Parameters.Count);
        }

        [Test]
        public void VisitBlockWithReturnDeclaration()
        {
            string declaration = @"int (^blockReturningIntWithIntAndCharArguments)(int, char);";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<VarDeclaration>(document.Declarations[0]);
            VarDeclaration blockDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("blockReturningIntWithIntAndCharArguments", blockDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(blockDeclaration.Type);
            FunctionPointerType functionPointer = blockDeclaration.Type as FunctionPointerType;

            Assert.IsFalse(functionPointer.IsVariadic);
            Assert.IsTrue(functionPointer.IsBlock);
            Assert.AreEqual(1, functionPointer.Id);
            Assert.IsInstanceOf<PrimitiveType>(functionPointer.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionPointer.ReturnType as PrimitiveType).Type);
            Assert.AreEqual(2, functionPointer.Parameters.Count);

            ParameterDeclaration parameter = functionPointer.Parameters[0];
            Assert.IsInstanceOf<PrimitiveType>(parameter.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (parameter.Type as PrimitiveType).Type);

            parameter = functionPointer.Parameters[1];
            Assert.IsInstanceOf<PrimitiveType>(parameter.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (parameter.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitTypedefBlock()
        {
            string declaration = @"typedef float (^MyBlockType)(float, float);";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("MyBlockType", typeDefDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(typeDefDeclaration.OldType);

            FunctionPointerType functionPointer = typeDefDeclaration.OldType as FunctionPointerType;

            Assert.IsFalse(functionPointer.IsVariadic);
            Assert.IsTrue(functionPointer.IsBlock);
            Assert.AreEqual(1, functionPointer.Id);
            Assert.IsInstanceOf<PrimitiveType>(functionPointer.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Float, (functionPointer.ReturnType as PrimitiveType).Type);
            Assert.AreEqual(2, functionPointer.Parameters.Count);

            ParameterDeclaration parameter = functionPointer.Parameters[0];
            Assert.IsInstanceOf<PrimitiveType>(parameter.Type);
            Assert.AreEqual(PrimitiveTypeType.Float, (parameter.Type as PrimitiveType).Type);

            parameter = functionPointer.Parameters[1];
            Assert.IsInstanceOf<PrimitiveType>(parameter.Type);
            Assert.AreEqual(PrimitiveTypeType.Float, (parameter.Type as PrimitiveType).Type);
        }
    }
}
