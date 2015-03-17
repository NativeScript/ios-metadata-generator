#include <llvm/Support/Casting.h>
#include "MetaFactory.h"

using namespace std;
using namespace Meta;

shared_ptr<FunctionMeta> MetaFactory::createFunctionMeta(clang::FunctionDecl& function) {
    shared_ptr<FunctionMeta> functionMeta(new FunctionMeta());
    populateMetaFields(function, *(functionMeta.get()));
    functionMeta->setFlags(MetaFlags::FunctionIsVariadic , function.isVariadic());
    functionMeta->setFlags(MetaFlags::FunctionOwnsReturnedCocoaObject , this->getAttributes<clang::NSReturnsRetainedAttr>(function).size() > 0);
    return functionMeta;
}

shared_ptr<RecordMeta> MetaFactory::createRecordMeta(clang::RecordDecl& record) {
    shared_ptr<RecordMeta> recordMeta(record.isStruct() ? (RecordMeta*)(new StructMeta()) : (RecordMeta*)(new UnionMeta()));
    populateMetaFields(record, *(recordMeta.get()));
    // TODO: Use field_iterator to iterate through all fields of the record
    return recordMeta;
}

shared_ptr<InterfaceMeta> MetaFactory::createInterfaceMeta(clang::ObjCInterfaceDecl& interface) {
    shared_ptr<InterfaceMeta> interfaceMeta(new InterfaceMeta());
    populateMetaFields(interface, *(interfaceMeta.get()));
    populateBaseClassMetaFields(interface, *(interfaceMeta.get()));

    // set base interface
    clang::ObjCInterfaceDecl *super = interface.getSuperClass();
    interfaceMeta->baseName = (super == nullptr) ? FQName { .name = "", .module = "" } : FQName { .name = _jsNameGenerator.getJsName(*super), .module = this->getModule(*super)->getFullModuleName() };

    return interfaceMeta;
}

shared_ptr<ProtocolMeta> MetaFactory::createProtocolMeta(clang::ObjCProtocolDecl& protocol) {
    shared_ptr<ProtocolMeta> protocolMeta(new ProtocolMeta());
    populateMetaFields(protocol, *(protocolMeta.get()));
//    for (clang::ObjCProtocolDecl::protocol_iterator i = protocol.protocol_begin(); i != protocol.protocol_end(); ++i) {
//        printf("%s, ", (*i)->getNameAsString().c_str());
//    }
    return protocolMeta;
}

MethodMeta MetaFactory::createMethodMeta(clang::ObjCMethodDecl& method) {
    MethodMeta methodMeta = MethodMeta();
    populateMetaFields(method, methodMeta);

    // set selector
    // TODO: We can use the name property instead of selector and remove the selector propery.
    methodMeta.selector = method.getSelector().getAsString();

    // set type encoding
    // TODO: Remove type encodings. We don't need them any more.
    this->_astUnit->getASTContext().getObjCEncodingForMethodDecl(&method, methodMeta.typeEncoding, false);

    // set IsVariadic flag
    methodMeta.setFlags(MetaFlags::MethodIsVariadic, method.isVariadic());

    // set MethodIsNilTerminatedVariadic
    methodMeta.setFlags(MetaFlags::MethodIsNullTerminatedVariadic, method.isVariadic() && this->getAttributes<clang::SentinelAttr>(method).size() > 0);

    // set MethodOwnsReturnedCocoaObject flag
    bool nsReturnsRetainedAttr = this->getAttributes<clang::NSReturnsRetainedAttr>(method).size() > 0;
    bool nsReturnsNotRetainedAttr = this->getAttributes<clang::NSReturnsNotRetainedAttr>(method).size() > 0;
    if(nsReturnsRetainedAttr && nsReturnsNotRetainedAttr)
        throw EntityCreationException(string("Method has both NS_Returns_Retained and NS_Returns_Not_Retained attributes."), true);
    else if(nsReturnsRetainedAttr)
        methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  true);
    else if(nsReturnsNotRetainedAttr)
        methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject ,  false);
    else {
        methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, false);
        vector<string> selectorBegins = { "alloc", "new", "copy", "mutableCopy" };
        for (int i = 0; i < selectorBegins.size(); ++i) {
            if(selectorBegins[i].compare(0, methodMeta.selector.size(), methodMeta.selector)) {
                methodMeta.setFlags(MetaFlags::MethodOwnsReturnedCocoaObject, true);
                break;
            }
        }
    }

    return methodMeta;
}

