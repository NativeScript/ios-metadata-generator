namespace TypeScript.Declarations.Model
{
    public static class PrimitiveTypes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The Primitive type used is truly immutable.")]
        public static readonly IType String = new StringPrimitive();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The Primitive type used is truly immutable.")]
        public static readonly IType Number = new NumberPrimitive();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The Primitive type used is truly immutable.")]
        public static readonly IType Boolean = new BooleanPrimitive();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The Primitive type used is truly immutable.")]
        public static readonly IType Void = new VoidPrimitive();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The Primitive type used is truly immutable.")]
        public static readonly IType Any = new AnyPrimitive();

        private class StringPrimitive : IType
        {
            public override string ToString()
            {
                return "string";
            }

            public void Accept(TypeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new System.ArgumentNullException("visitor");
                }

                visitor.VisitString();
            }
        }

        private class NumberPrimitive : IType
        {
            public override string ToString()
            {
                return "number";
            }

            public void Accept(TypeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new System.ArgumentNullException("visitor");
                }

                visitor.VisitNumber();
            }
        }

        private class BooleanPrimitive : IType
        {
            public override string ToString()
            {
                return "boolean";
            }

            public void Accept(TypeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new System.ArgumentNullException("visitor");
                }

                visitor.VisitBoolean();
            }
        }

        private class VoidPrimitive : IType
        {
            public override string ToString()
            {
                return "void";
            }

            public void Accept(TypeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new System.ArgumentNullException("visitor");
                }

                visitor.VisitVoid();
            }
        }

        private class AnyPrimitive : IType
        {
            public override string ToString()
            {
                return "any";
            }

            public void Accept(TypeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new System.ArgumentNullException("visitor");
                }

                visitor.VisitAny();
            }
        }
    }
}
