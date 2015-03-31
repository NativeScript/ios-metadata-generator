#include "MetaFactory.h"
#include <llvm/Support/Casting.h>
#include <sstream>
#include <algorithm>
#include "Utils.h"

using namespace std;

bool protocolsComparerByJsName(Meta::FQName& protocol1, Meta::FQName& protocol2) {
    string name1 = protocol1.jsName;
    string name2 = protocol2.jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower) < std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

bool methodsComparerByJsName(std::shared_ptr<Meta::MethodMeta>& method1, std::shared_ptr<Meta::MethodMeta>& method2) {
    string name1 = method1->jsName;
    string name2 = method2->jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower) < std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

bool propertiesComparerByJsName(std::shared_ptr<Meta::PropertyMeta>& property1, std::shared_ptr<Meta::PropertyMeta>& property2) {
    string name1 = property1->jsName;
    string name2 = property2->jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower) < std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

bool stringBeginsWith(std::string& str, std::vector<std::string>& possibleBegins) {
    for (int i = 0; i < possibleBegins.size(); ++i) {
        if(possibleBegins[i].length() <= str.length() && str.compare(0, possibleBegins[i].size(), possibleBegins[i]) == 0) {
            return true;
        }
    }
    return false;
}

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
        else if (clang::EnumConstantDecl *enumConstantDecl = clang::dyn_cast<clang::EnumConstantDecl>(&decl))
            return createFromEnumConstant(*enumConstantDecl);
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
    if(function.isThisDeclarationADefinition()) {
        // The function is defined in headers
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(function), "The function is defined in headers.", false);
    }

    // TODO: We don't support variadic functions but we save in metadata flags whether a function is variadic or not.
    // If we not plan in the future to support variadic functions this redundant flag should be removed.
    if (function.isVariadic())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(function), "The function is variadic.", false);

    shared_ptr<FunctionMeta> functionMeta = make_shared<FunctionMeta>();
    populateMetaFields(function, *(functionMeta.get()));

    // set IsVariadic
    functionMeta->setFlags(MetaFlags::FunctionIsVariadic, function.isVariadic());

    // set OwnsReturnedCocoaObjects
    functionMeta->setFlags(MetaFlags::FunctionOwnsReturnedCocoaObject, Utils::getAttributes<clang::NSReturnsRetainedAttr>(function).size() > 0);

    // set signature
    functionMeta->signature.push_back(this->_typeFactory.create(function.getReturnType()));
    for (clang::FunctionDecl::param_const_iterator it = function.param_begin(); it != function.param_end(); ++it) {
        clang::ParmVarDecl *param = *it;
        functionMeta->signature.push_back(this->_typeFactory.create(param->getType()));
    }

    return functionMeta;
}

shared_ptr<Meta::RecordMeta> Meta::MetaFactory::createFromRecord(clang::RecordDecl& record) {
    if(!record.isThisDeclarationADefinition()) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(record), "A formard declaration of record.", false);
    }

    if(record.isUnion())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(record), "The record is union.", false);
    if(!record.isStruct())
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(record), "The record is not a struct.", false);

    shared_ptr<RecordMeta> recordMeta(record.isStruct() ? (RecordMeta*)(new StructMeta()) : (RecordMeta*)(new UnionMeta()));
    populateMetaFields(record, *(recordMeta.get()));

    // set fields
    for(clang::RecordDecl::field_iterator it = record.field_begin(); it != record.field_end(); ++it) {
        clang::FieldDecl *field = *it;
        RecordField recordField(_identifierGenerator.getJsName(*field), this->_typeFactory.create(field->getType()));
        recordMeta->fields.push_back(recordField);
    }

    return recordMeta;
}

std::shared_ptr<Meta::VarMeta> Meta::MetaFactory::createFromVar(clang::VarDecl& var) {
    if(var.getKind() != clang::Decl::Kind::Var) {
        // It is not exactly a VarDecl but an inheritor of VarDecl (e.g. ParmVarDecl)
        throw MetaCreationException(Identifier(var.getNameAsString(), "", ""), "Not a var declaration.", false);
    }
    if(var.getLexicalDeclContext() != var.getASTContext().getTranslationUnitDecl()) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(var), "A nested var.", false);
    }

    shared_ptr<VarMeta> varMeta = make_shared<VarMeta>();
    populateMetaFields(var, *(varMeta.get()));

    //set type
    varMeta->signature = this->_typeFactory.create(var.getType());
    return varMeta;
}

