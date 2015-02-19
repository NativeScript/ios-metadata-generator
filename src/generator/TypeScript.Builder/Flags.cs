namespace TypeScript.Factory
{
    using TypeScript.Declarations.Model;

    internal static class Flags
    {
        private static readonly object ReturnsInstancetypeKey = new object();

        public static void SetReturnsInstancetype(this MethodSignature method)
        {
            method.Annotations.Add(ReturnsInstancetypeKey);
        }

        public static bool ReturnsInstancetype(this MethodSignature method)
        {
            return method.Annotations.Contains(ReturnsInstancetypeKey);
        }
    }
}
