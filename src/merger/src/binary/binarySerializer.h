#ifndef BINARYSERIALIZER_H
#define BINARYSERIALIZER_H

#include "../utils/serializer.h"
#include "../meta/meta.h"
#include "metaFile.h"
#include "binaryTypeEncodingSerializer.h"
#include "../utils/metaContainer.h"
#include <map>

namespace binary {
    /*
     * \class BinarySerializer
     * \brief Applies the Visitor pattern for serializing \c meta::Meta objects in binary format.
     */
    class BinarySerializer : public utils::Serializer {
    private:
        MetaFile* file;
        std::shared_ptr<BinaryWriter> heapWriter;
        BinaryTypeEncodingSerializer typeEncodingSerializer;
        std::map<std::string, MetaFileOffset> moduleMap;

        void serializeBase(meta::Meta* meta, binary::Meta& binaryMetaStruct);

        void serializeBaseClass(meta::BaseClassMeta* meta, binary::BaseClassMeta& binaryMetaStruct);

        void serializeMethod(meta::MethodMeta* meta, binary::MethodMeta& binaryMetaStruct);

        void serializeProperty(meta::PropertyMeta* meta, binary::PropertyMeta& binaryMetaStruct);

        void serializeRecord(meta::RecordMeta* meta, binary::RecordMeta& binaryMetaStruct);

    public:
        BinarySerializer(MetaFile* file) : heapWriter(file->heap_writer()), typeEncodingSerializer(heapWriter) {
            this->file = file;
        }

        virtual void start(utils::MetaContainer *container) override;

        virtual void finish(utils::MetaContainer *container) override;

        virtual void serialize(meta::InterfaceMeta* meta) override;

        virtual void serialize(meta::ProtocolMeta* meta) override;

        virtual void serialize(meta::CategoryMeta* meta) override;

        virtual void serialize(meta::FunctionMeta* meta) override;

        virtual void serialize(meta::StructMeta* meta) override;

        virtual void serialize(meta::UnionMeta* meta) override;

        virtual void serialize(meta::JsCodeMeta* meta) override;

        virtual void serialize(meta::VarMeta* meta) override;
    };
}

#endif