#include "MetaFactory.h"
#include <sstream>
#include "Utils.h"
#include "CreationException.h"

using namespace std;

namespace Meta {

static bool compareJsNames(string& protocol1, string& protocol2)
{
    string name1 = protocol1;
    string name2 = protocol2;
    transform(name1.begin(), name1.end(), name1.begin(), ::tolower);
    transform(name2.begin(), name2.end(), name2.begin(), ::tolower);
    return name1 < name2;
}

static bool metasComparerByJsName(Meta* meta1, Meta* meta2)
{
    return compareJsNames(meta1->jsName, meta2->jsName);
}

static void splitString(const std::string& s, char delim, vector<string>& elems)
{
    stringstream ss(s);
    string item;
    while (getline(ss, item, delim)) {
        elems.push_back(item);
    }
}

static string getTypedefOrOwnName(const clang::TagDecl* tagDecl)
{
    assert(tagDecl);

    if (!tagDecl->hasNameForLinkage()) {
        return ""; // It is absolutely anonymous decl. It has neither name nor typedef name.
    }

    if (tagDecl->getNextDeclInContext() != nullptr) {
        if (const clang::TypedefDecl* nextDecl = clang::dyn_cast<clang::TypedefDecl>(tagDecl->getNextDeclInContext())) {
            if (const clang::ElaboratedType* innerElaboratedType = clang::dyn_cast<clang::ElaboratedType>(nextDecl->getUnderlyingType().getTypePtr())) {
                if (const clang::TagType* tagType = clang::dyn_cast<clang::TagType>(innerElaboratedType->desugar().getTypePtr())) {
                    if (tagType->getDecl() == tagDecl) {
                        return nextDecl->getFirstDecl()->getNameAsString();
                    }
                }
            }
        }
    }

    // The decl has no typedef name, so we return its name.
    return tagDecl->getNameAsString();
}

Meta* MetaFactory::create(const clang::Decl& decl)
{

    // check for cached Meta
    Cache::const_iterator cachedMetaIt = _cache.find(&decl);
    if (cachedMetaIt != _cache.end()) {
        Meta* meta = cachedMetaIt->second.first.get();
        std::string errorMessage = cachedMetaIt->second.second;
        if (errorMessage.empty()) {
            /* TODO: The meta object is not guaranteed to be fully initialized. If the meta object is in the creation stack
                 * it will appear in cache, but will not be fully initialized. This may cause some inconsistent results.
                 * */
            return meta;
        }
        throw MetaCreationException(meta, errorMessage, false);
    }

    std::pair<Cache::iterator, bool> insertionResult = _cache.insert(std::make_pair<Cache::key_type, Cache::mapped_type>(&decl, std::make_pair<std::unique_ptr<Meta>, std::string>(nullptr, std::string())));
    assert(insertionResult.second);
    std::unique_ptr<Meta>& metaPtr = insertionResult.first->second.first;
    try {
        if (const clang::FunctionDecl* function = clang::dyn_cast<clang::FunctionDecl>(&decl)) {
            metaPtr.reset(new FunctionMeta());
            populateIdentificationFields(*function, *metaPtr.get());
            createFromFunction(*function, metaPtr.get()->as<FunctionMeta>());
        }
        else if (const clang::RecordDecl* record = clang::dyn_cast<clang::RecordDecl>(&decl)) {
            if (record->isStruct()) {
                metaPtr.reset(new StructMeta());
                populateIdentificationFields(*record, *metaPtr.get());
                createFromStruct(*record, metaPtr.get()->as<StructMeta>());
            }
            else {
                metaPtr.reset(new UnionMeta());
                populateIdentificationFields(*record, *metaPtr.get());
                throw MetaCreationException(metaPtr.get(), "The record is union.", false);
            }
        }
        else if (const clang::VarDecl* var = clang::dyn_cast<clang::VarDecl>(&decl)) {
            metaPtr.reset(new VarMeta());
            populateIdentificationFields(*var, *metaPtr.get());
            createFromVar(*var, metaPtr.get()->as<VarMeta>());
        }
        else if (const clang::EnumDecl* enumDecl = clang::dyn_cast<clang::EnumDecl>(&decl)) {
            metaPtr.reset(new EnumMeta());
            populateIdentificationFields(*enumDecl, *metaPtr.get());
            createFromEnum(*enumDecl, metaPtr.get()->as<EnumMeta>());
        }
        else if (const clang::EnumConstantDecl* enumConstantDecl = clang::dyn_cast<clang::EnumConstantDecl>(&decl)) {
            metaPtr.reset(new EnumConstantMeta());
            populateIdentificationFields(*enumConstantDecl, *metaPtr.get());
            createFromEnumConstant(*enumConstantDecl, metaPtr.get()->as<EnumConstantMeta>());
        }
        else if (const clang::ObjCInterfaceDecl* interface = clang::dyn_cast<clang::ObjCInterfaceDecl>(&decl)) {
            metaPtr.reset(new InterfaceMeta());
            populateIdentificationFields(*interface, *metaPtr.get());
            createFromInterface(*interface, metaPtr.get()->as<InterfaceMeta>());
        }
        else if (const clang::ObjCProtocolDecl* protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(&decl)) {
            metaPtr.reset(new ProtocolMeta());
            populateIdentificationFields(*protocol, *metaPtr.get());
            createFromProtocol(*protocol, metaPtr.get()->as<ProtocolMeta>());
        }
        else if (const clang::ObjCCategoryDecl* category = clang::dyn_cast<clang::ObjCCategoryDecl>(&decl)) {
            metaPtr.reset(new CategoryMeta());
            populateIdentificationFields(*category, *metaPtr.get());
            createFromCategory(*category, metaPtr.get()->as<CategoryMeta>());
        }
        else if (const clang::ObjCMethodDecl* method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl)) {
            metaPtr.reset(new MethodMeta());
            populateIdentificationFields(*method, *metaPtr.get());
            createFromMethod(*method, metaPtr.get()->as<MethodMeta>());
        }
        else if (const clang::ObjCPropertyDecl* property = clang::dyn_cast<clang::ObjCPropertyDecl>(&decl)) {
            metaPtr.reset(new PropertyMeta());
            populateIdentificationFields(*property, *metaPtr.get());
            createFromProperty(*property, metaPtr.get()->as<PropertyMeta>());
        }
        else {
            throw logic_error("Unknown declaration type.");
        }

        return metaPtr.get();
    }
    catch (MetaCreationException& e) {
        if (e.getMeta() == metaPtr.get()) {
            insertionResult.first->second.second = e.getMessage();
            throw;
        }
        std::string message = CreationException::constructMessage("Can't create meta dependency.", e.getDetailedMessage());
        insertionResult.first->second.second = message;
        throw MetaCreationException(metaPtr.get(), message, e.isError());
    }
    catch (TypeCreationException& e) {
        std::string message = CreationException::constructMessage("Can't create type dependency.", e.getDetailedMessage());
        insertionResult.first->second.second = message;
        throw MetaCreationException(metaPtr.get(), message, e.isError());
    }
}

