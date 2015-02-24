#include "binarySerializer.h"
#include "binarySerializerPrivate.h"
#include "../utils/metaContainer.h"
#include <set>

uint8_t convertVersion(Version version) {
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

bool compareMethodMetas(meta::MethodMeta& meta1, meta::MethodMeta& meta2) {
    return meta1.jsName < meta2.jsName;
}

bool comparePropertyMetas(meta::PropertyMeta& meta1, meta::PropertyMeta& meta2) {
    return meta1.jsName < meta2.jsName;
}

bool compareFQN(FQName& name1, FQName& name2) {
    return name1.name < name2.name;
}

bool isInitMethod(meta::MethodMeta& meta) {
    std::string prefix = "init";
    return meta.name.substr(0, prefix.size()) == prefix;
}

string getTopLevelModule(const string& moduleName) {
    int dotPosition = moduleName.find('.', 0);
    return (dotPosition == string::npos) ? moduleName : moduleName.substr(0, dotPosition);
}

void binary::BinarySerializer::serializeBase(meta::Meta* meta, binary::Meta& binaryMetaStruct) {
    // name
    bool hasName = meta->name != meta->jsName;
    if (!hasName) {
        binaryMetaStruct._names = this->heapWriter->push_string(meta->jsName);
    } else {
        MetaFileOffset offset1 = this->heapWriter->push_string(meta->jsName);
        MetaFileOffset offset2 = this->heapWriter->push_string(meta->name);
        binaryMetaStruct._names = this->heapWriter->push_pointer(offset1);
        this->heapWriter->push_pointer(offset2);
    }

    // flags
    meta::MetaFlags flags = meta->flags;
    if (hasName) {
        flags = (meta::MetaFlags)(flags | meta::MetaFlags::HasName);
    }
    flags = (meta::MetaFlags)(flags | meta->type); // add type
    binaryMetaStruct._flags = flags;

    // module
    binaryMetaStruct._frameworkId = (uint16_t)this->moduleMap[getTopLevelModule(meta->module)];

    // introduced in
    binaryMetaStruct._introduced = convertVersion(meta->introducedIn);
}

void binary::BinarySerializer::serializeBaseClass(meta::BaseClassMeta *meta, binary::BaseClassMeta& binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    std::vector<MetaFileOffset> offsets;

    // instance methods
    std::sort(meta->instanceMethods.begin(), meta->instanceMethods.end(), compareMethodMetas);
    for (meta::MethodMeta& methodMeta : meta->instanceMethods) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(&methodMeta, binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter.get()));
    }
    binaryMetaStruct._instanceMethods = offsets.size() > 0 ? this->heapWriter->push_binaryArray(offsets) : 0;
    offsets.clear();

    // static methods
    std::sort(meta->staticMethods.begin(), meta->staticMethods.end(), compareMethodMetas);
    for (meta::MethodMeta& methodMeta : meta->staticMethods) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(&methodMeta, binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter.get()));
    }
    binaryMetaStruct._staticMethods = offsets.size() > 0 ? this->heapWriter->push_binaryArray(offsets) : 0;
    offsets.clear();

    // properties
    std::sort(meta->properties.begin(), meta->properties.end(), comparePropertyMetas);
    for (meta::PropertyMeta& propertyMeta : meta->properties) {
        binary::PropertyMeta binaryMeta;
        this->serializeProperty(&propertyMeta, binaryMeta);
        offsets.push_back(binaryMeta.save(this->heapWriter.get()));
    }
    binaryMetaStruct._properties = offsets.size() > 0 ? this->heapWriter->push_binaryArray(offsets) : 0;
    offsets.clear();

    // protocols
    std::sort(meta->protocols.begin(), meta->protocols.end(), compareFQN);
    for (FQName& protocolName : meta->protocols) {
        offsets.push_back(this->heapWriter->push_string(protocolName.name));
    }
    binaryMetaStruct._protocols = offsets.size() > 0 ? this->heapWriter->push_binaryArray(offsets) : 0;
    offsets.clear();

    // first initializer index
    vector<meta::MethodMeta>::iterator initMethod = std::find_if(meta->instanceMethods.begin(), meta->instanceMethods.end(), isInitMethod);
    int16_t firstInitializerIndex = initMethod != meta->instanceMethods.end() ? (int16_t)std::distance(meta->instanceMethods.begin(), initMethod) : -1;
    binaryMetaStruct._initializersStartIndex = firstInitializerIndex;
}

