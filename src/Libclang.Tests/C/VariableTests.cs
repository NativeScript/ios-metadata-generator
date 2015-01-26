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
    public class VariableTests
    {
        [Test]
        public void VisitVar()
        {
            string declaration = @"double NSFoundationVersionNumber;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("NSFoundationVersionNumber", varDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(varDeclaration.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (varDeclaration.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitVar2()
        {
            string declaration = @"void * _NSConcreteGlobalBlock[32];";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("_NSConcreteGlobalBlock", varDeclaration.Name);
            Assert.IsInstanceOf<ConstantArrayType>(varDeclaration.Type);
            Assert.AreEqual(32, (varDeclaration.Type as ConstantArrayType).Size);
            Assert.IsInstanceOf<PointerType>((varDeclaration.Type as ConstantArrayType).ElementType);
            PointerType elementType = (varDeclaration.Type as ConstantArrayType).ElementType as PointerType;
            Assert.IsInstanceOf<PrimitiveType>(elementType.Target);
            Assert.AreEqual(PrimitiveTypeType.Void, (elementType.Target as PrimitiveType).Type);
        }

        [Test]
        public void VisitExtern()
        {
            string declaration = @"extern double NSFoundationVersionNumber;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("NSFoundationVersionNumber", varDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(varDeclaration.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (varDeclaration.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitExternFollowedByDefinition()
        {
            string declaration = @"extern double NSFoundationVersionNumber;
                                   double NSFoundationVersionNumber;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("NSFoundationVersionNumber", varDeclaration.Name);
            Assert.IsInstanceOf<PrimitiveType>(varDeclaration.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (varDeclaration.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitExternFollowedByDefinition2()
        {
            string declaration = @"extern void *_NSConstantStringClassReference;
                                   void *_NSConstantStringClassReference;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("_NSConstantStringClassReference", varDeclaration.Name);
            Assert.IsInstanceOf<PointerType>(varDeclaration.Type);
            Assert.IsInstanceOf<PrimitiveType>((varDeclaration.Type as PointerType).Target);
            Assert.AreEqual(PrimitiveTypeType.Void, ((varDeclaration.Type as PointerType).Target as PrimitiveType).Type);
        }

        [Test]
        public void VisitConstant()
        {
            string declaration = @"void * const NSHTTPCookieExpires;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("NSHTTPCookieExpires", varDeclaration.Name);
            Assert.IsTrue(varDeclaration.Type.IsConst);
            Assert.IsInstanceOf<PointerType>(varDeclaration.Type);
            Assert.IsInstanceOf<PrimitiveType>((varDeclaration.Type as PointerType).Target);
            Assert.AreEqual(PrimitiveTypeType.Void, ((varDeclaration.Type as PointerType).Target as PrimitiveType).Type);
        }

        [Test]
        public void VisitConstant2()
        {
            string declaration = @"const int myConst;";

            DocumentDeclaration document = new DocumentDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            VarDeclaration varDeclaration = document.Declarations[0] as VarDeclaration;
            Assert.AreEqual("myConst", varDeclaration.Name);
            Assert.IsTrue(varDeclaration.Type.IsConst);
            Assert.IsInstanceOf<PrimitiveType>(varDeclaration.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (varDeclaration.Type as PrimitiveType).Type);
        }
    }
}