bool MetaFactory::tryCreate(const clang::Decl& decl, Meta** meta)
{
    try {
        Meta* result = this->create(decl);
        if (meta != nullptr) {
            *meta = result;
        }
        return true;
    }
    catch (CreationException& e) {
        return false;
    }
}

void MetaFactory::createFromFunction(const clang::FunctionDecl& function, FunctionMeta& functionMeta)
{
    if (function.isThisDeclarationADefinition()) {
        throw MetaCreationException(&functionMeta, "The function is defined in headers.", false);
    }

    // TODO: We don't support variadic functions but we save in metadata flags whether a function is variadic or not.
    // If we not plan in the future to support variadic functions this redundant flag should be removed.
    if (function.isVariadic())
        throw MetaCreationException(&functionMeta, "The function is variadic.", false);

    populateMetaFields(function, functionMeta);

    functionMeta.setFlags(MetaFlags::FunctionIsVariadic, function.isVariadic()); // set IsVariadic

    // set signature
    functionMeta.signature.push_back(_typeFactory.create(function.getReturnType()).get());
    for (clang::ParmVarDecl* param : function.parameters()) {
        functionMeta.signature.push_back(_typeFactory.create(param->getType()).get());
    }

    bool returnsRetained = function.hasAttr<clang::NSReturnsRetainedAttr>() || function.hasAttr<clang::CFReturnsRetainedAttr>();
    bool returnsNotRetained = function.hasAttr<clang::NSReturnsNotRetainedAttr>() || function.hasAttr<clang::CFReturnsNotRetainedAttr>();

    // Clang doesn't handle The Create Rule automatically like for methods, so we have to do it manually
    if (!(returnsRetained || returnsNotRetained) && functionMeta.signature[0]->is(TypeBridgedInterface)) {
        if (function.hasAttr<clang::CFAuditedTransferAttr>()) {
            std::string functionName = function.getNameAsString();
            if (functionName.find("Create") != string::npos || functionName.find("Copy") != string::npos) {
                returnsRetained = true;
            }
        }
        else {
            functionMeta.setFlags(MetaFlags::FunctionReturnsUnmanaged, true);
        }
    }

    functionMeta.setFlags(MetaFlags::FunctionOwnsReturnedCocoaObject, returnsRetained); // set OwnsReturnedCocoaObjects
}

