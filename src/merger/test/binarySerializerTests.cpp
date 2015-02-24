#include "test.h"
#include "binary/binarySerializerPrivate.h"
#include "binary/binarySerializer.h"
#include "binary/metaFile.h"
#include "meta/meta.h"
#include "typeEncoding/typeEncoding.h"
#include "utils/memoryStream.h"

TEST (BinarySerializerTests, TestConvertVersion_Major) {
    Version version = { .Major = 8, .Minor = 0, .SubMinor = -1 };
    uint8_t versionBytes = convertVersion(version);
    EXPECT_EQ(64, versionBytes);
}

TEST (BinarySerializerTests, TestConvertVersion_MajorMinor) {
    Version version = { .Major = 8, .Minor = 1, .SubMinor = -1 };
    uint8_t versionBytes = convertVersion(version);
    EXPECT_EQ(65, versionBytes);
}

TEST (BinarySerializerTests, TestConvertVersion_MajorMinor_SubMinor) {
    Version version = { .Major = 8, .Minor = 1, .SubMinor = 3 };
    uint8_t versionBytes = convertVersion(version);
    EXPECT_EQ(65, versionBytes);
}

TEST (BinarySerializerTests, TestConvertVersion_Invalid) {
    Version version = { .Major = -1, .Minor = -1, .SubMinor = -1 };
    uint8_t versionBytes = convertVersion(version);
    EXPECT_EQ(0, versionBytes);
}

TEST (BinarySerializerTests, TestIsInitMethod_WithInitMethod) {
    meta::MethodMeta m;
    m.name = "initString";
    EXPECT_TRUE(isInitMethod(m));
}

TEST (BinarySerializerTests, TestIsInitMethod2_WithNormalMethod) {
    meta::MethodMeta m;
    m.name = "stringWithString";
    EXPECT_FALSE(isInitMethod(m));
}

/// Serialization tests

