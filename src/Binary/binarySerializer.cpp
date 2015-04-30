#include "binarySerializer.h"
#include "binarySerializerPrivate.h"
#include "../Meta/Utils.h"

uint8_t convertVersion(Meta::Version version) {
    uint8_t result = 0;
    if (version.Major != -1)
    {
        result |= version.Major << 3;
        if (version.Minor != -1)
        {
            result |= version.Minor;
        }
    }
    return result;
}

bool compareMethodMetas(std::shared_ptr<Meta::MethodMeta>& meta1, std::shared_ptr<Meta::MethodMeta>& meta2) {
    return meta1->id.jsName < meta2->id.jsName;
}

bool comparePropertyMetas(std::shared_ptr<Meta::PropertyMeta>& meta1, std::shared_ptr<Meta::PropertyMeta>& meta2) {
    return meta1->id.jsName < meta2->id.jsName;
}

bool compareIdentifiers(Meta::DeclId & id1, Meta::DeclId & id2) {
    return id1.jsName < id2.jsName;
}

bool isInitMethod(std::shared_ptr<Meta::MethodMeta>& meta) {
    std::string prefix = "init";
    return meta->id.name.substr(0, prefix.size()) == prefix;
}

void binary::BinarySerializer::serializeBase(::Meta::Meta* meta, binary::Meta& binaryMetaStruct) {
    // name
    bool hasName = meta->id.name != meta->id.jsName;
    if (hasName) {
        MetaFileOffset offset1 = this->heapWriter.push_string(meta->id.jsName);
        MetaFileOffset offset2 = this->heapWriter.push_string(meta->id.name);
        binaryMetaStruct._names = this->heapWriter.push_pointer(offset1);
        this->heapWriter.push_pointer(offset2);
    } else {
        binaryMetaStruct._names = this->heapWriter.push_string(meta->id.jsName);
    }

    // flags
    uint8_t flags = 0;
    //::Meta::MetaFlags flags = meta->flags;
    if (hasName) {
        flags = (uint8_t)(flags | BinaryFlags::HasName);
    }
    flags = (uint8_t)(flags | (meta->type & 7)); // add type; 7 = 111 -> get only the first 3 bits of the type
    if(meta->getFlags(::Meta::MetaFlags::IsIosAppExtensionAvailable))
        flags |= BinaryFlags::IsIosAppExtensionAvailable;
    binaryMetaStruct._flags = flags;

    // module
    clang::Module* topLevelModule = meta->id.module->getTopLevelModule();
    std::string topLevelModuleName = topLevelModule->getFullModuleName();
    MetaFileOffset moduleOffset = this->file->getFromTopLevelModulesTable(topLevelModuleName);
    if(moduleOffset != 0)
        binaryMetaStruct._topLevelModule = moduleOffset;
    else {
        binary::ModuleMeta moduleMeta;
        serializeModule(topLevelModule, moduleMeta);
        binaryMetaStruct._topLevelModule = moduleMeta.save(this->heapWriter);
        this->file->registerInTopLevelModulesTable(topLevelModuleName, binaryMetaStruct._topLevelModule);
    }

    // introduced in
    binaryMetaStruct._introduced = convertVersion(meta->introducedIn);
}