void MetaFactory::createFromStruct(const clang::RecordDecl& record, StructMeta& structMeta)
{
    if (!record.isStruct())
        throw MetaCreationException(&structMeta, "The record is not a struct.", false);
    if (!record.isThisDeclarationADefinition()) {
        throw MetaCreationException(&structMeta, "A forward declaration of record.", false);
    }

    populateMetaFields(record, structMeta);

    // set fields
    for (clang::FieldDecl* field : record.fields()) {
        RecordField recordField(field->getNameAsString(), _typeFactory.create(field->getType()).get());
        structMeta.fields.push_back(recordField);
    }
}

void MetaFactory::createFromVar(const clang::VarDecl& var, VarMeta& varMeta)
{
    if (var.getLexicalDeclContext() != var.getASTContext().getTranslationUnitDecl()) {
        throw MetaCreationException(&varMeta, "A nested var.", false);
    }

    populateMetaFields(var, varMeta);
    //set type
    varMeta.signature = _typeFactory.create(var.getType()).get();
    varMeta.hasValue = false;

    if (var.hasInit()) {
        clang::APValue* evValue = var.evaluateValue();
        if (evValue == nullptr) {
            throw MetaCreationException(&varMeta, "Unable to evaluate compile-time constant value.", false);
        }

        varMeta.hasValue = true;
        llvm::SmallVector<char, 10> valueAsString;

        switch (evValue->getKind()) {
        case clang::APValue::ValueKind::Int:
            evValue->getInt().toString(valueAsString, 10, evValue->getInt().isSigned());
            break;
        case clang::APValue::ValueKind::Float:
            evValue->getFloat().toString(valueAsString);
            break;
        case clang::APValue::ValueKind::ComplexInt:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: ComplexInt.", false);
        case clang::APValue::ValueKind::ComplexFloat:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: ComplexFloat.", false);
        case clang::APValue::ValueKind::AddrLabelDiff:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: AddrLabelDiff.", false);
        case clang::APValue::ValueKind::Array:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: Array.", false);
        case clang::APValue::ValueKind::LValue:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: LValue.", false);
        case clang::APValue::ValueKind::MemberPointer:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: MemberPointer.", false);
        case clang::APValue::ValueKind::Struct:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: Struct.", false);
        case clang::APValue::ValueKind::Union:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: Union.", false);
        case clang::APValue::ValueKind::Vector:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: Vector.", false);
        case clang::APValue::ValueKind::Uninitialized:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: Uninitialized.", false);
        default:
            throw MetaCreationException(&varMeta, "Not supported compile-time constant value: -.", false);
        }

        varMeta.value = std::string(valueAsString.data(), valueAsString.size());
    }
}

void MetaFactory::createFromEnum(const clang::EnumDecl& enumeration, EnumMeta& enumMeta)
{
    if (!enumeration.isThisDeclarationADefinition()) {
        throw MetaCreationException(&enumMeta, "Forward declaration of enum.", false);
    }

    populateMetaFields(enumeration, enumMeta);

    std::vector<std::string> fieldNames;
    for (clang::EnumConstantDecl* enumField : enumeration.enumerators())
        fieldNames.push_back(enumField->getNameAsString());
    size_t fieldNamePrefixLength = Utils::calculateEnumFieldsPrefix(enumMeta.jsName, fieldNames).size();

    for (clang::EnumConstantDecl* enumField : enumeration.enumerators()) {
        llvm::SmallVector<char, 10> value;
        enumField->getInitVal().toString(value, 10, enumField->getInitVal().isSigned());
        std::string valueStr = std::string(value.data(), value.size());

        if (fieldNamePrefixLength > 0) {
            enumMeta.swiftNameFields.push_back({ enumField->getNameAsString().substr(fieldNamePrefixLength, std::string::npos), valueStr });
        }
        enumMeta.fullNameFields.push_back({ enumField->getNameAsString(), valueStr });
    }
}

