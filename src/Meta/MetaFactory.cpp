#include "MetaFactory.h"
#include <llvm/Support/Casting.h>
#include <sstream>
#include <algorithm>
#include "Utils.h"

using namespace std;

bool protocolsComparerByJsName(Meta::Identifier& protocol1, Meta::Identifier& protocol2) {
    string name1 = protocol1.jsName;
    string name2 = protocol2.jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower);
    std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

bool methodsComparerByJsName(std::shared_ptr<Meta::MethodMeta>& method1, std::shared_ptr<Meta::MethodMeta>& method2) {
    string name1 = method1->id.jsName;
    string name2 = method2->id.jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower);
    std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

bool propertiesComparerByJsName(std::shared_ptr<Meta::PropertyMeta>& property1, std::shared_ptr<Meta::PropertyMeta>& property2) {
    string name1 = property1->id.jsName;
    string name2 = property2->id.jsName;
    std::transform(name1.begin(), name1.end(), name1.begin(), ::tolower);
    std::transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
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
    // check for cached Meta
    std::unordered_map<const clang::Decl*, std::shared_ptr<Meta>>::const_iterator cachedId = _cache.find(&decl);
    if(cachedId != _cache.end()) {
        std::shared_ptr<Meta> result = cachedId->second;
        if(result)
            return result;
        throw MetaCreationException(_delegate->getId(decl, false), "Unable to create metadata.", false); // The meta object cannot be created
    }
    else if(std::find(_metaCreationStack.begin(), _metaCreationStack.end(), &decl) != _metaCreationStack.end()) {
        throw runtime_error(std::string("Attempt to create the same meta object recursively(") + _delegate->getId(decl, false).jsName + ").");
    }

    _metaCreationStack.push_back(&decl); // add to creation stack
    std::shared_ptr<Meta> result(nullptr);
    try {
        if (clang::FunctionDecl *function = clang::dyn_cast<clang::FunctionDecl>(&decl))
            result = createFromFunction(*function);
        else if (clang::RecordDecl *record = clang::dyn_cast<clang::RecordDecl>(&decl))
            result = createFromRecord(*record);
        else if (clang::VarDecl *var = clang::dyn_cast<clang::VarDecl>(&decl))
            result = createFromVar(*var);
        else if (clang::EnumDecl *enumDecl = clang::dyn_cast<clang::EnumDecl>(&decl))
            result = createFromEnum(*enumDecl);
        else if (clang::EnumConstantDecl *enumConstantDecl = clang::dyn_cast<clang::EnumConstantDecl>(&decl))
            result = createFromEnumConstant(*enumConstantDecl);
        else if (clang::ObjCInterfaceDecl *ineterface = clang::dyn_cast<clang::ObjCInterfaceDecl>(&decl))
            result = createFromInterface(*ineterface);
        else if (clang::ObjCProtocolDecl *protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(&decl))
            result = createFromProtocol(*protocol);
        else if (clang::ObjCCategoryDecl *category = clang::dyn_cast<clang::ObjCCategoryDecl>(&decl))
            result = createFromCategory(*category);
        else if (clang::ObjCMethodDecl *method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl))
            result = createFromMethod(*method);
        else if (clang::ObjCPropertyDecl *property = clang::dyn_cast<clang::ObjCPropertyDecl>(&decl))
            result = createFromProperty(*property);
        else
            throw MetaCreationException(_delegate->getId(decl, false), "Unknow declaration type.", true);

        _metaCreationStack.pop_back(); // remove from creation stack
        _cache.insert(std::pair<const clang::Decl*, std::shared_ptr<Meta>>(&decl, result)); // add to cache
        return result;
    }
    catch(IdentifierCreationException& e) {
        _metaCreationStack.pop_back(); // remove from creation stack
        _cache.insert(std::pair<const clang::Decl*, std::shared_ptr<Meta>>(&decl, result)); // add to cache
        throw MetaCreationException(_delegate->getId(decl, false), string("[") + e.whatAsString() + string("]"), true);
    }
    catch(TypeCreationException & e) {
        _metaCreationStack.pop_back(); // remove from creation stack
        _cache.insert(std::pair<const clang::Decl*, std::shared_ptr<Meta>>(&decl, result)); // add to cache
        throw MetaCreationException(_delegate->getId(decl, false), string("[") + e.whatAsString() + string("]"), e.isError());
    }
    catch(MetaCreationException & e) {
        _metaCreationStack.pop_back(); // remove from creation stack
        _cache.insert(std::pair<const clang::Decl*, std::shared_ptr<Meta>>(&decl, result)); // add to cache
        throw;
    }
}

clang::Decl& Meta::MetaFactory::ensureCanBeCreated(clang::Decl& decl) {
    if(std::find(_metaCreationStack.begin(), _metaCreationStack.end(), &decl) == _metaCreationStack.end()) {
        create(decl);
    }
    return decl;
}

