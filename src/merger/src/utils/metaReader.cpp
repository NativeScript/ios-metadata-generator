#include "metaReader.h"
#include <iostream>
#include <regex>
#include <memory>
#include "metaReaderPrivate.h"

using namespace meta;
using namespace typeEncoding;
using namespace std;

static map<string, MetaFlags> flagsMap {
    { "HasName", MetaFlags::HasName },
    { "IsIosAppExtensionAvailable", MetaFlags::IsIosAppExtensionAvailable },
    { "FunctionIsVariadic", MetaFlags::FunctionIsVariadic },
    { "FunctionOwnsReturnedCocoaObject", MetaFlags::FunctionOwnsReturnedCocoaObject },
    { "MemberIsLocalJsNameDuplicate", MetaFlags::MemberIsLocalJsNameDuplicate },
    { "MemberHasJsNameDuplicateInHierarchy", MetaFlags::MemberHasJsNameDuplicateInHierarchy },
    { "MethodIsVariadic", MetaFlags::MethodIsVariadic },
    { "MethodIsNullTerminatedVariadic", MetaFlags::MethodIsNullTerminatedVariadic },
    { "MethodOwnsReturnedCocoaObject", MetaFlags::MethodOwnsReturnedCocoaObject },
    { "PropertyHasGetter", MetaFlags::PropertyHasGetter },
    { "PropertyHasSetter", MetaFlags::PropertyHasSetter }
};

FQName parseName(const YAML::Node& node) {
    FQName n = { };
    if (node["Name"].IsDefined()) {
        n.name = node["Name"].as<string>();
        if (!node["Module"].IsNull()) {
            n.module = node["Module"].as<string>();
        }
    }
    return n;
}

Version parseVersion(string versionStr) {
    Version v = UNKNOWN_VERSION;
    regex versionPattern("^(\\d+)\\.?(\\d+)?\\.?(\\d+)?$");
    smatch match;
    if (regex_search(versionStr, match, versionPattern)) {
        istringstream(match.str(1)) >> v.Major;
        istringstream(match.str(2)) >> v.Minor;
        istringstream(match.str(3)) >> v.SubMinor;
    }
    return v;
}

MetaFlags parseFlags(const YAML::Node& node) {
    MetaFlags flags = MetaFlags::None;
    for (YAML::const_iterator it = node.begin(); it != node.end(); it++) {
        flags = (MetaFlags)(flags | flagsMap[it->as<string>()]);
    }
    return flags;
}

void parseSignature(const YAML::Node& signatureNode, vector<unique_ptr<typeEncoding::TypeEncoding>>& signatureVector) {
    for (YAML::const_iterator typeEncodingIterator = signatureNode.begin(); typeEncodingIterator != signatureNode.end(); ++typeEncodingIterator) {
        signatureVector.push_back(parseTypeEncoding(*typeEncodingIterator));
    }
}

void parseAnonymousRecordEncoding(const YAML::Node& recordNodeFields, typeEncoding::AnonymousRecordEncoding* recordEncoding) {
    for (YAML::const_iterator fieldIter = recordNodeFields.begin(); fieldIter != recordNodeFields.end(); ++fieldIter) {
        recordEncoding->fieldNames.push_back((*fieldIter)["Name"].as<string>());
        recordEncoding->fieldEncodings.push_back(parseTypeEncoding((*fieldIter)["Signature"]));
    }
}

// type encoding