void MetaFactory::createFromEnumConstant(const clang::EnumConstantDecl& enumConstant, EnumConstantMeta& enumMeta)
{
    populateMetaFields(enumConstant, enumMeta);
    llvm::SmallVector<char, 10> value;
    enumConstant.getInitVal().toString(value, 10, enumConstant.getInitVal().isSigned());
    enumMeta.value = std::string(value.data(), value.size());
}

void MetaFactory::createFromInterface(const clang::ObjCInterfaceDecl& interface, InterfaceMeta& interfaceMeta)
{
    if (!interface.isThisDeclarationADefinition()) {
        throw MetaCreationException(&interfaceMeta, "A forward declaration of interface.", false);
    }

    populateMetaFields(interface, interfaceMeta);
    populateBaseClassMetaFields(interface, interfaceMeta);

    // set base interface
    clang::ObjCInterfaceDecl* super = interface.getSuperClass();
    interfaceMeta.base = (super == nullptr) ? nullptr : &this->create(*super->getDefinition())->as<InterfaceMeta>();
}

void MetaFactory::createFromProtocol(const clang::ObjCProtocolDecl& protocol, ProtocolMeta& protocolMeta)
{
    if (!protocol.isThisDeclarationADefinition()) {
        throw MetaCreationException(&protocolMeta, "A forward declaration of protocol.", false);
    }

    populateMetaFields(protocol, protocolMeta);
    populateBaseClassMetaFields(protocol, protocolMeta);
}

void MetaFactory::createFromCategory(const clang::ObjCCategoryDecl& category, CategoryMeta& categoryMeta)
{
    populateMetaFields(category, categoryMeta);
    populateBaseClassMetaFields(category, categoryMeta);
    categoryMeta.extendedInterface = &this->create(*category.getClassInterface()->getDefinition())->as<InterfaceMeta>();
}

void MetaFactory::createFromMethod(const clang::ObjCMethodDecl& method, MethodMeta& methodMeta)
{
    populateMetaFields(method, methodMeta);

    methodMeta.setFlags(MetaFlags::MethodIsVariadic, method.isVariadic()); // set IsVariadic flag

    bool isNullTerminatedVariadic = method.isVariadic() && method.hasAttr<clang::SentinelAttr>(); // set MethodIsNilTerminatedVariadic flag
    methodMeta.setFlags(MetaFlags::MethodIsNullTerminatedVariadic, isNullTerminatedVariadic);

    // set MethodHasErrorOutParameter flag
    if (method.parameters().size() > 0) {
        clang::ParmVarDecl* lastParameter = method.parameters()[method.parameters().size() - 1];
        Type* type = _typeFactory.create(lastParameter->getType()).get();
        if (type->is(TypeType::TypePointer)) {
            Type* innerType = type->as<PointerType>().innerType;
            if (innerType->is(TypeType::TypeInterface) && innerType->as<InterfaceType>().interface->jsName == "NSError") {
                methodMeta.setFlags(MetaFlags::MethodHasErrorOutParameter, true);
            }
        }
    }

    methodMeta.setFlags(MetaFlags::MethodIsInitializer, method.getMethodFamily() == clang::ObjCMethodFamily::OMF_init); // set MethodIsInitializer flag

    if (method.isVariadic() && !isNullTerminatedVariadic)
        throw MetaCreationException(&methodMeta, "Method is variadic (and is not marked as nil terminated.).", false);

    // set MethodOwnsReturnedCocoaObject flag
    clang::ObjCMethodFamily methodFamily = method.getMethodFamily();
    switch (methodFamily) {
    case clang::ObjCMethodFamily::OMF_copy:
    //case clang::ObjCMethodFamily::OMF_init :
    //case clang::ObjCMethodFamily::OMF_alloc :
    case clang::ObjCMethodFamily::OMF_mutableCopy:
    case clang::ObjCMethodFamily::OMF_new: {
        bool hasNsReturnsNotRetainedAttr = method.hasAttr<clang::NSReturnsNotRetainedAttr>();
        bool hasCfReturnsNotRetainedAttr = method.hasAttr<clang::CFReturnsNotRetainedAttr>();
        methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, !(hasNsReturnsNotRetainedAttr || hasCfReturnsNotRetainedAttr));
        break;
    }
    default: {
        bool hasNsReturnsRetainedAttr = method.hasAttr<clang::NSReturnsRetainedAttr>();
        bool hasCfReturnsRetainedAttr = method.hasAttr<clang::CFReturnsRetainedAttr>();
        methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, hasNsReturnsRetainedAttr || hasCfReturnsRetainedAttr);
        break;
    }
    }

    // set signature
    methodMeta.signature.push_back(method.hasRelatedResultType() ? _typeFactory.getInstancetype().get() : _typeFactory.create(method.getReturnType()).get());
    for (clang::ParmVarDecl* param : method.parameters()) {
        methodMeta.signature.push_back(_typeFactory.create(param->getType()).get());
    }
}