void binary::BinarySerializer::serializeBaseClass(::Meta::BaseClassMeta *meta, binary::BaseClassMeta& binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    std::vector<MetaFileOffset> offsets;

    // instance methods
    std::sort(meta->instanceMethods.begin(), meta->instanceMethods.end(), compareMethodMetas);
    for (std::shared_ptr<::Meta::MethodMeta> methodMeta : meta->instanceMethods) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(methodMeta.get(), binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter));
    }
    binaryMetaStruct._instanceMethods = offsets.size() > 0 ? this->heapWriter.push_binaryArray(offsets) : 0;
    offsets.clear();

    // static methods
    std::sort(meta->staticMethods.begin(), meta->staticMethods.end(), compareMethodMetas);
    for (std::shared_ptr<::Meta::MethodMeta> methodMeta : meta->staticMethods) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(methodMeta.get(), binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter));
    }
    binaryMetaStruct._staticMethods = offsets.size() > 0 ? this->heapWriter.push_binaryArray(offsets) : 0;
    offsets.clear();

    // properties
    std::sort(meta->properties.begin(), meta->properties.end(), comparePropertyMetas);
    for (std::shared_ptr<::Meta::PropertyMeta>& propertyMeta : meta->properties) {
        binary::PropertyMeta binaryMeta;
        this->serializeProperty(propertyMeta.get(), binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter));
    }
    binaryMetaStruct._properties = offsets.size() > 0 ? this->heapWriter.push_binaryArray(offsets) : 0;
    offsets.clear();

    // protocols
    std::sort(meta->protocols.begin(), meta->protocols.end(), compareIdentifiers);
    for (::Meta::DeclId & protocolName : meta->protocols) {
        offsets.push_back(this->heapWriter.push_string(protocolName.jsName));
    }
    binaryMetaStruct._protocols = offsets.size() > 0 ? this->heapWriter.push_binaryArray(offsets) : 0;
    offsets.clear();

    // first initializer index
    int16_t firstInitializerIndex = -1;
    for(std::vector<std::shared_ptr<::Meta::MethodMeta>>::iterator it =  meta->instanceMethods.begin(); it != meta->instanceMethods.end(); ++it) {
        if(isInitMethod(*it)) {
            firstInitializerIndex = (int16_t) std::distance(meta->instanceMethods.begin(), it);
            break;
        }
    }
    binaryMetaStruct._initializersStartIndex = firstInitializerIndex;
}

void binary::BinarySerializer::serializeMethod(::Meta::MethodMeta *meta, binary::MethodMeta &binaryMetaStruct) {

    this->serializeBase(meta, binaryMetaStruct);
    binaryMetaStruct._flags &= 248; // 248 = 11111000; this clears the type information written in the lower 3 bits

    if(meta->getFlags(::Meta::MetaFlags::MethodIsVariadic))
        binaryMetaStruct._flags |= BinaryFlags::MethodIsVariadic;
    if(meta->getFlags(::Meta::MetaFlags::MethodIsNullTerminatedVariadic))
        binaryMetaStruct._flags |= BinaryFlags::MethodIsNullTerminatedVariadic;
    if(meta->getFlags(::Meta::MetaFlags::MethodOwnsReturnedCocoaObject))
        binaryMetaStruct._flags |= BinaryFlags::MethodOwnsReturnedCocoaObject;

    vector<::Meta::Type> typeEncodings;
    for (auto& encoding : meta->signature) {
        typeEncodings.push_back(encoding);
    }
    binaryMetaStruct._encoding = this->typeEncodingSerializer.visit(typeEncodings);
}

void binary::BinarySerializer::serializeProperty(::Meta::PropertyMeta *meta, binary::PropertyMeta &binaryMetaStruct) {

    this->serializeBase(meta, binaryMetaStruct);
    binaryMetaStruct._flags &= 248; // 248 = 11111000; this clears the type information writen in the lower 3 bits

    if(meta->getter) {
        binaryMetaStruct._flags |= BinaryFlags::PropertyHasGetter;
        binary::MethodMeta binaryMeta;
        this->serializeMethod(meta->getter.get(), binaryMeta);
        binaryMetaStruct._getter = binaryMeta.save(this->heapWriter);
    }
    if(meta->setter) {
        binaryMetaStruct._flags |= BinaryFlags::PropertyHasSetter;
        binary::MethodMeta binaryMeta;
        this->serializeMethod(meta->setter.get(), binaryMeta);
        binaryMetaStruct._setter = binaryMeta.save(this->heapWriter);
    }
}

void binary::BinarySerializer::serializeRecord(::Meta::RecordMeta *meta, binary::RecordMeta &binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    vector<::Meta::Type> typeEncodings;
    vector<MetaFileOffset> nameOffsets;

    for (::Meta::RecordField& recordField : meta->fields) {
        typeEncodings.push_back(recordField.encoding);
        nameOffsets.push_back(this->heapWriter.push_string(recordField.name));
    }

    binaryMetaStruct._fieldNames = this->heapWriter.push_binaryArray(nameOffsets);
    binaryMetaStruct._fieldsEncodings = this->typeEncodingSerializer.visit(typeEncodings);
}

