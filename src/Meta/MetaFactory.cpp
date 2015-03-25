#include "MetaFactory.h"
#include <llvm/Support/Casting.h>
#include <sstream>

using namespace std;

bool protocolsComparerByJsName(Meta::FQName& protocol1, Meta::FQName& protocol2) { return (protocol1.jsName < protocol2.jsName); }

bool methodsComparerByJsName(std::shared_ptr<Meta::MethodMeta>& method1, std::shared_ptr<Meta::MethodMeta>& method2) { return (method1->jsName < method2->jsName); }

bool propertiesComparerByJsName(std::shared_ptr<Meta::PropertyMeta>& property1, std::shared_ptr<Meta::PropertyMeta>& property2) { return (property1->jsName < property2->jsName); }

shared_ptr<Meta::Meta> Meta::MetaFactory::create(clang::Decl& decl) {
    try {
        if (clang::FunctionDecl *function = clang::dyn_cast<clang::FunctionDecl>(&decl))
            return createFromFunction(*function);
        else if (clang::RecordDecl *record = clang::dyn_cast<clang::RecordDecl>(&decl))
            return createFromRecord(*record);
        else if (clang::VarDecl *var = clang::dyn_cast<clang::VarDecl>(&decl))
            return createFromVar(*var);
        else if (clang::EnumDecl *enumDecl = clang::dyn_cast<clang::EnumDecl>(&decl))
            return createFromEnum(*enumDecl);
        else if (clang::ObjCInterfaceDecl *ineterface = clang::dyn_cast<clang::ObjCInterfaceDecl>(&decl))
            return createFromInterface(*ineterface);
        else if (clang::ObjCProtocolDecl *protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(&decl))
            return createFromProtocol(*protocol);
        else if (clang::ObjCCategoryDecl *category = clang::dyn_cast<clang::ObjCCategoryDecl>(&decl))
            return createFromCategory(*category);
        else if (clang::ObjCMethodDecl *method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl))
            return createFromMethod(*method);
        else if (clang::ObjCPropertyDecl *property = clang::dyn_cast<clang::ObjCPropertyDecl>(&decl))
            return createFromProperty(*property);
        else
            throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(decl), "Unknow declaration type.", true);
    }
    catch(IdentifierCreationException& e) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(decl), string("[") + e.whatAsString() + string("]"), true);
    }
    catch(TypeCreationException & e) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(decl), string("[") + e.whatAsString() + string("]"), e.isError());
    }
}

shared_ptr<Meta::FunctionMeta> Meta::MetaFactory::createFromFunction(clang::FunctionDecl& function) {
    // TODO: We don't support variadic functions but we save in metadata flags whether a function is variadic or not.
    // If we not plan in the future to support variadic functions this redundant flag should be removed.
    if (function.isVariadic())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(function), "The function is variadic.", false);

    shared_ptr<FunctionMeta> functionMeta = make_shared<FunctionMeta>();
    populateMetaFields(function, *(functionMeta.get()));
    functionMeta->setFlags(MetaFlags::FunctionIsVariadic, function.isVariadic());
    functionMeta->setFlags(MetaFlags::FunctionOwnsReturnedCocoaObject, this->getAttributes<clang::NSReturnsRetainedAttr>(function).size() > 0);
    functionMeta->signature.push_back(this->_typeEncodingFactory.create(function.getReturnType()));
    for (clang::FunctionDecl::param_const_iterator it = function.param_begin(); it != function.param_end(); ++it) {
        clang::ParmVarDecl *param = *it;
        functionMeta->signature.push_back(this->_typeEncodingFactory.create(param->getType()));
    }
    return functionMeta;
}