void MetaFactory::createFromProperty(const clang::ObjCPropertyDecl& property, PropertyMeta& propertyMeta)
{
    populateMetaFields(property, propertyMeta);

    clang::ObjCMethodDecl* getter = property.getGetterMethodDecl();
    propertyMeta.getter = getter ? &create(*getter)->as<MethodMeta>() : nullptr;

    clang::ObjCMethodDecl* setter = property.getSetterMethodDecl();
    propertyMeta.setter = setter ? &create(*setter)->as<MethodMeta>() : nullptr;
}

void MetaFactory::populateIdentificationFields(const clang::NamedDecl& decl, Meta& meta)
{
    meta.declaration = &decl;
    // calculate name
    clang::ObjCRuntimeNameAttr* objCRuntimeNameAttribute = decl.getAttr<clang::ObjCRuntimeNameAttr>();
    meta.name = !objCRuntimeNameAttribute ? decl.getNameAsString() : objCRuntimeNameAttribute->getMetadataName().str();

    // calculate file name and module
    clang::SourceLocation location = _sourceManager.getFileLoc(decl.getLocation());
    clang::FileID fileId = _sourceManager.getDecomposedLoc(location).first;
    const clang::FileEntry* entry = _sourceManager.getFileEntryForID(fileId);
    if (entry != nullptr) {
        meta.fileName = entry->getName();
        meta.module = _headerSearch.findModuleForHeader(entry).getModule();
    }

    // calculate js name
    switch (decl.getKind()) {
    case clang::Decl::Kind::Function:
    case clang::Decl::Kind::ObjCInterface:
    case clang::Decl::Kind::ObjCProtocol:
    case clang::Decl::Kind::ObjCCategory:
    case clang::Decl::Kind::ObjCProperty:
    case clang::Decl::Kind::EnumConstant:
    case clang::Decl::Kind::Var:
        meta.jsName = decl.getNameAsString();
        break;
    case clang::Decl::Kind::ObjCMethod: {
        const clang::ObjCMethodDecl* method = clang::dyn_cast<clang::ObjCMethodDecl>(&decl);
        std::string selector = method->getSelector().getAsString();
        vector<string> tokens;
        splitString(selector, ':', tokens);
        for (vector<string>::size_type i = 1; i < tokens.size(); ++i) {
            tokens[i][0] = toupper(tokens[i][0]);
            tokens[0] += tokens[i];
        }
        meta.jsName = tokens[0];
        break;
    }
    case clang::Decl::Kind::Record:
    case clang::Decl::Kind::Enum: {
        const clang::TagDecl* tagDecl = clang::dyn_cast<clang::TagDecl>(&decl);
        meta.jsName = getTypedefOrOwnName(tagDecl);
        break;
    }
    default:
        throw logic_error(string("Can't generate jsName for ") + decl.getDeclKindName() + " type of declaration.");
    }

    // We allow  anonymous categories to be created. There is no need for categories to be named
    // because we don't keep them as separate entity in metadata. They are merged in their interfaces
    if (!meta.is(MetaType::Category)) {
        if (meta.fileName == "") {
            throw MetaCreationException(&meta, "Unknown file for declaration.", true);
        }
        else if (meta.module == nullptr) {
            throw MetaCreationException(&meta, "Unknown module for declaration.", false);
        }
        else if (meta.jsName == "") {
            throw MetaCreationException(&meta, "Anonymous declaration. Unable to calculate JS name.", false);
        }
    }
}

