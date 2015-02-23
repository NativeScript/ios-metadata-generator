using System;
using System.Collections.Generic;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Parser;
using MetadataGenerator.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace MetadataGenerator.Tests
{
    [TestFixture]
    public class FunctionTests
    {
        [Test]
        public void VisitFunctionDeclaration()
        {
            string declaration = @"void foo();";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsFalse(functionDeclaration.IsDefinition);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (functionDeclaration.ReturnType as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionDeclaration2()
        {
            string declaration = @"void foo(int param1, float param2);";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsFalse(functionDeclaration.IsDefinition);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (functionDeclaration.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(2, functionDeclaration.Parameters.Count);
            ParameterDeclaration @param = functionDeclaration.Parameters[0];
            Assert.AreEqual("param1", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            @param = functionDeclaration.Parameters[1];
            Assert.AreEqual("param2", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Float, (@param.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionDefinition()
        {
            string declaration = @"int foo()
                                   {
                                       return 0;
                                   }";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsTrue(functionDeclaration.IsDefinition);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionDeclaration.ReturnType as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionDefinition2()
        {
            string declaration = @"int foo(int param1, float param2)
                                   {
                                       return 0;
                                   }";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsTrue(functionDeclaration.IsDefinition);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionDeclaration.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(2, functionDeclaration.Parameters.Count);
            ParameterDeclaration @param = functionDeclaration.Parameters[0];
            Assert.AreEqual("param1", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            @param = functionDeclaration.Parameters[1];
            Assert.AreEqual("param2", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Float, (@param.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionDeclarationFollowedByImplementation()
        {
            string declaration = @"int foo(int param);
                                   int foo(int param)
                                   {
                                       return 0;
                                   }";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsTrue(functionDeclaration.IsDefinition);
            Assert.AreEqual(2, functionDeclaration.Location.Line);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionDeclaration.ReturnType as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionImplementationFollowedByDeclaration()
        {
            string declaration = @"int foo(int param)
                                   {
                                       return 0;
                                   }
                                   int foo(int param);";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsTrue(functionDeclaration.IsDefinition);
            Assert.AreEqual(1, functionDeclaration.Location.Line);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionDeclaration.ReturnType as PrimitiveType).Type);
        }

        [Test]
        public void VisitFunctionDeclarationFollowedByImplementation2()
        {
            string declaration = @"int foo(int param);
                                   enum { DB_BTREE, DB_HASH, DB_RECNO };
                                   int foo(int param)
                                   {
                                       return 0;
                                   }";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<FunctionDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[1]);
            FunctionDeclaration functionDeclaration = document.Declarations[0] as FunctionDeclaration;
            Assert.AreEqual("foo", functionDeclaration.Name);
            Assert.IsTrue(functionDeclaration.IsDefinition);
            Assert.AreEqual(3, functionDeclaration.Location.Line);
            Assert.IsInstanceOf<PrimitiveType>(functionDeclaration.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (functionDeclaration.ReturnType as PrimitiveType).Type);
        }
    }
}
