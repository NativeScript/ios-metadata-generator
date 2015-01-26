namespace TypeScript.Declarations
{
    using System.Linq;
    using TypeScript.Declarations.Model;

    public static class ModelExtensions
    {
        public static MethodSignature Clone(this MethodSignature @this)
        {
            if (@this == null)
            {
                return null;
            }

            var clone = new MethodSignature()
            {
                Name = @this.Name,
                IsStatic = @this.IsStatic,
                ReturnType = @this.ReturnType
            };

            clone.Parameters.AddRange(@this.Parameters.Select(ModelExtensions.Clone));
            clone.Annotations.AddRange(@this.Annotations);

            return clone;
        }

        public static Parameter Clone(this Parameter @this)
        {
            if (@this == null)
            {
                return null;
            }

            var clone = new Parameter()
            {
                Name = @this.Name,
                TypeAnnotation = @this.TypeAnnotation
            };

            clone.Annotations.AddRange(@this.Annotations);

            return clone;
        }

        public static PropertySignature Clone(this PropertySignature @this)
        {
            if (@this == null)
            {
                return null;
            }

            var clone = new PropertySignature()
            {
                Name = @this.Name,
                IsOptional = @this.IsOptional,
                TypeAnnotation = @this.TypeAnnotation
            };

            clone.Annotations.AddRange(@this.Annotations);

            return clone;
        }
    }
}