std::shared_ptr<Meta::JsCodeMeta> Meta::MetaFactory::createFromEnum(clang::EnumDecl& enumeration) {
    if(!enumeration.isThisDeclarationADefinition()) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(enumeration), "Froward declaration of enum.", false);
    }

    std::ostringstream jsCodeStream;
    jsCodeStream << "__tsEnum({";
    bool isFirstField = true;
    for (clang::EnumDecl::enumerator_iterator it = enumeration.enumerator_begin(); it != enumeration.enumerator_end() ; ++it) {
        clang::EnumConstantDecl *enumField = *it;
        llvm::SmallVector<char, 10> value;
        enumField->getInitVal().toString(value, 10, enumField->getInitVal().isSigned());
        jsCodeStream << (isFirstField ? "" : ",") << "\"" << _identifierGenerator.getJsName(*enumField) << "\":" << std::string(value.data(), value.size());
        isFirstField = false;
    }
    jsCodeStream << "})";
    shared_ptr<JsCodeMeta> jsCodeMeta = make_shared<JsCodeMeta>();
    populateMetaFields(enumeration, *(jsCodeMeta.get()));
    jsCodeMeta->jsCode = jsCodeStream.str();
    return jsCodeMeta;
}

std::shared_ptr<Meta::JsCodeMeta> Meta::MetaFactory::createFromEnumConstant(clang::EnumConstantDecl& enumConstant) {
    if(clang::EnumDecl *parentEnum = clang::dyn_cast<clang::EnumDecl>(enumConstant.getLexicalDeclContext())) {
        if(!parentEnum->hasNameForLinkage()) {
            shared_ptr<JsCodeMeta> jsCodeMeta = make_shared<JsCodeMeta>();
            populateMetaFields(enumConstant, *(jsCodeMeta.get()));
            llvm::SmallVector<char, 10> value;
            enumConstant.getInitVal().toString(value, 10, enumConstant.getInitVal().isSigned());
            jsCodeMeta->jsCode = std::string(value.data(), value.size());
            return jsCodeMeta;
        }
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(enumConstant), "The containing enum is not anonymous.", false);
    }
    throw std::runtime_error("Invalid enum constant declaration.");
}

shared_ptr<Meta::InterfaceMeta> Meta::MetaFactory::createFromInterface(clang::ObjCInterfaceDecl& interface) {
    if(!interface.isThisDeclarationADefinition()) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(interface), "A forward declaration of interface.", false);
    }

    shared_ptr<InterfaceMeta> interfaceMeta = make_shared<InterfaceMeta>();
    populateMetaFields(interface, *(interfaceMeta.get()));
    populateBaseClassMetaFields(interface, *(interfaceMeta.get()));

    // set base interface
    clang::ObjCInterfaceDecl *super = interface.getSuperClass();
    interfaceMeta->baseName = (super == nullptr) ? FQName() : _identifierGenerator.getFqName(*super);

    return interfaceMeta;
}

shared_ptr<Meta::ProtocolMeta> Meta::MetaFactory::createFromProtocol(clang::ObjCProtocolDecl& protocol) {
    if(!protocol.isThisDeclarationADefinition()) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(protocol), "A forward declaration of protocol.", false);
    }

    shared_ptr<ProtocolMeta> protocolMeta = make_shared<ProtocolMeta>();
    populateMetaFields(protocol, *(protocolMeta.get()));
    populateBaseClassMetaFields(protocol, *(protocolMeta.get()));
    return protocolMeta;
}

shared_ptr<Meta::CategoryMeta> Meta::MetaFactory::createFromCategory(clang::ObjCCategoryDecl& category) {
    shared_ptr<CategoryMeta> categoryMeta = make_shared<CategoryMeta>();
    populateMetaFields(category, *(categoryMeta.get()));
    populateBaseClassMetaFields(category, *(categoryMeta.get()));
    categoryMeta->extendedInterface = _identifierGenerator.getFqName(*category.getClassInterface());
    return categoryMeta;
}

