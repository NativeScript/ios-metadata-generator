using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Parser;
using MetadataGenerator.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace MetadataGenerator.Tests
{
    [TestFixture]
    public class ClassTests
    {
        [Test]
        public void VisitSimpleClass()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual(0, classDeclaration.Methods.Count);
        }

        [Test]
        public void VisitSimpleClass2()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end
                                   @class SimpleClass;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual(0, classDeclaration.Methods.Count);
        }

        [Test]
        public void VisitClassWithInstanceMethod1()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (void) foo;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo", method.Name);
            Assert.AreEqual("v8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceMethod2()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (float) fooWithReturn;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithReturn", method.Name);
            Assert.AreEqual("f8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Float, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceMethod3()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (void) fooWithParam: (int) param;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithParam:", method.Name);
            Assert.AreEqual("v12@0:4i8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceMethod4()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (void) fooWithParam: (int) param secondParam: (double) param2;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithParam:secondParam:", method.Name);
            Assert.AreEqual("v20@0:4i8d12", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(2, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            @param = method.Parameters[1];
            Assert.AreEqual("param2", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceMethod5()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (const char *) foo;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo", method.Name);
            Assert.AreEqual("r*8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PointerType>(method.ReturnType);
            TypeDefinition pointeeType = (method.ReturnType as PointerType).Target;
            Assert.IsInstanceOf<PrimitiveType>(pointeeType);
            Assert.AreEqual(PrimitiveTypeType.CharS, (pointeeType as PrimitiveType).Type);
            Assert.IsTrue((pointeeType as PrimitiveType).IsConst);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceMethod6()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (SimpleClass *) foo;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo", method.Name);
            Assert.AreEqual("@8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PointerType>(method.ReturnType);
            TypeDefinition pointeeType = (method.ReturnType as PointerType).Target;
            Assert.IsInstanceOf<DeclarationReferenceType>(pointeeType);
            Assert.AreSame(classDeclaration, (pointeeType as DeclarationReferenceType).Target);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithInstanceConstructor()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    - (instancetype) initWithChar: (char) param;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("initWithChar:", method.Name);
            Assert.AreEqual("@12@0:4c8", method.TypeEncoding);
            Assert.IsInstanceOf<InstanceType>(method.ReturnType);
            Assert.IsTrue(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.CharS, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithStaticConstructor()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    + (instancetype) classWithChar: (float) param;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("classWithChar:", method.Name);
            Assert.AreEqual("@12@0:4f8", method.TypeEncoding);
            Assert.IsInstanceOf<InstanceType>(method.ReturnType);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsTrue(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Float, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithStaticMethod1()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    + (void) foo;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("foo", method.Name);
            Assert.AreEqual("v8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsTrue(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithStaticMethod2()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    + (long) fooWithReturn;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithReturn", method.Name);
            Assert.AreEqual("l8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Long, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsTrue(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithStaticMethod3()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    + (void) fooWithParam: (int) param;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithParam:", method.Name);
            Assert.AreEqual("v12@0:4i8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsTrue(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithStaticMethod4()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    + (void) fooWithParam: (int) param secondParam: (double) param2;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithParam:secondParam:", method.Name);
            Assert.AreEqual("v20@0:4i8d12", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsTrue(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(2, method.Parameters.Count);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            @param = method.Parameters[1];
            Assert.AreEqual("param2", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassWithProperty()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    @property int myProperty;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("myProperty", getMethod.Name);
            Assert.AreEqual("i8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(getMethod.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (getMethod.ReturnType as PrimitiveType).Type);
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
            Assert.AreEqual("v12@0:4i8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            ParameterDeclaration @param = setMethod.Parameters[0];
            Assert.AreEqual("myProperty", @param.Name);
            Assert.IsInstanceOf<PrimitiveType>(@param.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (@param.Type as PrimitiveType).Type);

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<PrimitiveType>(property.Type);
            Assert.AreEqual(PrimitiveTypeType.Int, (property.Type as PrimitiveType).Type);
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsFalse(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsFalse(property.IsWeak);
        }

        [Test]
        public void VisitClassWithReadonlyProperty()
        {
            string declaration = @"@interface SimpleClass : NSObject
	                                    @property (readonly) double myProperty;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("myProperty", getMethod.Name);
            Assert.AreEqual("d8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(getMethod.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Double, (getMethod.ReturnType as PrimitiveType).Type);
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.IsNull(property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<PrimitiveType>(property.Type);
            Assert.AreEqual(PrimitiveTypeType.Double, (property.Type as PrimitiveType).Type);
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsFalse(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsTrue(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsFalse(property.IsWeak);
        }

        [Test]
        public void VisitClassForwardDeclaration()
        {
            string declaration = @"@class MyOtherClass;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitClassForwardDeclaration2()
        {
            string declaration = @"@class MyOtherClass;
                                   @class MyOtherClass;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitClassForwardDeclarationFollowedByActualClass()
        {
            string declaration = @"@class MyOtherClass;
                                   @interface MyOtherClass : NSObject
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("MyOtherClass", classDeclaration.Name);
            Assert.AreEqual(2, classDeclaration.Location.Line);
            Assert.IsTrue(classDeclaration.IsContainer);
        }

        [Test]
        public void VisitClassForwardDeclarationFollowedByActualClass2()
        {
            string declaration = @"@class MyOtherClass;
                                   @class MyOtherClass;
                                   @interface MyOtherClass : NSObject
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("MyOtherClass", classDeclaration.Name);
            Assert.AreEqual(3, classDeclaration.Location.Line);
            Assert.IsTrue(classDeclaration.IsContainer);
        }

        [Test]
        public void VisitClassWithUnresolvedDeclaration()
        {
            string declaration = @"@class MyOtherClass;
                                   @interface SimpleClass : NSObject
                                       - (void) fooWithMyOtherClass: (MyOtherClass *) param;
                                   @end
                                   @interface MyOtherClass : SimpleClass
                                      - (int) anotherMethod;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[1]);
            InterfaceDeclaration classDeclaration2 = document.Declarations[1] as InterfaceDeclaration;
            Assert.AreEqual("MyOtherClass", classDeclaration2.Name);
            Assert.AreEqual(1, classDeclaration2.Methods.Count);
            Assert.AreEqual(5, classDeclaration2.Location.Line);

            MethodDeclaration method = classDeclaration2.Methods.ElementAt(0);
            Assert.AreEqual("anotherMethod", method.Name);
            Assert.AreEqual("i8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration2, method.Parent);

            method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithMyOtherClass:", method.Name);
            Assert.AreEqual("v12@0:4@8", method.TypeEncoding);
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
            Assert.IsInstanceOf<PointerType>(@param.Type);
            Assert.IsInstanceOf<DeclarationReferenceType>((@param.Type as PointerType).Target);
            DeclarationReferenceType decl = (@param.Type as PointerType).Target as DeclarationReferenceType;
            Assert.AreSame(classDeclaration2, decl.Target);
        }

        [Test]
        public void VisitClassWith2UnresolvedDeclaration()
        {
            string declaration = @"@class MyOtherClass;
                                   @interface SimpleClass : NSObject
                                       - (void) fooWithMyOtherClass: (MyOtherClass *) param andAgain: (MyOtherClass *) param2;
                                   @end
                                   @interface MyOtherClass : SimpleClass
                                      - (int) anotherMethod;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            MetadataGeneratorHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("SimpleClass", classDeclaration.Name);
            Assert.AreEqual(1, classDeclaration.Methods.Count);
            Assert.IsTrue(classDeclaration.IsContainer);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[1]);
            InterfaceDeclaration classDeclaration2 = document.Declarations[1] as InterfaceDeclaration;
            Assert.AreEqual("MyOtherClass", classDeclaration2.Name);
            Assert.AreEqual(1, classDeclaration2.Methods.Count);

            MethodDeclaration method = classDeclaration2.Methods.ElementAt(0);
            Assert.AreEqual("anotherMethod", method.Name);
            Assert.AreEqual("i8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(classDeclaration2, method.Parent);

            method = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("fooWithMyOtherClass:andAgain:", method.Name);
            Assert.AreEqual("v16@0:4@8@12", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(2, method.Parameters.Count);
            Assert.AreSame(classDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("param", @param.Name);
            Assert.IsInstanceOf<PointerType>(@param.Type);
            Assert.IsInstanceOf<DeclarationReferenceType>((@param.Type as PointerType).Target);
            DeclarationReferenceType decl = (@param.Type as PointerType).Target as DeclarationReferenceType;
            Assert.AreSame(classDeclaration2, decl.Target);

            @param = method.Parameters[1];
            Assert.AreEqual("param2", @param.Name);
            Assert.IsInstanceOf<PointerType>(@param.Type);
            Assert.IsInstanceOf<DeclarationReferenceType>((@param.Type as PointerType).Target);
            decl = (@param.Type as PointerType).Target as DeclarationReferenceType;
            Assert.AreSame(classDeclaration2, decl.Target);
        }
    }
}