TEST (BinarySerializer_SerializationTests, TestSerialization_FunctionMeta) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::FunctionMeta* target = new meta::FunctionMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->signature.push_back(unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::VoidEncoding()));
    target->signature.push_back(unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target->signature.push_back(unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());

    // Type encoding
    int encodingOffset = reader->baseStream()->position();
    EXPECT_EQ(target->signature.size(), reader->read_arrayCount());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());

    // FunctionMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    EXPECT_EQ(target->flags | meta::SymbolType::Function, reader->read_byte());
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(encodingOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_FunctionMetaWithDifferentNames) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::FunctionMeta* target = new meta::FunctionMeta();
    target->name = "foo";
    target->jsName = "foo2";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->signature.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::VoidEncoding()));
    target->signature.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));
    target->signature.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int jsNameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->jsName, reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());

    int nameOffsetBA = reader->baseStream()->position();
    EXPECT_EQ(jsNameOffset, reader->read_pointer());
    EXPECT_EQ(nameOffset, reader->read_pointer());

    // Type encoding
    int encodingOffset = reader->baseStream()->position();
    EXPECT_EQ(target->signature.size(), reader->read_arrayCount());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Void, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());

    // FunctionMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffsetBA, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::MetaFlags::HasName | meta::SymbolType::Function, flags);
    EXPECT_EQ(meta::SymbolType::Function, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(encodingOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_StructMeta) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::StructMeta* target = new meta::StructMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->fields.push_back({ .name = "a1", .encoding = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()) });
    target->fields.push_back({ .name = "b1", .encoding = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::DoubleEncoding()) });
    target->fields.push_back({ .name = "c1", .encoding = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::LongEncoding()) });

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());

    // field names
    int a1Offset = reader->baseStream()->position();
    EXPECT_EQ(target->fields.at(0).name, reader->read_string());
    int b1Offset = reader->baseStream()->position();
    EXPECT_EQ(target->fields.at(1).name, reader->read_string());
    int c1Offset = reader->baseStream()->position();
    EXPECT_EQ(target->fields.at(2).name, reader->read_string());

    // field names binary array
    int fieldNamesOffset = reader->baseStream()->position();
    EXPECT_EQ(target->fields.size(), reader->read_arrayCount());
    EXPECT_EQ(a1Offset, reader->read_pointer()); // a1
    EXPECT_EQ(b1Offset, reader->read_pointer()); // b1
    EXPECT_EQ(c1Offset, reader->read_pointer()); // c1

    // field encodings
    int encodingOffset = reader->baseStream()->position();
    EXPECT_EQ(target->fields.size(), reader->read_arrayCount());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Double, reader->read_byte());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Long, reader->read_byte());

    // RecordMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::SymbolType::Struct, flags);
    EXPECT_EQ(meta::SymbolType::Struct, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(fieldNamesOffset, reader->read_pointer());
    EXPECT_EQ(encodingOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_VarMeta) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::VarMeta* target = new meta::VarMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->signature = std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding());

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());

    // Type encoding
    int encodingOffset = reader->baseStream()->position();
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());

    // VarMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::SymbolType::Var, flags);
    EXPECT_EQ(meta::SymbolType::Var, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(encodingOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_JSCodeMeta) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::JsCodeMeta* target = new meta::JsCodeMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->jsCode = "1";

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());
    int encodingOffset = reader->baseStream()->position();
    EXPECT_EQ(target->jsCode, reader->read_string());

    // JsCodeMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::SymbolType::JsCode, flags);
    EXPECT_EQ(meta::SymbolType::JsCode, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(encodingOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_Interface) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::InterfaceMeta* target = new meta::InterfaceMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->protocols.push_back({ .name = "fooProtocol", .module = "Foundation.foo" });
    target->baseName = { .name = "fooBase", .module = "Foundation.foo" };

    meta::MethodMeta *method1 = new meta::MethodMeta();
    method1->name = "foo";
    method1->jsName = "foo";
    method1->selector = "foo:";
    method1->typeEncoding = "@:i";
    method1->signature.push_back(std::unique_ptr<typeEncoding::TypeEncoding>(new typeEncoding::IntEncoding()));

    meta::PropertyMeta property1;
    property1.name = "foo";
    property1.jsName = "foo";
    property1.getter = std::unique_ptr<meta::MethodMeta>(method1);
    property1.flags = meta::MetaFlags::PropertyHasGetter;
    target->properties.push_back(std::move(property1));

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());
    int methodSelectorOffset = reader->baseStream()->position();
    EXPECT_EQ(method1->selector, reader->read_string());
    int methodSignatureOffset = reader->baseStream()->position();
    EXPECT_EQ(method1->signature.size(), reader->read_arrayCount());
    EXPECT_EQ(binary::BinaryTypeEncodingType::Int, reader->read_byte());
    int methodTypeEncodingOffset = reader->baseStream()->position();
    EXPECT_EQ(method1->typeEncoding, reader->read_string());

    // MethodMeta
    int methodOffset = reader->baseStream()->position();
    EXPECT_EQ(nameOffset, reader->read_pointer());
    EXPECT_EQ(method1->flags, reader->read_byte());
    EXPECT_EQ(0, reader->read_short()); // module id
    EXPECT_EQ(0, reader->read_byte()); // introduced in
    EXPECT_EQ(methodSelectorOffset, reader->read_pointer()); // selector
    EXPECT_EQ(methodSignatureOffset, reader->read_pointer()); // encoding
    EXPECT_EQ(methodTypeEncodingOffset, reader->read_pointer()); // compiler encoding

    // PropertyMeta
    int propertyOffset = reader->baseStream()->position();
    EXPECT_EQ(nameOffset, reader->read_pointer());
    EXPECT_EQ(property1.flags, reader->read_byte());
    EXPECT_EQ(0, reader->read_short()); // module id
    EXPECT_EQ(0, reader->read_byte()); // introduced in
    EXPECT_EQ(methodOffset, reader->read_pointer());

    int propertyListOffset = reader->baseStream()->position();
    EXPECT_EQ(target->properties.size(), reader->read_arrayCount());
    EXPECT_EQ(propertyOffset, reader->read_pointer());

    // protocols
    int protocolNameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->protocols[0].name, reader->read_string());

    int protocolListOffset = reader->baseStream()->position();
    EXPECT_EQ(target->protocols.size(), reader->read_arrayCount());
    EXPECT_EQ(protocolNameOffset, reader->read_pointer());

    // base name
    int baseNameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->baseName.name, reader->read_string());

    // InterfaceMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::SymbolType::Interface, flags);
    EXPECT_EQ(meta::SymbolType::Interface, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(0, reader->read_pointer());
    EXPECT_EQ(0, reader->read_pointer());
    EXPECT_EQ(propertyListOffset, reader->read_pointer());
    EXPECT_EQ(protocolListOffset, reader->read_pointer());
    EXPECT_EQ(-1, reader->read_short());
    EXPECT_EQ(baseNameOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}

TEST (BinarySerializer_SerializationTests, TestSerialization_InterfaceWithConstructor) {
    binary::MetaFile* file = new binary::MetaFile();
    binary::BinarySerializer* serializer = new binary::BinarySerializer(file);

    meta::InterfaceMeta* target = new meta::InterfaceMeta();
    target->name = "foo";
    target->jsName = "foo";
    target->module = "Foundation.foo";
    target->flags = meta::MetaFlags::IsIosAppExtensionAvailable;
    target->introducedIn = { .Major = 2, .Minor = 0, .SubMinor = -1 };
    target->protocols.push_back({ .name = "fooProtocol", .module = "Foundation.foo" });
    target->baseName = { .name = "fooBase", .module = "Foundation.foo" };

    meta::MethodMeta method1;
    method1.name = "init";
    method1.jsName = "init";
    method1.selector = "init";
    method1.typeEncoding = "@:";
    target->instanceMethods.push_back(std::move(method1));

    utils::MetaContainer container;
    container.add(std::unique_ptr<meta::Meta>(target));
    container.serialize(serializer);

    /// Check heap

    std::shared_ptr<binary::BinaryReader> reader = file->heap_reader();
    reader->baseStream()->set_position(0);
    EXPECT_EQ(0, reader->read_byte()); // marking byte
    int moduleOffset = reader->baseStream()->position();
    EXPECT_EQ("Foundation", reader->read_string());
    int nameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->name, reader->read_string());
    int methodNameOffset = reader->baseStream()->position();
    EXPECT_EQ(method1.name, reader->read_string());
    int methodTypeEncodingOffset = reader->baseStream()->position();
    EXPECT_EQ(method1.typeEncoding, reader->read_string());

    // MethodMeta
    int methodOffset = reader->baseStream()->position();
    EXPECT_EQ(methodNameOffset, reader->read_pointer());
    EXPECT_EQ(method1.flags, reader->read_byte());
    EXPECT_EQ(0, reader->read_short()); // module id
    EXPECT_EQ(0, reader->read_byte()); // introduced in
    EXPECT_EQ(methodNameOffset, reader->read_pointer()); // selector
    EXPECT_EQ(0, reader->read_pointer()); // encoding
    EXPECT_EQ(methodTypeEncodingOffset, reader->read_pointer()); // compiler encoding

    int methodListOffset = reader->baseStream()->position();
    EXPECT_EQ(target->instanceMethods.size(), reader->read_arrayCount());
    EXPECT_EQ(methodOffset, reader->read_pointer());

    // protocols
    int protocolNameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->protocols[0].name, reader->read_string());

    int protocolListOffset = reader->baseStream()->position();
    EXPECT_EQ(target->protocols.size(), reader->read_arrayCount());
    EXPECT_EQ(protocolNameOffset, reader->read_pointer());

    // base name
    int baseNameOffset = reader->baseStream()->position();
    EXPECT_EQ(target->baseName.name, reader->read_string());

    // InterfaceMeta
    EXPECT_EQ(file->getFromGlobalTable(target->jsName), reader->baseStream()->position());
    EXPECT_EQ(nameOffset, reader->read_pointer());
    uint8_t flags = reader->read_byte();
    EXPECT_EQ(target->flags | meta::SymbolType::Interface, flags);
    EXPECT_EQ(meta::SymbolType::Interface, flags & 7);
    EXPECT_EQ(moduleOffset, reader->read_short());
    EXPECT_EQ(2 << 3, reader->read_byte());
    EXPECT_EQ(methodListOffset, reader->read_pointer());
    EXPECT_EQ(0, reader->read_pointer());
    EXPECT_EQ(0, reader->read_pointer());
    EXPECT_EQ(protocolListOffset, reader->read_pointer());
    EXPECT_EQ(0, reader->read_short());
    EXPECT_EQ(baseNameOffset, reader->read_pointer());

    EXPECT_EQ(reader->baseStream()->size(), reader->baseStream()->position());
}