PropertyMeta MetaFactory::createPropertyMeta(clang::ObjCPropertyDecl& property) {
    PropertyMeta propertyMeta = PropertyMeta();
    populateMetaFields(property, propertyMeta);
    // TODO: populate all property fields
    return propertyMeta;
}

void MetaFactory::populateMetaFields(clang::NamedDecl& decl, Meta& meta) {
    meta.name = decl.getNameAsString();
    meta.jsName = this->_jsNameGenerator.getJsName(decl);
    meta.setFlags(MetaFlags::HasName , meta.name != meta.jsName);
    meta.module = this->getModule(decl)->getFullModuleName();

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
            throw EntityCreationException("The declaration is marked unvailable for ios platform (with availability attribute).", false);
        }
        meta.introducedIn = this->convertVersion(iosAvailability->getIntroduced());
        meta.deprecatedIn = this->convertVersion(iosAvailability->getDeprecated());
        meta.obsoletedIn = this->convertVersion(iosAvailability->getObsoleted());
    }
    bool isIosExtensionsAvailable = iosExtensionsAvailability == nullptr || !iosExtensionsAvailability->getUnavailable();
    meta.setFlags(MetaFlags::IsIosAppExtensionAvailable , isIosExtensionsAvailable);
}

void MetaFactory::populateBaseClassMetaFields(clang::ObjCContainerDecl& decl, BaseClassMeta& baseClass) {
    llvm::iterator_range<clang::ObjCProtocolList::iterator> protocols = this->getProtocols(&decl);
    for (clang::ObjCProtocolList::iterator i = protocols.begin(); i != protocols.end() ; ++i) {
        clang::ObjCProtocolDecl *protocol = *i;
        FQName protocolName = FQName { .name = _jsNameGenerator.getJsName(*protocol), .module = this->getModule(*protocol)->getFullModuleName() };
        baseClass.protocols.push_back(protocolName);
    }

    for (clang::ObjCContainerDecl::classmeth_iterator i = decl.classmeth_begin(); i != decl.classmeth_end(); ++i) {
        clang::ObjCMethodDecl& classMethod = **i;
        MethodMeta method = this->createMethodMeta(classMethod);
        baseClass.staticMethods.push_back(method);
    }

    for (clang::ObjCContainerDecl::instmeth_iterator i = decl.instmeth_begin(); i != decl.instmeth_end(); ++i) {
        clang::ObjCMethodDecl& instanceMethod = **i;
        MethodMeta method = this->createMethodMeta(instanceMethod);
        baseClass.instanceMethods.push_back(method);
    }
}

clang::Module *MetaFactory::getModule(clang::Decl& decl) {
    clang::FileID id = _astUnit->getSourceManager().getDecomposedLoc(decl.getLocation()).first;
    const clang::FileEntry *entry = _astUnit->getSourceManager().getFileEntryForID(id);
    if(!entry) {
        throw EntityCreationException("The containing file (and respectively the containing module) for declaration is not found.", true);
    }
    clang::Module *owningModule = _astUnit->getPreprocessor().getHeaderSearchInfo().findModuleForHeader(entry).getModule();
    if(!owningModule) {
        // TODO: research why the full module name is empty and maybe add the declaration to global module
        throw EntityCreationException(string("There is no module corresponding to file ") + string(entry->getName()) + string("."), true);
    }
    return owningModule;
}

llvm::iterator_range<clang::ObjCProtocolList::iterator> MetaFactory::getProtocols(clang::ObjCContainerDecl* objCContainer) {
    if(clang::ObjCInterfaceDecl* interface = clang::dyn_cast<clang::ObjCInterfaceDecl>(objCContainer))
        return interface->protocols();
    else if(clang::ObjCProtocolDecl* protocol = clang::dyn_cast<clang::ObjCProtocolDecl>(objCContainer))
        return protocol->protocols();
    else if(clang::ObjCCategoryDecl* category = clang::dyn_cast<clang::ObjCCategoryDecl>(objCContainer))
        return category->protocols();
    throw EntityCreationException(string("Unable to extract protocols form this type of ObjC container."), true);
}

Version MetaFactory::convertVersion(clang::VersionTuple clangVersion) {
    Version result = {
            .Major = (int)clangVersion.getMajor(),
            .Minor = (int)(clangVersion.getMinor().hasValue() ? clangVersion.getMinor().getValue() : -1),
            .SubMinor = (int)(clangVersion.getSubminor().hasValue() ? clangVersion.getSubminor().getValue() : -1)
    };
    return result;
}