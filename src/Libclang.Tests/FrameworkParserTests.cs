﻿using Libclang.Core.Ast;
using Libclang.Core.Parser;
using Libclang.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace Libclang.Tests
{
    [TestFixture]
    public class FrameworkParserTests
    {
        [Test]
        public void TestFrameworkResolver()
        {
            string document1 = @"@interface SimpleClass : NSObject
                                 @end";

            string tempFolder = System.IO.Path.GetTempPath();
            string frameworkPath = System.IO.Path.Combine(tempFolder, "SimpleFramework.framework");
            string filename1 = System.IO.Path.Combine(frameworkPath, "SimpleClass.h");
            try
            {
                System.IO.Directory.CreateDirectory(frameworkPath);
                System.IO.File.WriteAllText(filename1, document1);

                FrameworkParser.ParserContext context = new FrameworkParser.ParserContext("");
                ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(context);
                LibclangHelper.ParseFileWithVisitor(filename1, visitor);

                Assert.AreEqual(1, context.documents.Count);
                Assert.AreEqual("SimpleFramework", context.documents[0].Name);
            }
            finally
            {
                System.IO.Directory.Delete(frameworkPath, true);
            }
        }

        [Test]
        public void TestDocumentResolver()
        {
            string document1Code = @"@interface SimpleClass : NSObject
                                     @end";

            string document2Code = @"#include ""SimpleFramework.framework/SimpleClass.h""
                                   @interface SimpleClass2 : SimpleClass
                                   @end";

            string tempFolder = System.IO.Path.GetTempPath();
            string frameworkPath = System.IO.Path.Combine(tempFolder, "SimpleFramework.framework");
            string filename1 = System.IO.Path.Combine(frameworkPath, "SimpleClass.h");
            string filename2 = System.IO.Path.Combine(tempFolder, "SimpleClass2.h");
            try
            {
                System.IO.Directory.CreateDirectory(frameworkPath);
                System.IO.File.WriteAllText(filename1, document1Code);
                System.IO.File.WriteAllText(filename2, document2Code);

                FrameworkParser.ParserContext context = new FrameworkParser.ParserContext("");
                ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(context);
                LibclangHelper.ParseFileWithVisitor(filename2, visitor);

                Assert.AreEqual(2, context.documents.Count);

                DocumentDeclaration document1 = context.documents[0];
                Assert.AreEqual("SimpleFramework", document1.Name);
                Assert.AreEqual(1, document1.Declarations.Count);
                Assert.IsInstanceOf<InterfaceDeclaration>(document1.Declarations[0]);
                InterfaceDeclaration class1 = document1.Declarations[0] as InterfaceDeclaration;
                Assert.AreEqual("SimpleClass", class1.Name);

                DocumentDeclaration document2 = context.documents[1];
                Assert.AreEqual("UsrLib", document2.Name);
                Assert.AreEqual(1, document2.Declarations.Count);
                Assert.IsInstanceOf<InterfaceDeclaration>(document2.Declarations[0]);
                InterfaceDeclaration class2 = document2.Declarations[0] as InterfaceDeclaration;
                Assert.AreEqual("SimpleClass2", class2.Name);
                Assert.AreSame(class2.Base, class1);
            }
            finally
            {
                System.IO.Directory.Delete(frameworkPath, true);
                System.IO.File.Delete(filename2);
            }
        }
    }
}
