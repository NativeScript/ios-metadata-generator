using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Parser;
using MetadataGenerator.Core.Types;
using NUnit.Framework;
using System;
using System.Linq;

namespace MetadataGenerator.Tests
{
    [TestFixture]
    public class BlockTests
    {
        [Test]
        public void VisitBlockAsMethodParameter()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                       - (void)foo:(void (^)(int))param;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo:", method.Name);
            Assert.AreEqual("v12@0:4@?8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@param.Type);

            FunctionPointerType block = @param.Type as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(1, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@blockparam.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitBlockAsMethodParameter2()
        {
            string declaration = @"typedef signed char     BOOL; 
                                   @interface SimpleClass : NSObject
                                       - (void)foo:(void (^)(BOOL))param;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[1]);
            InterfaceDeclaration classDeclaration = document.Declarations[1] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo:", method.Name);
            Assert.AreEqual("v12@0:4@?8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@param.Type);

            FunctionPointerType block = @param.Type as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(1, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(@blockparam.Type);
            Assert.AreSame(document.Declarations[0], (@blockparam.Type as DeclarationReferenceType).Target);
        }

        [Test]
        public void VisitBlockInBlock()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                       - (void)foo:(void (^)(void (^reacquirer)(void))) param;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo:", method.Name);
            Assert.AreEqual("v12@0:4@?8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@param.Type);

            FunctionPointerType block = @param.Type as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(1, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@blockparam.Type);

            FunctionPointerType nestedBlock = @blockparam.Type as FunctionPointerType;
            Assert.IsTrue(nestedBlock.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(nestedBlock.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (nestedBlock.ReturnType as PrimitiveType).Type);
            Assert.AreEqual(0, nestedBlock.Parameters.Count);
        }

        [Test]
        public void VisitBlockInBlock2()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                       - (void)foo:(void (^)(void (^reacquirer)(int, double))) param;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo:", method.Name);
            Assert.AreEqual("v12@0:4@?8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@param.Type);

            FunctionPointerType block = @param.Type as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(1, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@blockparam.Type);

            FunctionPointerType nestedBlock = @blockparam.Type as FunctionPointerType;
            Assert.IsTrue(nestedBlock.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(nestedBlock.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (nestedBlock.ReturnType as PrimitiveType).Type);
            Assert.AreEqual(2, nestedBlock.Parameters.Count);

            @blockparam = nestedBlock.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@blockparam.Type as PrimitiveType).Type);

            @blockparam = nestedBlock.Parameters[1];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (@blockparam.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitBlockAsProperty()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                       @property (nonatomic, copy) int (^myProperty)(double, double);
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Action<FunctionPointerType> checkBlock = (block) =>
            {
                Assert.IsTrue(block.IsBlock);
                Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
                Assert.AreEqual(PrimitiveTypeType.Int, (block.ReturnType as PrimitiveType).Type);

                Assert.AreEqual(2, block.Parameters.Count);
                ParameterDeclaration @blockparam = block.Parameters[0];
                Assert.IsNullOrEmpty(@blockparam.Name);
                Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
                Assert.AreEqual(PrimitiveTypeType.Double, (@blockparam.Type as PrimitiveType).Type);

                @blockparam = block.Parameters[1];
                Assert.IsNullOrEmpty(@blockparam.Name);
                Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
                Assert.AreEqual(PrimitiveTypeType.Double, (@blockparam.Type as PrimitiveType).Type);
            };

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("myProperty", getMethod.Name);
            Assert.AreEqual("@?8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<FunctionPointerType>(getMethod.ReturnType);
            checkBlock(getMethod.ReturnType as FunctionPointerType);
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = classDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("setMyProperty:", setMethod.Name);
            Assert.AreEqual("v12@0:4@?8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            ParameterDeclaration @param = setMethod.Parameters[0];
            Assert.AreEqual("myProperty", @param.Name);
            Assert.IsInstanceOf<FunctionPointerType>(@param.Type);
            checkBlock(@param.Type as FunctionPointerType);

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<FunctionPointerType>(property.Type);
            checkBlock(property.Type as FunctionPointerType);
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsTrue(property.IsCopy);
            Assert.IsTrue(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsFalse(property.IsWeak);
        }

        [Test]
        public void VisitBlockAsTypedef()
        {
            string declaration = @"typedef int (^MyBlock)(int, double);";

            ModuleDeclaration document = new ModuleDeclaration("test");
            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(document));

            Assert.AreEqual(1, document.Declarations.Count);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("MyBlock", typeDefDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(typeDefDeclaration.OldType);

            FunctionPointerType block = typeDefDeclaration.OldType as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(2, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@blockparam.Type as PrimitiveType).Type);

            @blockparam = block.Parameters[1];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (@blockparam.Type as PrimitiveType).Type);
        }

        [Test]
        public void VisitBlockAsTypedef2()
        {
            string declaration = @"typedef int (^MyBlock)(int, double);
                                   @interface SimpleClass : NSObject
                                       - (void)foo: (MyBlock)param;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            FrameworkParser.ParserContext context = new FrameworkParser.ParserContext(document);
            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, new CDeclarationVisitor(context),
                new ObjCDeclarationVisitor(context));

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<TypedefDeclaration>(document.Declarations[0]);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[1]);
            TypedefDeclaration typeDefDeclaration = document.Declarations[0] as TypedefDeclaration;
            Assert.AreEqual("MyBlock", typeDefDeclaration.Name);
            Assert.IsInstanceOf<FunctionPointerType>(typeDefDeclaration.OldType);

            FunctionPointerType block = typeDefDeclaration.OldType as FunctionPointerType;
            Assert.IsTrue(block.IsBlock);
            Assert.IsInstanceOf<PrimitiveType>(block.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (block.ReturnType as PrimitiveType).Type);

            Assert.AreEqual(2, block.Parameters.Count);
            ParameterDeclaration @blockparam = block.Parameters[0];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@blockparam.Type as PrimitiveType).Type);

            @blockparam = block.Parameters[1];
            Assert.IsNullOrEmpty(@blockparam.Name);
            Assert.IsInstanceOf<PrimitiveType>(@blockparam.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (@blockparam.Type as PrimitiveType).Type);

            InterfaceDeclaration classDeclaration = document.Declarations[1] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo:", method.Name);
            Assert.AreEqual("v12@0:4@?8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<DeclarationReferenceType>(@param.Type);
            Assert.AreSame(typeDefDeclaration, (@param.Type as DeclarationReferenceType).Target);
        }
    }
}