shared_ptr<Meta::RecordMeta> Meta::MetaFactory::createFromRecord(clang::RecordDecl& record) {
    if(record.isUnion())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(record), "The record is union.", false);
    if(!record.isStruct())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(record), "The record is not a struct.", false);
    shared_ptr<RecordMeta> recordMeta(record.isStruct() ? (RecordMeta*)(new StructMeta()) : (RecordMeta*)(new UnionMeta()));
    populateMetaFields(record, *(recordMeta.get()));
    for(clang::RecordDecl::field_iterator it = record.field_begin(); it != record.field_end(); ++it) {
        clang::FieldDecl *field = *it;
        RecordField recordField(_identifierGenerator.getJsName(*field), this->_typeEncodingFactory.create(field->getType()));
        recordMeta->fields.push_back(recordField);
    }
    return recordMeta;
}

std::shared_ptr<Meta::VarMeta> Meta::MetaFactory::createFromVar(clang::VarDecl& var) {
    shared_ptr<VarMeta> varMeta = make_shared<VarMeta>();
    populateMetaFields(var, *(varMeta.get()));
    varMeta->signature = this->_typeEncodingFactory.create(var.getType());
    return varMeta;
}

std::shared_ptr<Meta::JsCodeMeta> Meta::MetaFactory::createFromEnum(clang::EnumDecl& enumeration) {
    std::ostringstream stringStream;
    stringStream << "__tsEnum(";
    bool isFirstField = true;
    for (clang::EnumDecl::enumerator_iterator it = enumeration.enumerator_begin(); it != enumeration.enumerator_end() ; ++it) {
        clang::EnumConstantDecl *enumField = *it;
        llvm::SmallVector<char, 10> value;
        enumField->getInitVal().toString(value, 10, enumField->getInitVal().isSigned());
        stringStream << (isFirstField ? "" : ",") << "\"" << _identifierGenerator.getJsName(*enumField) << "\":" << std::string(value.data(), value.size());
        isFirstField = false;
    }
    stringStream << ")";
    std::string jsCode = stringStream.str();
    shared_ptr<JsCodeMeta> jsCodeMeta = make_shared<JsCodeMeta>();
    populateMetaFields(enumeration, *(jsCodeMeta.get()));
    return jsCodeMeta;
}

shared_ptr<Meta::InterfaceMeta> Meta::MetaFactory::createFromInterface(clang::ObjCInterfaceDecl& interface) {
    shared_ptr<InterfaceMeta> interfaceMeta = make_shared<InterfaceMeta>();
    populateMetaFields(interface, *(interfaceMeta.get()));
    populateBaseClassMetaFields(interface, *(interfaceMeta.get()));

    // set base interface
    clang::ObjCInterfaceDecl *super = interface.getSuperClass();
    interfaceMeta->baseName = (super == nullptr) ? FQName() : _identifierGenerator.getFqName(*super);

    return interfaceMeta;
}

shared_ptr<Meta::ProtocolMeta> Meta::MetaFactory::createFromProtocol(clang::ObjCProtocolDecl& protocol) {
    shared_ptr<ProtocolMeta> protocolMeta = make_shared<ProtocolMeta>();
    populateMetaFields(protocol, *(protocolMeta.get()));
    populateBaseClassMetaFields(protocol, *(protocolMeta.get()));
    return protocolMeta;
}

shared_ptr<Meta::CategoryMeta> Meta::MetaFactory::createFromCategory(clang::ObjCCategoryDecl& category) {
    shared_ptr<CategoryMeta> categoryMeta = make_shared<CategoryMeta>();
    populateMetaFields(category, *(categoryMeta.get()));
    populateBaseClassMetaFields(category, *(categoryMeta.get()));
    return categoryMeta;
}

