using Libclang.Core.Ast;
using Libclang.Core.Parser;
using Libclang.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace Libclang.Tests
{
    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void VisitSimpleCategory()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass (SimpleClassExtention)
                                   @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<CategoryDeclaration>(document.Declarations[1]);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(0, categoryDeclaration.Methods.Count);
            Assert.AreEqual(0, categoryDeclaration.Properties.Count);
            Assert.IsFalse(categoryDeclaration.IsClassExtension);
        }

        [Test]
        public void VisitClassExtensionCategory()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass ()
                                   @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<CategoryDeclaration>(document.Declarations[1]);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(0, categoryDeclaration.Methods.Count);
            Assert.AreEqual(0, categoryDeclaration.Properties.Count);
            Assert.IsTrue(categoryDeclaration.IsClassExtension);
        }

        [Test]
        public void VisitSimpleCategoryWithInstanceMethod1()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass (SimpleClassExtention)
	                                    - (void) foo;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.Methods.Count);

            MethodDeclaration method = categoryDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(categoryDeclaration, method.Parent);
        }

        [Test]
        public void VisitSimpleCategoryWithInstanceMethod2()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass (SimpleClassExtention)
	                                    - (float) fooWithReturn;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.Methods.Count);

            MethodDeclaration method = categoryDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(categoryDeclaration, method.Parent);
        }

        [Test]
        public void VisitSimpleCategoryWithInstanceMethod3()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass (SimpleClassExtention)
	                                    - (void) fooWithParam: (int) param;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.Methods.Count);

            MethodDeclaration method = categoryDeclaration.Methods.ElementAt(0);
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

            Assert.AreSame(categoryDeclaration, method.Parent);
        }

        [Test]
        public void VisitSimpleCategoryWithInstanceMethod4()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass (SimpleClassExtention)
	                                    - (void) fooWithParam: (int) param secondParam: (double) param2;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.Methods.Count);

            MethodDeclaration method = categoryDeclaration.Methods.ElementAt(0);
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

            Assert.AreSame(categoryDeclaration, method.Parent);
        }

        [Test]
        public void VisitClassExtensionCategoryWithProperty()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass ()
	                                    @property int myProperty;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("", categoryDeclaration.Name);
            Assert.IsTrue(categoryDeclaration.IsClassExtension);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(2, categoryDeclaration.Methods.Count);
            Assert.AreEqual(1, categoryDeclaration.Properties.Count);

            MethodDeclaration getMethod = categoryDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(categoryDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = categoryDeclaration.Methods.ElementAt(1);
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

            Assert.AreSame(categoryDeclaration, setMethod.Parent);

            PropertyDeclaration property = categoryDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(categoryDeclaration, property.Parent);
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
        public void VisitClassExtensionCategoryWithReadonlyProperty()
        {
            string declaration = @"@interface SimpleClass : NSObject
                                   @end

                                   @interface SimpleClass ()
	                                    @property (readonly) double myProperty;
                                    @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            CategoryDeclaration categoryDeclaration = document.Declarations[1] as CategoryDeclaration;
            Assert.AreEqual("", categoryDeclaration.Name);
            Assert.IsTrue(categoryDeclaration.IsClassExtension);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.Methods.Count);
            Assert.AreEqual(1, categoryDeclaration.Properties.Count);

            MethodDeclaration getMethod = categoryDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(categoryDeclaration, getMethod.Parent);

            PropertyDeclaration property = categoryDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.IsNull(property.Setter);
            Assert.AreSame(categoryDeclaration, property.Parent);
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
        public void VisitCategoryWithImplementedProtocol()
        {
            string declaration = @"@protocol MyProtocol <NSObject>
                                   @end
                                   @interface SimpleClass : NSObject
                                   @end
                                   @interface SimpleClass (SimpleClassExtention) <MyProtocol>
                                   @end";

            DocumentDeclaration document = new DocumentDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(3, document.Declarations.Count);
            Assert.IsInstanceOf<CategoryDeclaration>(document.Declarations[2]);
            CategoryDeclaration categoryDeclaration = document.Declarations[2] as CategoryDeclaration;
            Assert.AreEqual("SimpleClassExtention", categoryDeclaration.Name);
            Assert.AreSame(document.Declarations[1], categoryDeclaration.ExtendedInterface);
            Assert.AreEqual(1, categoryDeclaration.ExtendedInterface.Categories.Count);
            Assert.AreSame(categoryDeclaration, categoryDeclaration.ExtendedInterface.Categories.ElementAt(0));
            Assert.AreEqual(1, categoryDeclaration.ImplementedProtocols.Count);
            Assert.AreSame(document.Declarations[0], categoryDeclaration.ImplementedProtocols.ElementAt(0));
            Assert.AreEqual(0, categoryDeclaration.Methods.Count);
            Assert.AreEqual(0, categoryDeclaration.Properties.Count);
            Assert.IsFalse(categoryDeclaration.IsClassExtension);
        }
    }
}
