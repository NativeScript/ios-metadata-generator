#pragma once

namespace Meta {

    struct ClassTypeDetails;
    struct IdTypeDetails;
    struct ConstantArrayTypeDetails;
    struct IncompleteArrayTypeDetails;
    struct InterfaceTypeDetails;
    struct BridgedInterfaceTypeDetails;
    struct PointerTypeDetails;
    struct BlockTypeDetails;
    struct FunctionPointerTypeDetails;
    struct StructTypeDetails;
    struct UnionTypeDetails;
    struct PureInterfaceTypeDetails;
    struct AnonymousStructTypeDetails;
    struct AnonymousUnionTypeDetails;

    /*
     * \class TypeVisitor<T>
     * \brief Applies the Visitor pattern for \c Meta::Type objects.
     *
     * Returns a value of type \c T_RESULT
     */
    template <typename T_RESULT>
    class TypeVisitor {
    public:
        virtual T_RESULT visitUnknown() = 0;

        virtual T_RESULT visitVoid() = 0;

        virtual T_RESULT visitBool() = 0;

        virtual T_RESULT visitShort() = 0;

        virtual T_RESULT visitUShort() = 0;

        virtual T_RESULT visitInt() = 0;

        virtual T_RESULT visitUInt() = 0;

        virtual T_RESULT visitLong() = 0;

        virtual T_RESULT visitUlong() = 0;

        virtual T_RESULT visitLongLong() = 0;

        virtual T_RESULT visitULongLong() = 0;

        virtual T_RESULT visitSignedChar() = 0;

        virtual T_RESULT visitUnsignedChar() = 0;

        virtual T_RESULT visitUnichar() = 0;

        virtual T_RESULT visitCString() = 0;

        virtual T_RESULT visitFloat() = 0;

        virtual T_RESULT visitDouble() = 0;

        virtual T_RESULT visitVaList() = 0;

        virtual T_RESULT visitSelector() = 0;

        virtual T_RESULT visitInstancetype() = 0;

        virtual T_RESULT visitClass(ClassTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitProtocol() = 0;

        virtual T_RESULT visitId(IdTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitConstantArray(ConstantArrayTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitIncompleteArray(IncompleteArrayTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitInterface(InterfaceTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitBridgedInterface(BridgedInterfaceTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitPointer(PointerTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitBlock(BlockTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitFunctionPointer(FunctionPointerTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitStruct(StructTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitUnion(UnionTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitPureInterface(PureInterfaceTypeDetails& typeDetails) = 0; // TODO: Remove it

        virtual T_RESULT visitAnonymousStruct(AnonymousStructTypeDetails& typeDetails) = 0;

        virtual T_RESULT visitAnonymousUnion(AnonymousUnionTypeDetails& typeDetails) = 0;
    };
}