unique_ptr<typeEncoding::TypeEncoding> parseTypeEncoding(const YAML::Node& node) {
    string type = node["Type"].as<string>();

    if (type == "Unknown") {
        return unique_ptr<typeEncoding::TypeEncoding>(new UnknownEncoding());
    } else if (type == "VaList") {
        return unique_ptr<typeEncoding::TypeEncoding>(new VaListEncoding());
    } else if (type == "Protocol") {
        return unique_ptr<typeEncoding::TypeEncoding>(new ProtocolEncoding());
    } else if (type == "Void") {
        return unique_ptr<typeEncoding::TypeEncoding>(new VoidEncoding());
    } else if (type == "Bool") {
        return unique_ptr<typeEncoding::TypeEncoding>(new BoolEncoding());
    } else if (type == "Short") {
        return unique_ptr<typeEncoding::TypeEncoding>(new ShortEncoding());
    } else if (type == "Ushort") {
        return unique_ptr<typeEncoding::TypeEncoding>(new UShortEncoding());
    } else if (type == "Int") {
        return unique_ptr<typeEncoding::TypeEncoding>(new IntEncoding());
    } else if (type == "UInt") {
        return unique_ptr<typeEncoding::TypeEncoding>(new UIntEncoding());
    } else if (type == "Long") {
        return unique_ptr<typeEncoding::TypeEncoding>(new LongEncoding());
    } else if (type == "ULong") {
        return unique_ptr<typeEncoding::TypeEncoding>(new ULongEncoding());
    } else if (type == "LongLong") {
        return unique_ptr<typeEncoding::TypeEncoding>(new LongLongEncoding());
    } else if (type == "ULongLong") {
        return unique_ptr<typeEncoding::TypeEncoding>(new ULongLongEncoding());
    } else if (type == "Char") {
        return unique_ptr<typeEncoding::TypeEncoding>(new SignedCharEncoding());
    } else if (type == "UChar") {
        return unique_ptr<typeEncoding::TypeEncoding>(new UnsignedCharEncoding());
    } else if (type == "Unichar") {
        return unique_ptr<typeEncoding::TypeEncoding>(new UnicharEncoding());
    } else if (type == "CString") {
        return unique_ptr<typeEncoding::TypeEncoding>(new CStringEncoding());
    } else if (type == "Float") {
        return unique_ptr<typeEncoding::TypeEncoding>(new FloatEncoding());
    } else if (type == "Double") {
        return unique_ptr<typeEncoding::TypeEncoding>(new DoubleEncoding());
    } else if (type == "Selector") {
        return unique_ptr<typeEncoding::TypeEncoding>(new SelectorEncoding());
    } else if (type == "Class") {
        return unique_ptr<typeEncoding::TypeEncoding>(new ClassEncoding());
    } else if (type == "Instancetype") {
        return unique_ptr<typeEncoding::TypeEncoding>(new InstancetypeEncoding());
    } else if (type == "Id") {
        IdEncoding *idEncoding = new IdEncoding();
        YAML::Node protocolsNode = node["WithProtocols"];
        for (YAML::const_iterator it = protocolsNode.begin(); it != protocolsNode.end(); it++) {
            idEncoding->protocols.push_back(parseName(*it));
        }
        return unique_ptr<typeEncoding::TypeEncoding>(idEncoding);
    } else if (type == "ConstantArray") {
        ConstantArrayEncoding *encoding = new ConstantArrayEncoding();
        encoding->elementType = parseTypeEncoding(node["ArrayType"]);
        encoding->size = node["Size"].as<int>();
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "IncompleteArray") {
        IncompleteArrayEncoding *encoding = new IncompleteArrayEncoding();
        encoding->elementType = parseTypeEncoding(node["ArrayType"]);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "Interface") {
        InterfaceEncoding *encoding = new InterfaceEncoding();
        encoding->name = parseName(node);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "FunctionPointer") {
        FunctionEncoding *encoding = new FunctionEncoding();
        parseSignature(node["Signature"], encoding->functionCall);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "Block") {
        BlockEncoding *encoding = new BlockEncoding();
        parseSignature(node["Signature"], encoding->blockCall);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "Pointer") {
        PointerEncoding *encoding = new PointerEncoding();
        encoding->target = parseTypeEncoding(node["PointerType"]);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "Struct") {
        StructEncoding *encoding = new StructEncoding();
        encoding->name = parseName(node);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "Union") {
        UnionEncoding *encoding = new UnionEncoding();
        encoding->name = parseName(node);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "PureInterface") {
        InterfaceDeclarationEncoding *encoding = new InterfaceDeclarationEncoding();
        encoding->name = parseName(node);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "AnonymousStruct") {
        AnonymousStructEncoding *encoding = new AnonymousStructEncoding();
        parseAnonymousRecordEncoding(node["Fields"], encoding);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else if (type == "AnonymousUnion") {
        AnonymousUnionEncoding *encoding = new AnonymousUnionEncoding();
        parseAnonymousRecordEncoding(node["Fields"], encoding);
        return unique_ptr<typeEncoding::TypeEncoding>(encoding);
    } else {
        cerr << "Error: unknown type encoding " << type << endl;
        exit(202);
    }

    return nullptr;
}

// meta

Meta* parseMeta(const YAML::Node& yamlNode, Meta* meta) {
    meta->name = yamlNode["Name"].as<string>();
    meta->jsName = yamlNode["JsName"].as<string>();
    if (yamlNode["Module"].IsDefined()) {
        meta->module = yamlNode["Module"].as<string>();
    }
    if (yamlNode["IntroducedIn"].IsDefined()) {
        meta->introducedIn = parseVersion(yamlNode["IntroducedIn"].as<string>());
    }
    meta->flags = parseFlags(yamlNode["Flags"]);

    return meta;
}

void parseMethodMeta(const YAML::Node &yamlNode, MethodMeta* meta) {
    parseMeta(yamlNode, meta);

    meta->selector = yamlNode["Selector"].as<string>();
    meta->typeEncoding = yamlNode["TypeEncoding"].as<string>();
    parseSignature(yamlNode["Signature"], meta->signature);
}

void parsePropertyMeta(const YAML::Node &yamlNode, PropertyMeta* meta) {
    parseMeta(yamlNode, meta);

    if (yamlNode["Getter"].IsDefined()) {
        MethodMeta* methodMeta = new MethodMeta();
        parseMethodMeta(yamlNode["Getter"], methodMeta);
        meta->getter = unique_ptr<MethodMeta>(methodMeta);
    }
    if (yamlNode["Setter"].IsDefined()) {
        MethodMeta* methodMeta = new MethodMeta();
        parseMethodMeta(yamlNode["Setter"], methodMeta);
        meta->setter = unique_ptr<MethodMeta>(methodMeta);
    }
}

Meta* parseBaseClassMeta(const YAML::Node& yamlNode, BaseClassMeta* meta){
    parseMeta(yamlNode, meta);

    YAML::Node instanceMethodsNode = yamlNode["InstanceMethods"];
    for (YAML::const_iterator methodIterator = instanceMethodsNode.begin(); methodIterator != instanceMethodsNode.end(); ++methodIterator) {
        MethodMeta methodMeta;
        parseMethodMeta(*methodIterator, &methodMeta);
        meta->instanceMethods.push_back(std::move(methodMeta));
    }

    YAML::Node staticMethodsNode = yamlNode["StaticMethods"];
    for (YAML::const_iterator methodIterator = staticMethodsNode.begin(); methodIterator != staticMethodsNode.end(); ++methodIterator) {
        MethodMeta methodMeta;
        parseMethodMeta(*methodIterator, &methodMeta);
        meta->staticMethods.push_back(std::move(methodMeta));
    }

    YAML::Node propertiesNode = yamlNode["Properties"];
    for (YAML::const_iterator propertyIterator = propertiesNode.begin(); propertyIterator != propertiesNode.end(); ++propertyIterator) {
        PropertyMeta propertyMeta;
        parsePropertyMeta(*propertyIterator, &propertyMeta);
        meta->properties.push_back(std::move(propertyMeta));
    }

    YAML::Node protocolsNode = yamlNode["Protocols"];
    for (YAML::const_iterator protocolIterator = protocolsNode.begin(); protocolIterator != protocolsNode.end(); ++protocolIterator) {
        meta->protocols.push_back(parseName(*protocolIterator));
    }

    return meta;
}

Meta* parseRecordMeta(const YAML::Node& yamlNode, RecordMeta* meta){
    parseMeta(yamlNode, meta);

    YAML::Node fieldsNode = yamlNode["Fields"];
    for (YAML::const_iterator fieldIterator = fieldsNode.begin(); fieldIterator != fieldsNode.end(); ++fieldIterator) {
        RecordField field;
        field.name = (*fieldIterator)["Name"].as<string>();
        field.encoding = std::move(parseTypeEncoding((*fieldIterator)["Signature"]));
        meta->fields.push_back(std::move(field));
    }

    return meta;
}

Meta* parseJsCodeMeta(const YAML::Node& yamlNode, JsCodeMeta* meta){
    parseMeta(yamlNode, meta);

    meta->jsCode = yamlNode["JsCode"].as<string>();
    return meta;
}

unique_ptr<Meta> createMeta(const YAML::Node& node) {
    string nodeType = node["Type"].as<string>();
    Meta* meta = nullptr;

    if (nodeType == "Interface") {
        InterfaceMeta *interfaceMeta = new InterfaceMeta();
        meta = parseBaseClassMeta(node, interfaceMeta);
        interfaceMeta->baseName = parseName(node["Base"]);
    } else if (nodeType == "Protocol") {
        meta = parseBaseClassMeta(node, new ProtocolMeta());
    } else if (nodeType == "Category") {
        CategoryMeta *categoryMeta = new CategoryMeta();
        meta = parseBaseClassMeta(node, categoryMeta);
        categoryMeta->extendedInterface = parseName(node["ExtendedInterface"]);
    } else if (nodeType == "Struct") {
        meta = parseRecordMeta(node, new StructMeta());
    } else if (nodeType == "Union") {
        meta = parseRecordMeta(node, new UnionMeta());
    } else if (nodeType == "JsCode") {
        meta = parseJsCodeMeta(node, new JsCodeMeta());
    } else if (nodeType == "Var") {
        VarMeta *varMeta = new VarMeta();
        meta = parseMeta(node, varMeta);
        varMeta->signature = parseTypeEncoding(node["Signature"]);
    } else if (nodeType == "Function") {
        FunctionMeta *functionMeta = new FunctionMeta();
        meta = parseMeta(node, functionMeta);
        parseSignature(node["Signature"], functionMeta->signature);
    } else {
        cerr << "Error: unknown meta type " << nodeType << endl;
        exit(201);
    }

    return unique_ptr<Meta>(meta);
}

void utils::MetaReader::readFile(const boost::filesystem::path& filepath, MetaContainer& container) {
    if (filepath.extension() != ".yaml")
        return;

    cout << filepath.string() << endl;
    YAML::Node node = YAML::LoadFile(filepath.string());
    YAML::Node items = node["items"];
    for (YAML::const_iterator it = items.begin(); it != items.end(); it++) {
        unique_ptr<Meta> meta = createMeta(*it);
        container.add(std::move(meta));
    }
}
