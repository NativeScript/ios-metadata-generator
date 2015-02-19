using Libclang.Core.Ast;
using Libclang.Core.Parser;
using Libclang.Core.Types;
using NUnit.Framework;
using System.Linq;

namespace Libclang.Tests
{
    [TestFixture]
    public class ProtocolTests
    {
        [Test]
        public void VisitSimpleProtocol()
        {
            string declaration = @"@protocol SimpleProtocol
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(0, protocolDeclaration.Methods.Count);
        }

        [Test]
        public void VisitSimpleProtocol2()
        {
            string declaration = @"@protocol SimpleProtocol
                                   @end
                                   @protocol SimpleProtocol;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(0, protocolDeclaration.Methods.Count);
        }

        [Test]
        public void VisitInheritedProtocol()
        {
            string declaration = @"@protocol SimpleProtocol
                                   @end
                                   @protocol SimpleProtocol2 <SimpleProtocol>
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(0, protocolDeclaration.Methods.Count);

            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[1]);
            ProtocolDeclaration protocolDeclaration2 = document.Declarations[1] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol2", protocolDeclaration2.Name);
            Assert.IsTrue(protocolDeclaration2.IsContainer);
            Assert.AreEqual(0, protocolDeclaration2.Methods.Count);
            Assert.AreEqual(1, protocolDeclaration2.ImplementedProtocols.Count);
            Assert.AreSame(protocolDeclaration, protocolDeclaration2.ImplementedProtocols.ElementAt(0));
        }

        [Test]
        public void VisitInheritedProtocol2()
        {
            string declaration = @"@protocol SimpleProtocol;
                                   @protocol SimpleProtocol2 <SimpleProtocol>
                                   @end
                                   @protocol SimpleProtocol
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[1]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[1] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(0, protocolDeclaration.Methods.Count);

            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration2 = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("SimpleProtocol2", protocolDeclaration2.Name);
            Assert.IsTrue(protocolDeclaration2.IsContainer);
            Assert.AreEqual(0, protocolDeclaration2.Methods.Count);
            Assert.AreEqual(1, protocolDeclaration2.ImplementedProtocols.Count);
            Assert.AreSame(protocolDeclaration, protocolDeclaration2.ImplementedProtocols.ElementAt(0));
        }