void binary::BinarySerializer::serializeContainer(::Meta::MetaContainer& container) {
    this->start(&container);
    for (::Meta::MetaContainer::top_level_modules_iterator moduleIt = container.top_level_modules_begin(); moduleIt != container.top_level_modules_end(); ++moduleIt) {
        ::Meta::ModuleMeta & module = *moduleIt;
        for(::Meta::ModuleMeta::iterator metaIt = module.begin(); metaIt != module.end(); ++metaIt) {
            std::pair<std::string, std::shared_ptr<::Meta::Meta>> metaPair = *metaIt;
            metaPair.second->visit(this);
        }
    }
    this->finish(&container);
}

void binary::BinarySerializer::serializeModule(clang::Module* module, binary::ModuleMeta& binaryModule) {
    uint8_t flags = 0;
    if(module->isPartOfFramework())
        flags |= 1;
    if(module->IsSystem)
        flags |= 2;
    binaryModule._flags |= flags;
    binaryModule._name = this->heapWriter.push_string(module->getFullModuleName());
    std::vector<clang::Module::LinkLibrary> libraries;
    ::Meta::Utils::getAllLinkLibraries(module, libraries);
    std::vector<MetaFileOffset> librariesOffsets;
    for(clang::Module::LinkLibrary lib : libraries) {
        binary::LibraryMeta libMeta;
        serializeLibrary(&lib, libMeta);
        librariesOffsets.push_back(libMeta.save(this->heapWriter));
    }
    binaryModule._libraries = this->heapWriter.push_binaryArray(librariesOffsets);
}

void binary::BinarySerializer::serializeLibrary(clang::Module::LinkLibrary* library, binary::LibraryMeta& binaryLib) {
    uint8_t flags = 0;
    if(library->IsFramework)
        flags |= 1;
    binaryLib._flags = flags;
    binaryLib._name = this->heapWriter.push_string(library->Library);
}

void binary::BinarySerializer::start(::Meta::MetaContainer *container) {
}

void binary::BinarySerializer::finish(::Meta::MetaContainer *container) {

}

void binary::BinarySerializer::visit(::Meta::InterfaceMeta* meta) {
    binary::InterfaceMeta binaryStruct;
    serializeBaseClass(meta, binaryStruct);
    if (!meta->base.jsName.empty()) {
        binaryStruct._baseName = this->heapWriter.push_string(meta->base.jsName);
    }
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::ProtocolMeta* meta) {
    binary::ProtocolMeta binaryStruct;
    serializeBaseClass(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::CategoryMeta *meta) {
    // we shouldn't have categories in the binary file
}

void binary::BinarySerializer::visit(::Meta::FunctionMeta* meta) {
    binary::FunctionMeta binaryStruct;
    serializeBase(meta, binaryStruct);

    if(meta->getFlags(::Meta::MetaFlags::FunctionIsVariadic))
        binaryStruct._flags |= BinaryFlags::FunctionIsVariadic;
    if(meta->getFlags(::Meta::MetaFlags::FunctionOwnsReturnedCocoaObject))
        binaryStruct._flags |= BinaryFlags::FunctionOwnsReturnedCocoaObject;

    vector<::Meta::Type> typeEncodings;
    for (auto& encoding : meta->signature) {
        typeEncodings.push_back(encoding);
    }
    binaryStruct._encoding = this->typeEncodingSerializer.visit(typeEncodings);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::StructMeta* meta) {
    binary::StructMeta binaryStruct;
    serializeRecord(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::UnionMeta* meta) {
    binary::UnionMeta binaryStruct;
    serializeRecord(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::JsCodeMeta* meta) {
    binary::JsCodeMeta binaryStruct;
    serializeBase(meta, binaryStruct);
    binaryStruct._jsCode = this->heapWriter.push_string(meta->jsCode);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}

void binary::BinarySerializer::visit(::Meta::VarMeta* meta) {
    binary::VarMeta binaryStruct;
    serializeBase(meta, binaryStruct);
    unique_ptr<binary::TypeEncoding> binarySignature = meta->signature.visit(this->typeEncodingSerializer);
    binaryStruct._encoding = binarySignature->save(this->heapWriter);
    this->file->registerInGlobalTable(meta->id.jsName, binaryStruct.save(this->heapWriter));
}