shared_ptr<Meta::FunctionMeta> Meta::MetaFactory::createFromFunction(clang::FunctionDecl& function) {
    if(function.isThisDeclarationADefinition()) {
        // The function is defined in headers
        throw MetaCreationException(_delegate->getId(function, false), "The function is defined in headers.", false);
    }

    // TODO: We don't support variadic functions but we save in metadata flags whether a function is variadic or not.
    // If we not plan in the future to support variadic functions this redundant flag should be removed.
    if (function.isVariadic())
        throw MetaCreationException(_delegate->getId(function, false), "The function is variadic.", false);

    shared_ptr<FunctionMeta> functionMeta = make_shared<FunctionMeta>();
    populateMetaFields(function, *(functionMeta.get()));

    // set IsVariadic
    functionMeta->setFlags(MetaFlags::FunctionIsVariadic, function.isVariadic());

    // set OwnsReturnedCocoaObjects
    functionMeta->setFlags(MetaFlags::FunctionOwnsReturnedCocoaObject, Utils::getAttributes<clang::NSReturnsRetainedAttr>(function).size() > 0);

    // set signature
    functionMeta->signature.push_back(_delegate->getType(function.getReturnType()));
    for (clang::FunctionDecl::param_const_iterator it = function.param_begin(); it != function.param_end(); ++it) {
        clang::ParmVarDecl *param = *it;
        functionMeta->signature.push_back(_delegate->getType(param->getType()));
    }

    return functionMeta;
}

shared_ptr<Meta::RecordMeta> Meta::MetaFactory::createFromRecord(clang::RecordDecl& record) {
    if(!record.isThisDeclarationADefinition()) {
        throw MetaCreationException(_delegate->getId(record, false), "A formard declaration of record.", false);
    }

    if(record.isUnion())
        throw MetaCreationException(_delegate->getId(record, false), "The record is union.", false);
    if(!record.isStruct())
        throw MetaCreationException(_delegate->getId(record, false), "The record is not a struct.", false);

    shared_ptr<RecordMeta> recordMeta(record.isStruct() ? (RecordMeta*)(new StructMeta()) : (RecordMeta*)(new UnionMeta()));
    populateMetaFields(record, *(recordMeta.get()));

    // set fields
    for(clang::RecordDecl::field_iterator it = record.field_begin(); it != record.field_end(); ++it) {
        clang::FieldDecl *field = *it;
        RecordField recordField(_delegate->getId(*field, true).jsName, _delegate->getType(field->getType()));
        recordMeta->fields.push_back(recordField);
    }
    return recordMeta;
}

std::shared_ptr<Meta::VarMeta> Meta::MetaFactory::createFromVar(clang::VarDecl& var) {
    if(var.getLexicalDeclContext() != var.getASTContext().getTranslationUnitDecl()) {
        throw MetaCreationException(_delegate->getId(var, false), "A nested var.", false);
    }

    shared_ptr<VarMeta> varMeta = make_shared<VarMeta>();
    populateMetaFields(var, *(varMeta.get()));

    //set type
    varMeta->signature = _delegate->getType(var.getType());
    return varMeta;
}

std::shared_ptr<Meta::JsCodeMeta> Meta::MetaFactory::createFromEnum(clang::EnumDecl& enumeration) {
    if(!enumeration.isThisDeclarationADefinition()) {
        throw MetaCreationException(_delegate->getId(enumeration, false), "Froward declaration of enum.", false);
    }

    std::vector<std::string> fieldNames;
    for (clang::EnumDecl::enumerator_iterator it = enumeration.enumerator_begin(); it != enumeration.enumerator_end() ; ++it)
        fieldNames.push_back((*it)->getNameAsString());
    size_t fieldNamePrefixLength = Utils::getCommonWordPrefix(fieldNames).length();

    std::ostringstream jsCodeStream;
    jsCodeStream << "__tsEnum({";
    bool isFirstField = true;
    for (clang::EnumDecl::enumerator_iterator it = enumeration.enumerator_begin(); it != enumeration.enumerator_end() ; ++it) {
        clang::EnumConstantDecl *enumField = *it;
        llvm::SmallVector<char, 10> value;
        enumField->getInitVal().toString(value, 10, enumField->getInitVal().isSigned());
        std::string valueStr = std::string(value.data(), value.size());
        if(fieldNamePrefixLength > 0)
            jsCodeStream << (isFirstField ? "" : ",") << "\"" << enumField->getNameAsString().substr(fieldNamePrefixLength, std::string::npos) << "\":" << valueStr;
        jsCodeStream << "," << "\"" << enumField->getNameAsString() << "\":" << valueStr;
        isFirstField = false;
    }
    jsCodeStream << "})";
    shared_ptr<JsCodeMeta> jsCodeMeta = make_shared<JsCodeMeta>();
    populateMetaFields(enumeration, *(jsCodeMeta.get()));
    jsCodeMeta->jsCode = jsCodeStream.str();
    return jsCodeMeta;
}

std::shared_ptr<Meta::JsCodeMeta> Meta::MetaFactory::createFromEnumConstant(clang::EnumConstantDecl& enumConstant) {
    shared_ptr<JsCodeMeta> jsCodeMeta = make_shared<JsCodeMeta>();
    populateMetaFields(enumConstant, *(jsCodeMeta.get()));
    llvm::SmallVector<char, 10> value;
    enumConstant.getInitVal().toString(value, 10, enumConstant.getInitVal().isSigned());
    jsCodeMeta->jsCode = std::string(value.data(), value.size());
    return jsCodeMeta;
}