shared_ptr<Meta::MethodMeta> Meta::MetaFactory::createFromMethod(clang::ObjCMethodDecl& method) {
    // TODO: Check if method->hasRelatedResultType() is true and replace the return type with instancetype
    shared_ptr<MethodMeta> methodMeta = make_shared<MethodMeta>();
    populateMetaFields(method, *(methodMeta.get()));

    // set selector
    // TODO: We can use the name property instead of selector and remove the selector property.
    methodMeta->selector = method.getSelector().getAsString();

    // set type encoding
    // TODO: Remove type encodings. We don't need them any more.
    this->_astUnit->getASTContext().getObjCEncodingForMethodDecl(&method, methodMeta->typeEncoding, false);

    // set IsVariadic flag
    methodMeta->setFlags(MetaFlags::MethodIsVariadic, method.isVariadic());

    // set MethodIsNilTerminatedVariadic
    methodMeta->setFlags(MetaFlags::MethodIsNullTerminatedVariadic, method.isVariadic() && this->getAttributes<clang::SentinelAttr>(method).size() > 0);

    // set MethodOwnsReturnedCocoaObject flag
    bool nsReturnsRetainedAttr = this->getAttributes<clang::NSReturnsRetainedAttr>(method).size() > 0;
    bool nsReturnsNotRetainedAttr = this->getAttributes<clang::NSReturnsNotRetainedAttr>(method).size() > 0;
    if(nsReturnsRetainedAttr && nsReturnsNotRetainedAttr)
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(method), "Method has both NS_Returns_Retained and NS_Returns_Not_Retained attributes.", true);
    else if(nsReturnsRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  true);
    else if(nsReturnsNotRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  false);
    else {
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, false);
        vector<string> selectorBegins = { "alloc", "new", "copy", "mutableCopy" };
        for (int i = 0; i < selectorBegins.size(); ++i) {
            if(selectorBegins[i].compare(0, methodMeta->selector.size(), methodMeta->selector)) {
                methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, true);
                break;
            }
        }
    }

    return methodMeta;
}

shared_ptr<Meta::PropertyMeta> Meta::MetaFactory::createFromProperty(clang::ObjCPropertyDecl& property) {
    shared_ptr<PropertyMeta> propertyMeta = make_shared<PropertyMeta>();
    populateMetaFields(property, *(propertyMeta.get()));

    clang::ObjCMethodDecl *getter = property.getGetterMethodDecl();
    propertyMeta->setFlags(MetaFlags::PropertyHasGetter, getter);
    propertyMeta->getter = getter ? static_pointer_cast<MethodMeta>(create(*getter)) : nullptr;

    clang::ObjCMethodDecl *setter = property.getSetterMethodDecl();
    propertyMeta->setFlags(MetaFlags::PropertyHasSetter, setter);
    propertyMeta->setter = setter ? static_pointer_cast<MethodMeta>(create(*setter)) : nullptr;

    return propertyMeta;
}

void Meta::MetaFactory::populateMetaFields(clang::NamedDecl& decl, Meta& meta) {
    meta.declaration = &decl;
    meta.name = decl.getNameAsString();
    FQName fqName = this->_identifierGenerator.getFqName(decl);
    meta.jsName = fqName.jsName;
    meta.module = fqName.module;
    meta.setFlags(MetaFlags::HasName , meta.name != meta.jsName);

    clang::AvailabilityAttr *iosAvailability = nullptr;
    clang::AvailabilityAttr *iosExtensionsAvailability = nullptr;

    // Traverse attributes
    vector<clang::AvailabilityAttr*> availabilityAttr = this->getAttributes<clang::AvailabilityAttr>(decl);
    for (vector<clang::AvailabilityAttr*>::iterator i = availabilityAttr.begin(); i != availabilityAttr.end(); ++i) {
        clang::AvailabilityAttr *availability = *i;
        string platform = availability->getPlatform()->getName().str();
        if(platform == string("ios")) {
            iosAvailability = availability;
        }
        else if(platform == string("ios_app_extension")) {
            iosExtensionsAvailability = availability;
        }
    }

    /*
        TODO: If a declaration is unavailable for iOS we automatically consider it unavailable for iOS Extensions
        and remove it from metadata. This may not be the case. Maybe a declaration can be unavailable for iOS but
        still available for iOS Extensions. In this case we should include the declaration in metadata and mark it as
        unavailable for iOS (no matter which iOS version).

        TODO: We are considering a declaration to be unavailable for iOS Extensions if it has
        ios_app_extension availability attribute and its unavailable property is set to true.
        This is not quite right because the availability attribute contains much more information such as
        Introduced, Deprecated, Obsolated properties which are not considered. The possible solution is to
        save information in metadata about all these properties (this is what we do for iOS Availability attribute).

        Maybe we can change availability format to some more clever alternative.
     */
    if(iosAvailability) {
        if(iosAvailability->getUnavailable()) {
            throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(decl), "The declaration is marked unvailable for ios platform (with availability attribute).", false);
        }
        meta.introducedIn = this->convertVersion(iosAvailability->getIntroduced());
        meta.deprecatedIn = this->convertVersion(iosAvailability->getDeprecated());
        meta.obsoletedIn = this->convertVersion(iosAvailability->getObsoleted());
    }
    bool isIosExtensionsAvailable = iosExtensionsAvailability == nullptr || !iosExtensionsAvailability->getUnavailable();
    meta.setFlags(MetaFlags::IsIosAppExtensionAvailable , isIosExtensionsAvailable);
}