shared_ptr<Meta::MethodMeta> Meta::MetaFactory::createFromMethod(clang::ObjCMethodDecl& method) {
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
    bool isNullTerminatedVariadic = method.isVariadic() && Utils::getAttributes<clang::SentinelAttr>(method).size() > 0;
    methodMeta->setFlags(MetaFlags::MethodIsNullTerminatedVariadic, isNullTerminatedVariadic);

    if(method.isVariadic() && !isNullTerminatedVariadic)
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(method), "Method is variadic (and is not marked as nil terminated.).", false);

    // set MethodOwnsReturnedCocoaObject flag
    bool nsReturnsRetainedAttr = Utils::getAttributes<clang::NSReturnsRetainedAttr>(method).size() > 0;
    bool nsReturnsNotRetainedAttr = Utils::getAttributes<clang::NSReturnsNotRetainedAttr>(method).size() > 0;
    if(nsReturnsRetainedAttr && nsReturnsNotRetainedAttr)
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(method), "Method has both NS_Returns_Retained and NS_Returns_Not_Retained attributes.", true);
    else if(nsReturnsRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  true);
    else if(nsReturnsNotRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  false);
    else {
        vector<string> selectorBegins = { "alloc", "new", "copy", "mutableCopy" };
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, stringBeginsWith(methodMeta->selector, selectorBegins));
    }

    // set signature
    methodMeta->signature.push_back(method.hasRelatedResultType() ? Type::Instancetype() : this->_typeFactory.create(method.getReturnType()));
    for (clang::FunctionDecl::param_const_iterator it = method.param_begin(); it != method.param_end(); ++it) {
        clang::ParmVarDecl *param = *it;
        methodMeta->signature.push_back(this->_typeFactory.create(param->getType()));
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
    bool hasUnavailableAttr = Utils::getAttributes<clang::UnavailableAttr>(decl).size() > 0;
    if(hasUnavailableAttr) {
        throw MetaCreationException(_identifierGenerator.getIdentifierOrEmpty(decl), "The declaration is marked unvailable (with unavailable attribute).", false);
    }
    vector<clang::AvailabilityAttr*> availabilityAttr = Utils::getAttributes<clang::AvailabilityAttr>(decl);
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
        try {
            baseClass.protocols.push_back(_identifierGenerator.getFqName(*protocol));
        } catch(IdentifierCreationException& e) {
            continue;
        }
    }
    std::sort(baseClass.protocols.begin(), baseClass.protocols.end(), protocolsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::classmeth_iterator i = decl.classmeth_begin(); i != decl.classmeth_end(); ++i) {
        clang::ObjCMethodDecl& classMethod = **i;
        if(!classMethod.isImplicit()) {
            try {
                baseClass.staticMethods.push_back(static_pointer_cast<MethodMeta>(this->create(classMethod)));
            } catch (MetaCreationException& e) {
                continue;
            }
        }
    }
    std::sort(baseClass.staticMethods.begin(), baseClass.staticMethods.end(), methodsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::instmeth_iterator i = decl.instmeth_begin(); i != decl.instmeth_end(); ++i) {
        clang::ObjCMethodDecl& instanceMethod = **i;
        if(!instanceMethod.isImplicit()) {
            try {
                baseClass.instanceMethods.push_back(static_pointer_cast<MethodMeta>(this->create(instanceMethod)));
            } catch(MetaCreationException& e) {
                continue;
            }
        }
    }
    std::sort(baseClass.instanceMethods.begin(), baseClass.instanceMethods.end(), methodsComparerByJsName); // order by jsName

    for (clang::ObjCContainerDecl::prop_iterator i = decl.prop_begin(); i != decl.prop_end(); ++i) {
        clang::ObjCPropertyDecl& property = **i;
        try {
            baseClass.properties.push_back(static_pointer_cast<PropertyMeta>(this->create(property)));
        } catch(MetaCreationException& e) {
            continue;
        }
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