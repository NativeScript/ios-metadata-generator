#pragma once

#include "MetaFile.h"
#include "binaryTypeEncodingSerializer.h"
#include <map>
#include "../Meta/MetaEntities.h"

namespace binary {
    /*
     * \class BinarySerializer
     * \brief Applies the Visitor pattern for serializing \c Meta::Meta objects in binary format.
     */
    class BinarySerializer : public ::Meta::MetaVisitor {
    private:
        MetaFile* file;
        BinaryWriter heapWriter;
        BinaryTypeEncodingSerializer typeEncodingSerializer;
        std::map<std::string, MetaFileOffset> moduleMap;

        void serializeBase(::Meta::Meta* Meta, binary::Meta& binaryMetaStruct);

        void serializeBaseClass(::Meta::BaseClassMeta* Meta, binary::BaseClassMeta& binaryMetaStruct);

        void serializeMethod(::Meta::MethodMeta* Meta, binary::MethodMeta& binaryMetaStruct);

        void serializeProperty(::Meta::PropertyMeta* Meta, binary::PropertyMeta& binaryMetaStruct);

        void serializeRecord(::Meta::RecordMeta* Meta, binary::RecordMeta& binaryMetaStruct);

    public:
        BinarySerializer(MetaFile* file) : heapWriter(file->heap_writer()), typeEncodingSerializer(heapWriter) {
            this->file = file;
        }

        void serializeContainer(::Meta::MetaContainer& container);

        void start(::Meta::MetaContainer *container);

        void finish(::Meta::MetaContainer *container);

        virtual void visit(::Meta::InterfaceMeta* Meta) override;

        virtual void visit(::Meta::ProtocolMeta* Meta) override;

        virtual void visit(::Meta::CategoryMeta* Meta) override;

        virtual void visit(::Meta::FunctionMeta* Meta) override;

        virtual void visit(::Meta::StructMeta* Meta) override;

        virtual void visit(::Meta::UnionMeta* Meta) override;

        virtual void visit(::Meta::JsCodeMeta* Meta) override;

        virtual void visit(::Meta::VarMeta* Meta) override;
    };
}