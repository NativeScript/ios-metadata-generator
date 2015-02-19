namespace TypeScript.Declarations.Writers
{
    //// TODO: We have a lot more tokens to add here. We also should try to reuse the same tokens list as in the TypeWriter.Tokens
    internal partial class DeclarationWriter
    {
        private void WriteCommaSeparatorNewLine()
        {
            this.WriteLine(",");
        }

        private void WriteCommaSeparator()
        {
            this.Write(", ");
        }

        private void WriteTypeAnnotationColon()
        {
            this.Write(": ");
        }

        private void WriteSpace()
        {
            this.Write(" ");
        }

        private void WriteQuestionMark()
        {
            this.Write("?");
        }

        private void WriteSpaceSuffixedImplement()
        {
            this.WriteImplement();
            this.WriteSpace();
        }

        private void WriteImplement()
        {
            this.Write("implements");
        }
    }
}