        [Test]
        public void VisitProtocolWithInstanceMethod1()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (void) foo;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, method.Parent);
        }

        [Test]
        public void VisitProtocolWithInstanceMethod2()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (float) fooWithReturn;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, method.Parent);
        }

        [Test]
        public void VisitProtocolWithInstanceMethod3()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (void) fooWithParam: (int) param;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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

            Assert.AreSame(protocolDeclaration, method.Parent);
        }

        [Test]
        public void VisitProtocolWithInstanceMethod4()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (void) fooWithParam: (int) param secondParam: (double) param2;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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

            Assert.AreSame(protocolDeclaration, method.Parent);
        }

        [Test]
        public void VisitProtocolWithProperty()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    @property int myProperty;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(2, protocolDeclaration.Methods.Count);
            Assert.AreEqual(1, protocolDeclaration.Properties.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration getMethod = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = protocolDeclaration.Methods.ElementAt(1);
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

            Assert.AreSame(protocolDeclaration, setMethod.Parent);

            PropertyDeclaration property = protocolDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(protocolDeclaration, property.Parent);
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
        public void VisitProtocolWithReadonlyProperty()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    @property (readonly) double myProperty;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.AreEqual(1, protocolDeclaration.Properties.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration getMethod = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, getMethod.Parent);

            PropertyDeclaration property = protocolDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("myProperty", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.IsNull(property.Setter);
            Assert.AreSame(protocolDeclaration, property.Parent);
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
        public void VisitProtocolForwardDeclaration()
        {
            string declaration = @"@protocol MyOtherProtocol;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitProtocolForwardDeclaration2()
        {
            string declaration = @"@protocol MyOtherProtocol;
                                   @protocol MyOtherProtocol;";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(0, document.Declarations.Count);
        }

        [Test]
        public void VisitProtocolForwardDeclarationFollowedByActualProtocol()
        {
            string declaration = @"@protocol MyOtherProtocol;
                                   @protocol MyOtherProtocol
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("MyOtherProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(2, protocolDeclaration.Location.Line);
        }

        [Test]
        public void VisitProtocolForwardDeclarationFollowedByActualProtocol2()
        {
            string declaration = @"@protocol MyOtherProtocol;
                                   @protocol MyOtherProtocol;
                                   @protocol MyOtherProtocol
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[0]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual("MyOtherProtocol", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(3, protocolDeclaration.Location.Line);
        }

        [Test]
        public void VisitClassWithForwardDeclaredProtocol()
        {
            string declaration = @"@protocol AwesomeDelegate;
                                   @interface Awesome : NSObject
                                       @property (weak, nonatomic) id<AwesomeDelegate> delegate;
                                   @end
                                   @protocol AwesomeDelegate
                                       - (void)awesome:(Awesome *)awesome;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(2, document.Declarations.Count);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("Awesome", classDeclaration.Name);
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);

            Assert.IsInstanceOf<ProtocolDeclaration>(document.Declarations[1]);
            ProtocolDeclaration protocolDeclaration = document.Declarations[1] as ProtocolDeclaration;
            Assert.AreEqual("AwesomeDelegate", protocolDeclaration.Name);
            Assert.IsTrue(protocolDeclaration.IsContainer);
            Assert.AreEqual(1, protocolDeclaration.Methods.Count);
            Assert.AreEqual(5, protocolDeclaration.Location.Line);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("awesome:", method.Name);
            Assert.AreEqual("v12@0:4@8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(protocolDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("awesome", @param.Name);
            Assert.IsInstanceOf<PointerType>(@param.Type);
            Assert.IsInstanceOf<DeclarationReferenceType>((@param.Type as PointerType).Target);
            DeclarationReferenceType decl = (@param.Type as PointerType).Target as DeclarationReferenceType;
            Assert.AreSame(classDeclaration, decl.Target);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("delegate", getMethod.Name);
            Assert.AreEqual("@8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<IdType>(getMethod.ReturnType);
            Assert.AreEqual(1, (getMethod.ReturnType as IdType).ImplementedProtocols.Count);
            Assert.AreSame(protocolDeclaration, (getMethod.ReturnType as IdType).ImplementedProtocols.ElementAt(0));
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = classDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("setDelegate:", setMethod.Name);
            Assert.AreEqual("v12@0:4@8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            param = setMethod.Parameters[0];
            Assert.AreEqual("delegate", @param.Name);
            Assert.IsInstanceOf<IdType>(@param.Type);
            Assert.AreEqual(1, (@param.Type as IdType).ImplementedProtocols.Count);
            Assert.AreSame(protocolDeclaration, (@param.Type as IdType).ImplementedProtocols.ElementAt(0));

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("delegate", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<IdType>(property.Type);
            Assert.AreEqual(1, (property.Type as IdType).ImplementedProtocols.Count);
            Assert.AreSame(protocolDeclaration, (property.Type as IdType).ImplementedProtocols.ElementAt(0));
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsTrue(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsTrue(property.IsWeak);
        }

        [Test]
        public void VisitClassWithForwardDeclaredProtocol2()
        {
            string declaration = @"@protocol AwesomeDelegate;
                                   @interface Awesome : NSObject
                                       @property (weak, nonatomic) Class<AwesomeDelegate> delegate;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual("Awesome", classDeclaration.Name);
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("delegate", getMethod.Name);
            Assert.AreEqual("#8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<ClassType>(getMethod.ReturnType);
            Assert.AreEqual(1, (getMethod.ReturnType as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(
                (getMethod.ReturnType as ClassType).ImplementedProtocols.ElementAt(0));
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = classDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("setDelegate:", setMethod.Name);
            Assert.AreEqual("v12@0:4#8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            ParameterDeclaration param = setMethod.Parameters[0];
            Assert.AreEqual("delegate", @param.Name);
            Assert.IsInstanceOf<ClassType>(@param.Type);
            Assert.AreEqual(1, (@param.Type as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((@param.Type as ClassType).ImplementedProtocols.ElementAt(0));

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("delegate", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<ClassType>(property.Type);
            Assert.AreEqual(1, (property.Type as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((property.Type as ClassType).ImplementedProtocols.ElementAt(0));
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsTrue(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsTrue(property.IsWeak);
        }

        [Test]
        public void VisitClassWith2ForwardDeclaredProtocols()
        {
            string declaration = @"@protocol AwesomeDelegate;
                                   @protocol AnotherAwesomeDelegate;
                                   @interface Awesome : NSObject
                                       @property (weak, nonatomic) id<AwesomeDelegate, AnotherAwesomeDelegate> delegate;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("Awesome", classDeclaration.Name);
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("delegate", getMethod.Name);
            Assert.AreEqual("@8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<IdType>(getMethod.ReturnType);
            Assert.AreEqual(2, (getMethod.ReturnType as IdType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((getMethod.ReturnType as IdType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>((getMethod.ReturnType as IdType).ImplementedProtocols.ElementAt(1));
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = classDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("setDelegate:", setMethod.Name);
            Assert.AreEqual("v12@0:4@8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            ParameterDeclaration param = setMethod.Parameters[0];
            Assert.AreEqual("delegate", @param.Name);
            Assert.IsInstanceOf<IdType>(@param.Type);
            Assert.AreEqual(2, (@param.Type as IdType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((@param.Type as IdType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>((@param.Type as IdType).ImplementedProtocols.ElementAt(1));

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("delegate", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<IdType>(property.Type);
            Assert.AreEqual(2, (property.Type as IdType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((property.Type as IdType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>((property.Type as IdType).ImplementedProtocols.ElementAt(1));
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsTrue(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsTrue(property.IsWeak);
        }

        [Test]
        public void VisitClassWith2ForwardDeclaredProtocols2()
        {
            string declaration = @"@protocol AwesomeDelegate;
                                   @protocol AnotherAwesomeDelegate;
                                   @interface Awesome : NSObject
                                       @property (weak, nonatomic) Class<AwesomeDelegate, AnotherAwesomeDelegate> delegate;
                                   @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);

            Assert.IsInstanceOf<InterfaceDeclaration>(document.Declarations[0]);
            InterfaceDeclaration classDeclaration = document.Declarations[0] as InterfaceDeclaration;
            Assert.AreEqual("Awesome", classDeclaration.Name);
            Assert.IsTrue(classDeclaration.IsContainer);
            Assert.AreEqual(2, classDeclaration.Methods.Count);
            Assert.AreEqual(1, classDeclaration.Properties.Count);

            MethodDeclaration getMethod = classDeclaration.Methods.ElementAt(0);
            Assert.AreEqual("delegate", getMethod.Name);
            Assert.AreEqual("#8@0:4", getMethod.TypeEncoding);
            Assert.IsInstanceOf<ClassType>(getMethod.ReturnType);
            Assert.AreEqual(2, (getMethod.ReturnType as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>(
                (getMethod.ReturnType as ClassType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>(
                (getMethod.ReturnType as ClassType).ImplementedProtocols.ElementAt(1));
            Assert.IsTrue(getMethod.IsImplicit);
            Assert.IsFalse(getMethod.IsConstructor);
            Assert.IsFalse(getMethod.IsOptional);
            Assert.IsFalse(getMethod.IsStatic);
            Assert.IsFalse(getMethod.IsVariadic);
            Assert.AreEqual(0, getMethod.Overrides.Count);
            Assert.AreEqual(0, getMethod.Parameters.Count);
            Assert.AreSame(classDeclaration, getMethod.Parent);

            MethodDeclaration setMethod = classDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("setDelegate:", setMethod.Name);
            Assert.AreEqual("v12@0:4#8", setMethod.TypeEncoding);
            Assert.IsTrue(setMethod.IsImplicit);
            Assert.IsFalse(setMethod.IsConstructor);
            Assert.IsFalse(setMethod.IsOptional);
            Assert.IsFalse(setMethod.IsStatic);
            Assert.IsFalse(setMethod.IsVariadic);
            Assert.AreEqual(0, setMethod.Overrides.Count);
            Assert.AreEqual(1, setMethod.Parameters.Count);

            ParameterDeclaration param = setMethod.Parameters[0];
            Assert.AreEqual("delegate", @param.Name);
            Assert.IsInstanceOf<ClassType>(@param.Type);
            Assert.AreEqual(2, (@param.Type as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((@param.Type as ClassType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>((@param.Type as ClassType).ImplementedProtocols.ElementAt(1));

            Assert.AreSame(classDeclaration, setMethod.Parent);

            PropertyDeclaration property = classDeclaration.Properties.ElementAt(0);
            Assert.AreEqual("delegate", property.Name);
            Assert.AreSame(getMethod, property.Getter);
            Assert.AreSame(setMethod, property.Setter);
            Assert.AreSame(classDeclaration, property.Parent);
            Assert.IsInstanceOf<ClassType>(property.Type);
            Assert.AreEqual(2, (property.Type as ClassType).ImplementedProtocols.Count);
            Assert.IsInstanceOf<ProtocolDeclaration>((property.Type as ClassType).ImplementedProtocols.ElementAt(0));
            Assert.IsInstanceOf<ProtocolDeclaration>((property.Type as ClassType).ImplementedProtocols.ElementAt(1));
            Assert.IsFalse(property.HasCustomGetter);
            Assert.IsFalse(property.HasCustomSetter);
            Assert.IsFalse(property.IsAssign);
            Assert.IsFalse(property.IsAtomic);
            Assert.IsFalse(property.IsCopy);
            Assert.IsTrue(property.IsNonatomic);
            Assert.IsFalse(property.IsOptional);
            Assert.IsFalse(property.IsReadonly);
            Assert.IsFalse(property.IsReadwrite);
            Assert.IsFalse(property.IsRetain);
            Assert.IsFalse(property.IsStrong);
            Assert.IsFalse(property.IsUnsafeUnretained);
            Assert.IsTrue(property.IsWeak);
        }

        [Test]
        public void VisitProtocolWithOptionalMethod()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (void) foo;
                                        @optional
                                        -(void)someMethod:(id)someArgument;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(2, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, method.Parent);

            method = protocolDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("someMethod:", method.Name);
            Assert.AreEqual("v12@0:4@8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsTrue(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(protocolDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("someArgument", @param.Name);
            Assert.IsInstanceOf<IdType>(@param.Type);
        }

        [Test]
        public void VisitProtocolWithOptionalMethod2()
        {
            string declaration = @"@protocol SimpleProtocol
	                                    - (void) foo;
                                        @optional
                                        -(void)someMethod:(id)someArgument;
                                        @required
                                        - (int) foo2;
                                    @end";

            ModuleDeclaration document = new ModuleDeclaration("test");
            ObjCDeclarationVisitor visitor = new ObjCDeclarationVisitor(document);

            LibclangHelper.ParseCodeWithVisitor(declaration, visitor);

            Assert.AreEqual(1, document.Declarations.Count);
            ProtocolDeclaration protocolDeclaration = document.Declarations[0] as ProtocolDeclaration;
            Assert.AreEqual(3, protocolDeclaration.Methods.Count);
            Assert.IsTrue(protocolDeclaration.IsContainer);

            MethodDeclaration method = protocolDeclaration.Methods.ElementAt(0);
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
            Assert.AreSame(protocolDeclaration, method.Parent);

            method = protocolDeclaration.Methods.ElementAt(1);
            Assert.AreEqual("someMethod:", method.Name);
            Assert.AreEqual("v12@0:4@8", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Void, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsTrue(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreSame(protocolDeclaration, method.Parent);

            ParameterDeclaration @param = method.Parameters[0];
            Assert.AreEqual("someArgument", @param.Name);
            Assert.IsInstanceOf<IdType>(@param.Type);

            method = protocolDeclaration.Methods.ElementAt(2);
            Assert.AreEqual("foo2", method.Name);
            Assert.AreEqual("i8@0:4", method.TypeEncoding);
            Assert.IsInstanceOf<PrimitiveType>(method.ReturnType);
            Assert.AreEqual(PrimitiveTypeType.Int, (method.ReturnType as PrimitiveType).Type);
            Assert.IsFalse(method.IsConstructor);
            Assert.IsFalse(method.IsOptional);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.IsVariadic);
            Assert.AreEqual(0, method.Overrides.Count);
            Assert.AreEqual(0, method.Parameters.Count);
            Assert.AreSame(protocolDeclaration, method.Parent);
        }
    }
}