shared_ptr<Meta::InterfaceMeta> Meta::MetaFactory::createFromInterface(clang::ObjCInterfaceDecl& interface) {
    if(!interface.isThisDeclarationADefinition()) {
        throw MetaCreationException(_delegate->getId(interface, false), "A forward declaration of interface.", false);
    }

    shared_ptr<InterfaceMeta> interfaceMeta = make_shared<InterfaceMeta>();
    populateMetaFields(interface, *(interfaceMeta.get()));
    populateBaseClassMetaFields(interface, *(interfaceMeta.get()));

    // set base interface
    clang::ObjCInterfaceDecl *super = interface.getSuperClass();
    interfaceMeta->base = (super == nullptr) ? Identifier() : _delegate->getId(ensureCanBeCreated(*super->getDefinition()), true);

    return interfaceMeta;
}

shared_ptr<Meta::ProtocolMeta> Meta::MetaFactory::createFromProtocol(clang::ObjCProtocolDecl& protocol) {
    if(!protocol.isThisDeclarationADefinition()) {
        throw MetaCreationException(_delegate->getId(protocol, false), "A forward declaration of protocol.", false);
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
    categoryMeta->extendedInterface = _delegate->getId(ensureCanBeCreated(*category.getClassInterface()->getDefinition()), true);
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
    methodMeta->typeEncoding = "";

    // set IsVariadic flag
    methodMeta->setFlags(MetaFlags::MethodIsVariadic, method.isVariadic());

    // set MethodIsNilTerminatedVariadic
    bool isNullTerminatedVariadic = method.isVariadic() && Utils::getAttributes<clang::SentinelAttr>(method).size() > 0;
    methodMeta->setFlags(MetaFlags::MethodIsNullTerminatedVariadic, isNullTerminatedVariadic);

    if(method.isVariadic() && !isNullTerminatedVariadic)
        throw MetaCreationException(_delegate->getId(method, false), "Method is variadic (and is not marked as nil terminated.).", false);

    // set MethodOwnsReturnedCocoaObject flag
    bool nsReturnsRetainedAttr = Utils::getAttributes<clang::NSReturnsRetainedAttr>(method).size() > 0;
    bool nsReturnsNotRetainedAttr = Utils::getAttributes<clang::NSReturnsNotRetainedAttr>(method).size() > 0;
    if(nsReturnsRetainedAttr && nsReturnsNotRetainedAttr)
        throw MetaCreationException(_delegate->getId(method, false), "Method has both NS_Returns_Retained and NS_Returns_Not_Retained attributes.", true);
    else if(nsReturnsRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  true);
    else if(nsReturnsNotRetainedAttr)
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  false);
    else {
        vector<string> selectorBegins = { "alloc", "new", "copy", "mutableCopy" };
        methodMeta->setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, stringBeginsWith(methodMeta->selector, selectorBegins));
    }

    // set signature
    methodMeta->signature.push_back(method.hasRelatedResultType() ? Type::Instancetype() : _delegate->getType(method.getReturnType()));
    for (clang::FunctionDecl::param_const_iterator it = method.param_begin(); it != method.param_end(); ++it) {
        clang::ParmVarDecl *param = *it;
        methodMeta->signature.push_back(_delegate->getType(param->getType()));
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
    // TODO: add identifier in meta object
    meta.declaration = &decl;
    // We allow  anonymous categories to be created. There is no need for categories to be named
    // because we don't keep them as separate entity in metadata. They are merged in their interfaces
    meta.id = this->_delegate->getId(decl, !meta.is(MetaType::Category));
    meta.setFlags(MetaFlags::HasName , meta.id.name != meta.id.jsName);

    clang::AvailabilityAttr *iosAvailability = nullptr;
    clang::AvailabilityAttr *iosExtensionsAvailability = nullptr;

    // Traverse attributes
    bool hasUnavailableAttr = Utils::getAttributes<clang::UnavailableAttr>(decl).size() > 0;
    if(hasUnavailableAttr) {
        throw MetaCreationException(_delegate->getId(decl, false), "The declaration is marked unvailable (with unavailable attribute).", false);
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
            throw MetaCreationException(_delegate->getId(decl, false), "The declaration is marked unvailable for ios platform (with availability attribute).", false);
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
            baseClass.protocols.push_back(_delegate->getId(ensureCanBeCreated(*protocol->getDefinition()), true));
        } catch(MetaCreationException& e) {
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
    throw MetaCreationException(_delegate->getId(*objCContainer, false), "Unable to extract protocols form this type of ObjC container.", true);
}

Meta::Version Meta::MetaFactory::convertVersion(clang::VersionTuple clangVersion) {
    Version result = {
            .Major = (int)clangVersion.getMajor(),
            .Minor = (int)(clangVersion.getMinor().hasValue() ? clangVersion.getMinor().getValue() : -1),
            .SubMinor = (int)(clangVersion.getSubminor().hasValue() ? clangVersion.getSubminor().getValue() : -1)
    };
    return result;
}