void Meta::MetaFactory::populateBaseClassMetaFields(clang::ObjCContainerDecl& decl, BaseClassMeta& baseClass) {
    llvm::iterator_range<clang::ObjCProtocolList::iterator> protocols = this->getProtocols(&decl);
    for (clang::ObjCProtocolList::iterator i = protocols.begin(); i != protocols.end() ; ++i) {
        clang::ObjCProtocolDecl *protocol = *i;
        baseClass.protocols.push_back(_identifierGenerator.getFqName(*protocol));
    }
    std::sort(baseClass.protocols.begin(), baseClass.protocols.end(), protocolsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::classmeth_iterator i = decl.classmeth_begin(); i != decl.classmeth_end(); ++i) {
        clang::ObjCMethodDecl& classMethod = **i;
        baseClass.staticMethods.push_back(static_pointer_cast<MethodMeta>(this->create(classMethod)));
    }
    std::sort(baseClass.staticMethods.begin(), baseClass.staticMethods.end(), methodsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::instmeth_iterator i = decl.instmeth_begin(); i != decl.instmeth_end(); ++i) {
        clang::ObjCMethodDecl& instanceMethod = **i;
        baseClass.instanceMethods.push_back(static_pointer_cast<MethodMeta>(this->create(instanceMethod)));
    }
    std::sort(baseClass.instanceMethods.begin(), baseClass.instanceMethods.end(), methodsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::prop_iterator i = decl.prop_begin(); i != decl.prop_end(); ++i) {
        clang::ObjCPropertyDecl& property = **i;
        baseClass.properties.push_back(static_pointer_cast<PropertyMeta>(this->create(property)));
    }
    std::sort(baseClass.properties.begin(), baseClass.properties.end(), propertiesComparerByJsName); // order by jsName
}

llvm::iterator_range<clang::ObjCProtocolList::iterator> Meta::MetaFactory::getProtocols(clang::ObjCContainerDecl* objCContainer) {
    if(clang::ObjCInterfaceDecl* interface = clang::dyn_cast<clang::ObjCInterfaceDecl>(objCContainer))
        return interface->protocols();
    else if(clang::ObjCProtocolDecl* protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(objCContainer))
        return protocol->protocols();
    else if(clang::ObjCCategoryDecl* category = clang::dyn_cast<clang::ObjCCategoryDecl>(objCContainer))
        return category->protocols();
    throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(*objCContainer), "Unable to extract protocols form this type of ObjC container.", true);
}

Meta::Version Meta::MetaFactory::convertVersion(clang::VersionTuple clangVersion) {
    Version result = {
            .Major = (int)clangVersion.getMajor(),
            .Minor = (int)(clangVersion.getMinor().hasValue() ? clangVersion.getMinor().getValue() : -1),
            .SubMinor = (int)(clangVersion.getSubminor().hasValue() ? clangVersion.getSubminor().getValue() : -1)
    };
    return result;
}