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
    public class EnumTests
    {
        [Test]
        public void VisitEnum()
        {
            string declaration = @"enum myEnum {
                                        kDNSServiceFlagsMoreComing          = 0x1,
                                        kDNSServiceFlagsAdd                 = 0x2,
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[0]);
            EnumDeclaration enumDeclaration = document.Declarations[0] as EnumDeclaration;
            Assert.AreEqual("myEnum", enumDeclaration.Name);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(2, enumDeclaration.Fields.Count);
        }

        [Test]
        public void VisitAnonymousEnum()
        {
            string declaration = @"enum {
                                        kDNSServiceFlagsMoreComing          = 0x1,
                                        kDNSServiceFlagsAdd                 = 0x2,
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[0]);
            EnumDeclaration enumDeclaration = document.Declarations[0] as EnumDeclaration;
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(2, enumDeclaration.Fields.Count);
        }

        [Test]
        public void VisitAnonymousEnums()
        {
            string declaration = @"enum {
                                       kFFTDirection_Forward         = +1,
                                       kFFTDirection_Inverse         = -1
                                   };
                                   enum {
                                       kFFTRadix2                    = 0,
                                       kFFTRadix3                    = 1,
                                       kFFTRadix5                    = 2
                                   };";

            ModuleDeclaration document = new ModuleDeclaration("test");
            CDeclarationVisitor visitor = new CDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            // Check declaration 1
            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[0]);
            EnumDeclaration enumDeclaration = document.Declarations[0] as EnumDeclaration;
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(2, enumDeclaration.Fields.Count);

            // Check fields
            EnumMemberDeclaration field = enumDeclaration.Fields[0];
            Assert.AreEqual("kFFTDirection_Forward", field.Name);
            Assert.AreEqual(+1, field.Value);

            field = enumDeclaration.Fields[1];
            Assert.AreEqual("kFFTDirection_Inverse", field.Name);
            Assert.AreEqual(-1, field.Value);

            // Check declaration 2
            Assert.IsInstanceOf<EnumDeclaration>(document.Declarations[1]);
            enumDeclaration = document.Declarations[1] as EnumDeclaration;
            Assert.IsTrue(enumDeclaration.IsAnonymous);
            Assert.IsNullOrEmpty(enumDeclaration.TypedefName);
            Assert.AreEqual(3, enumDeclaration.Fields.Count);

            // Check fields
            field = enumDeclaration.Fields[0];
            Assert.AreEqual("kFFTRadix2", field.Name);
            Assert.AreEqual(0, field.Value);

            field = enumDeclaration.Fields[1];
            Assert.AreEqual("kFFTRadix3", field.Name);
            Assert.AreEqual(1, field.Value);

            field = enumDeclaration.Fields[2];
            Assert.AreEqual("kFFTRadix5", field.Name);
            Assert.AreEqual(2, field.Value);
        }
    }
}