void MetaFactory::populateMetaFields(const clang::NamedDecl& decl, Meta& meta)
{
    clang::AvailabilityAttr* iosAvailability = nullptr;
    clang::AvailabilityAttr* iosExtensionsAvailability = nullptr;

    // Traverse attributes
    if (decl.hasAttr<clang::UnavailableAttr>()) {
        throw MetaCreationException(&meta, "The declaration is marked unavailable (with unavailable attribute).", false);
    }
    vector<clang::AvailabilityAttr*> availabilityAttributes = Utils::getAttributes<clang::AvailabilityAttr>(decl);
    for (clang::AvailabilityAttr* availability : availabilityAttributes) {
        string platform = availability->getPlatform()->getName().str();
        if (platform == string("ios")) {
            iosAvailability = availability;
        }
        else if (platform == string("ios_app_extension")) {
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
            Introduced, Deprecated, Obsoleted properties which are not considered. The possible solution is to
            save information in metadata about all these properties (this is what we do for iOS Availability attribute).

            Maybe we can change availability format to some more clever alternative.
         */
    if (iosAvailability) {
        if (iosAvailability->getUnavailable()) {
            throw MetaCreationException(&meta, "The declaration is marked unvailable for ios platform (with availability attribute).", false);
        }
        meta.introducedIn = this->convertVersion(iosAvailability->getIntroduced());
        meta.deprecatedIn = this->convertVersion(iosAvailability->getDeprecated());
        meta.obsoletedIn = this->convertVersion(iosAvailability->getObsoleted());
    }
    bool isIosExtensionsAvailable = iosExtensionsAvailability == nullptr || !iosExtensionsAvailability->getUnavailable();
    meta.setFlags(MetaFlags::IsIosAppExtensionAvailable, isIosExtensionsAvailable);
}

void MetaFactory::populateBaseClassMetaFields(const clang::ObjCContainerDecl& decl, BaseClassMeta& baseClass)
{
    for (clang::ObjCProtocolDecl* protocol : this->getProtocols(&decl)) {
        Meta* protocolMeta;
        if (this->tryCreate(*protocol->getDefinition(), &protocolMeta)) {
            baseClass.protocols.push_back(&protocolMeta->as<ProtocolMeta>());
        }
    }
    std::sort(baseClass.protocols.begin(), baseClass.protocols.end(), metasComparerByJsName); // order by jsName

    for (clang::ObjCMethodDecl* classMethod : decl.class_methods()) {
        Meta* methodMeta;
        if (!classMethod->isImplicit() && this->tryCreate(*classMethod, &methodMeta)) {
            baseClass.staticMethods.push_back(&methodMeta->as<MethodMeta>());
        }
    }
    std::sort(baseClass.staticMethods.begin(), baseClass.staticMethods.end(), metasComparerByJsName); // order by jsName

    for (clang::ObjCMethodDecl* instanceMethod : decl.instance_methods()) {
        Meta* methodMeta;
        if (!instanceMethod->isImplicit() && this->tryCreate(*instanceMethod, &methodMeta)) {
            baseClass.instanceMethods.push_back(&methodMeta->as<MethodMeta>());
        }
    }
    std::sort(baseClass.instanceMethods.begin(), baseClass.instanceMethods.end(), metasComparerByJsName); // order by jsName

    for (clang::ObjCPropertyDecl* property : decl.properties()) {
        Meta* propertyMeta;
        if (this->tryCreate(*property, &propertyMeta)) {
            baseClass.properties.push_back(&propertyMeta->as<PropertyMeta>());
        }
    }
    std::sort(baseClass.properties.begin(), baseClass.properties.end(), metasComparerByJsName); // order by jsName
}

llvm::iterator_range<clang::ObjCProtocolList::iterator> MetaFactory::getProtocols(const clang::ObjCContainerDecl* objCContainer)
{
    if (const clang::ObjCInterfaceDecl* interface = clang::dyn_cast<clang::ObjCInterfaceDecl>(objCContainer))
        return interface->protocols();
    else if (const clang::ObjCProtocolDecl* protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(objCContainer))
        return protocol->protocols();
    else if (const clang::ObjCCategoryDecl* category = clang::dyn_cast<clang::ObjCCategoryDecl>(objCContainer))
        return category->protocols();
    throw logic_error("Unable to extract protocols form this type of ObjC container.");
}

Version MetaFactory::convertVersion(clang::VersionTuple clangVersion)
{
    Version result = {
        .Major = (int)clangVersion.getMajor(),
        .Minor = (int)(clangVersion.getMinor().hasValue() ? clangVersion.getMinor().getValue() : -1),
        .SubMinor = (int)(clangVersion.getSubminor().hasValue() ? clangVersion.getSubminor().getValue() : -1)
    };
    return result;
}
}