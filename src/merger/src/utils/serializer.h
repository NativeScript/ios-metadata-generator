#ifndef SERIALIZER_H
#define SERIALIZER_H

/// forward declarations
namespace meta {
    class InterfaceMeta;
    class ProtocolMeta;
    class CategoryMeta;
    class FunctionMeta;
    class StructMeta;
    class UnionMeta;
    class JsCodeMeta;
    class VarMeta;
}
namespace typeEncoding {
    class UnknownEncoding;
    class VoidEncoding;
    class BoolEncoding;
    class ShortEncoding;
    class UShortEncoding;
    class IntEncoding;
    class UIntEncoding;
    class LongEncoding;
    class ULongEncoding;
    class LongLongEncoding;
    class ULongLongEncoding;
    class SignedCharEncoding;
    class UnsignedCharEncoding;
    class UnicharEncoding;
    class CStringEncoding;
    class FloatEncoding;
    class DoubleEncoding;
    class VaListEncoding;
    class SelectorEncoding;
    class InstancetypeEncoding;
    class ClassEncoding;
    class ProtocolEncoding;
    class IdEncoding;
    class ConstantArrayEncoding;
    class IncompleteArrayEncoding;
    class InterfaceEncoding;
    class PointerEncoding;
    class BlockEncoding;
    class FunctionEncoding;
    class StructEncoding;
    class UnionEncoding;
    class InterfaceDeclarationEncoding;
    class AnonymousStructEncoding;
    class AnonymousUnionEncoding;
}

namespace utils {
    class MetaContainer;

    /*
     * \class Serializer
     * \brief Applies the Visitor pattern for serializing \c meta::Meta objects.
     */
    class Serializer {
    public:
        /*
         * \brief Called before serialization of meta objects in the specified container begins.
         */
        virtual void start(MetaContainer* container) = 0;

        /*
         * \brief Called after serialization of meta objects in the specified container.
         */
        virtual void finish(MetaContainer* container) = 0;

        virtual void serialize(meta::InterfaceMeta* meta) = 0;

        virtual void serialize(meta::ProtocolMeta* meta) = 0;

        virtual void serialize(meta::CategoryMeta* meta) = 0;

        virtual void serialize(meta::FunctionMeta* meta) = 0;

        virtual void serialize(meta::StructMeta* meta) = 0;

        virtual void serialize(meta::UnionMeta* meta) = 0;

        virtual void serialize(meta::JsCodeMeta* meta) = 0;

        virtual void serialize(meta::VarMeta* meta) = 0;
    };

    /*
     * \class TypeEncodingSerializer<T>
     * \brief Applies the Visitor pattern for serializing \c typeEncoding::TypeEncoding objects.
     *
     * Returns a value of type \c T_RESULT
     */
    template <typename T_RESULT>
    class TypeEncodingSerializer {
    public:
        virtual T_RESULT serialize(typeEncoding::UnknownEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::VoidEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::BoolEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ShortEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::UShortEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::IntEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::UIntEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::LongEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ULongEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::LongLongEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ULongLongEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::SignedCharEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::UnsignedCharEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::UnicharEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::CStringEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::FloatEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::DoubleEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::VaListEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::SelectorEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::InstancetypeEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ClassEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ProtocolEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::IdEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::ConstantArrayEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::IncompleteArrayEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::InterfaceEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::PointerEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::BlockEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::FunctionEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::StructEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::UnionEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::InterfaceDeclarationEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::AnonymousStructEncoding* encoding) = 0;

        virtual T_RESULT serialize(typeEncoding::AnonymousUnionEncoding* encoding) = 0;
    };
}

#endif
