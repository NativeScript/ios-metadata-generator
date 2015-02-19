using Libclang.Core.Ast;
using Libclang.Core.Parser;
using Libclang.DocsetParser;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Libclang.Tests
{
    [TestFixture]
    public class APITests
    {
        private TokenMetadata[] docsetTokens;
        private IEnumerable<ModuleDeclaration> parsedModules;

        private static readonly string sdkPath = @"D:/MetadataInput/sdks/iPhoneOS8.1.sdk";// @"/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk";

        [TestFixtureSetUp]
        public void BeforeAll()
        {
            FrameworkParser parser = new FrameworkParser();
            this.parsedModules = parser.Parse("API/header.h", sdkPath, "", "armv7");
        }

        private IEnumerable GetTestCaseData()
        {
            this.docsetTokens = DocsetParser.DocsetParser.GetTokens();
            var query = from c in this.docsetTokens
                        select new TestCaseData(c)
                             .SetCategory(c.Module)
                             .SetName(string.Format("{0}::{1}::{2}", c.Module, c.Type.Name, c.Name));
            return query;
        }

        [Test, TestCaseSource("GetTestCaseData")]
        public void TestApi(TokenMetadata token)
        {
            var module = parsedModules.SingleOrDefault(c => c.Name == token.Module);
            Assert.IsNotNull(module, "Module not found!");

            if (!CheckModuleContainsToken(module, token))
            {
                Assert.Fail(string.Format("Declaration {0} not found in module {1}!", token.Name, token.Module));
            }
        }

        private bool CheckModuleContainsToken(ModuleDeclaration module, TokenMetadata token)
        {
            if (module.Declarations.Any(c => c.GetType() == token.Type && c.Name == token.Name))
            {
                return true;
            }
            else
            {
                foreach (var subModule in module.Submodules)
                {
                    if (CheckModuleContainsToken(subModule, token))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