void binary::BinarySerializer::serializeMethod(meta::MethodMeta *meta, binary::MethodMeta &binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    binaryMetaStruct._selector = this->heapWriter->push_string(meta->selector);
    vector<typeEncoding::TypeEncoding*> typeEncodings;
    for (auto& encoding : meta->signature) {
        typeEncodings.push_back(encoding.get());
    }
    binaryMetaStruct._encoding = this->typeEncodingSerializer.serialize(typeEncodings);
    binaryMetaStruct._compilerEncoding = this->heapWriter->push_string(meta->typeEncoding);
}

void binary::BinarySerializer::serializeProperty(meta::PropertyMeta *meta, binary::PropertyMeta &binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    if ((meta->flags & meta::MetaFlags::PropertyHasGetter) == meta::MetaFlags::PropertyHasGetter) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(meta->getter.get(), binaryMeta);
        binaryMetaStruct._getter = binaryMeta.save(this->heapWriter.get());
    }
    if ((meta->flags & meta::MetaFlags::PropertyHasSetter) == meta::MetaFlags::PropertyHasSetter) {
        binary::MethodMeta binaryMeta;
        this->serializeMethod(meta->setter.get(), binaryMeta);
        binaryMetaStruct._setter = binaryMeta.save(this->heapWriter.get());
    }
}

void binary::BinarySerializer::serializeRecord(meta::RecordMeta *meta, binary::RecordMeta &binaryMetaStruct) {
    this->serializeBase(meta, binaryMetaStruct);

    vector<typeEncoding::TypeEncoding*> typeEncodings;
    vector<MetaFileOffset> nameOffsets;

    for (meta::RecordField& recordField : meta->fields) {
        typeEncodings.push_back(recordField.encoding.get());
        nameOffsets.push_back(this->heapWriter->push_string(recordField.name));
    }

    binaryMetaStruct._fieldNames = this->heapWriter->push_binaryArray(nameOffsets);
    binaryMetaStruct._fieldsEncodings = this->typeEncodingSerializer.serialize(typeEncodings);
}

struct CaseInsensitiveCompare {
    bool operator() (const std::string& a, const std::string& b) const {
        return strcasecmp(a.c_str(), b.c_str()) < 0;
    }
};

void binary::BinarySerializer::start(utils::MetaContainer *container) {
    // get all top level modules and write them down first
    set<string, CaseInsensitiveCompare> topLevelModules;
    for (auto moduleIter = container->beginModules(); moduleIter != container->endModules(); ++moduleIter) {
        string topLevelModule = getTopLevelModule(*moduleIter);
        topLevelModules.emplace(topLevelModule);

        binary::MetaFileOffset offset = this->heapWriter->push_string(topLevelModule);
        this->moduleMap.emplace(topLevelModule, offset);
    }
    vector<string> modulesVector(topLevelModules.begin(), topLevelModules.end());
    this->file->registerTopLevelModules(modulesVector);
}

void binary::BinarySerializer::finish(utils::MetaContainer *container) {

}

void binary::BinarySerializer::serialize(meta::InterfaceMeta* meta) {
    binary::InterfaceMeta binaryStruct;
    serializeBaseClass(meta, binaryStruct);
    if (!meta->baseName.isEmpty()) {
        binaryStruct._baseName = this->heapWriter->push_string(meta->baseName.name);
    }
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::ProtocolMeta* meta) {
    binary::ProtocolMeta binaryStruct;
    serializeBaseClass(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::CategoryMeta *meta) {
    // we shouldn't have categories in the binary file
}

void binary::BinarySerializer::serialize(meta::FunctionMeta* meta) {
    binary::FunctionMeta binaryStruct;
    serializeBase(meta, binaryStruct);
    vector<typeEncoding::TypeEncoding*> typeEncodings;
    for (auto& encoding : meta->signature) {
        typeEncodings.push_back(encoding.get());
    }
    binaryStruct._encoding = this->typeEncodingSerializer.serialize(typeEncodings);
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::StructMeta* meta) {
    binary::StructMeta binaryStruct;
    serializeRecord(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::UnionMeta* meta) {
    binary::UnionMeta binaryStruct;
    serializeRecord(meta, binaryStruct);
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::JsCodeMeta* meta) {
    binary::JsCodeMeta binaryStruct;
    serializeBase(meta, binaryStruct);
    binaryStruct._jsCode = this->heapWriter->push_string(meta->jsCode);
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}

void binary::BinarySerializer::serialize(meta::VarMeta* meta) {
    binary::VarMeta binaryStruct;
    serializeBase(meta, binaryStruct);
    unique_ptr<binary::TypeEncoding> binarySignature = meta->signature->serialize(&this->typeEncodingSerializer);
    binaryStruct._encoding = binarySignature->save(this->heapWriter.get());
    this->file->registerInGlobalTable(meta->jsName, binaryStruct.save(this->heapWriter.get()));
}
