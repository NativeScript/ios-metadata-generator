#pragma once

#include "Meta/MetaEntities.h"
#include "metaFile.h"
#include "binaryTypeEncodingSerializer.h"
#include <map>

namespace binary {
    
enum FFIType : uint8_t {
    FFIVoid,
    FFIPointer,
    FFISint8,
    FFIUint8,
    FFIUint16,
    FFISint16,
    FFIUint32,
    FFISint32,
    FFIUint64,
    FFISint64,
    FFIUshort,
    FFIDouble,
    FFIStruct,
    FFIFloat
};
    
struct CachedSignature {
    
    MetaFileOffset offset;
    std::vector<FFIType> types;
    
};
    
    
/*
     * \class BinarySerializer
     * \brief Applies the Visitor pattern for serializing \c Meta::Meta objects in binary format.
     */
class BinarySerializer : public ::Meta::MetaVisitor {
private:
    MetaFile* file;
    BinaryWriter heapWriter;
    BinaryTypeEncodingSerializer typeEncodingSerializer;
    
    std::vector<CachedSignature> cachedSignatures;
    
    binary::MetaFileOffset checkForExistingSignature(std::vector<::Meta::Type*> signature);

    void serializeBase(::Meta::Meta* Meta, binary::Meta& binaryMetaStruct);

    void serializeBaseClass(::Meta::BaseClassMeta* Meta, binary::BaseClassMeta& binaryMetaStruct);

    void serializeMethod(::Meta::MethodMeta* Meta, binary::MethodMeta& binaryMetaStruct);

    void serializeProperty(::Meta::PropertyMeta* Meta, binary::PropertyMeta& binaryMetaStruct);

    void serializeRecord(::Meta::RecordMeta* Meta, binary::RecordMeta& binaryMetaStruct);

    void serializeModule(clang::Module* module, binary::ModuleMeta& binaryMetaStruct);

    void serializeLibrary(clang::Module::LinkLibrary* library, binary::LibraryMeta& binaryLib);

public:
    BinarySerializer(MetaFile* file)
        : heapWriter(file->heap_writer())
        , typeEncodingSerializer(heapWriter)
    {
        this->file = file;
    }

    void serializeContainer(std::vector<std::pair<clang::Module*, std::vector< ::Meta::Meta*> > >& container);

    void start(std::vector<std::pair<clang::Module*, std::vector< ::Meta::Meta*> > >& container);

    void finish(std::vector<std::pair<clang::Module*, std::vector< ::Meta::Meta*> > >& container);

    virtual void visit(::Meta::InterfaceMeta* Meta) override;

    virtual void visit(::Meta::ProtocolMeta* Meta) override;

    virtual void visit(::Meta::CategoryMeta* Meta) override;

    virtual void visit(::Meta::FunctionMeta* Meta) override;

    virtual void visit(::Meta::StructMeta* Meta) override;

    virtual void visit(::Meta::UnionMeta* Meta) override;

    virtual void visit(::Meta::EnumMeta* Meta) override;

    virtual void visit(::Meta::VarMeta* Meta) override;

    virtual void visit(::Meta::EnumConstantMeta* Meta) override;

    virtual void visit(::Meta::PropertyMeta* Meta) override;

    virtual void visit(::Meta::MethodMeta* Meta) override;
};
}
