namespace TypeScript.Declarations.Writers
{
    internal partial class TypeWriter
    {
        private void WriteSemicolonSeparator()
        {
            this.Write("; ");
        }

        private void WriteCommaSeparator()
        {
            this.Write(", ");
        }

        private void WriteTypeAnnotationColon()
        {
            this.Write(": ");
        }

        private void WriteTypeAnnotationArrow()
        {
            this.Write(" => ");
        }

        private void WriteQuestionMark()
        {
            this.Write("?");
        }

        private void WriteClosingCurlyBracket()
        {
            this.Write("}");
        }

        private void WriteClosingParenthesis()
        {
            this.Write(")");
        }

        private void WriteOpeningCurlyBracket()
        {
            this.Write("{");
        }

        private void WriteOpeningParenthesis()
        {
            this.Write("(");
        }

        private void WriteArrayBrackets()
        {
            this.WriteOpeningSquareBracket();
            this.WriteClosingSquareBracket();
        }

        private void WriteOpeningSquareBracket()
        {
            this.Write("[");
        }

        private void WriteClosingSquareBracket()
        {
            this.Write("]");
        }

        private void WriteAny()
        {
            this.Write("any");
        }

        private void WriteString()
        {
            this.Write("string");
        }

        private void WriteNumber()
        {
            this.Write("number");
        }

        private void WriteBoolean()
        {
            this.Write("boolean");
        }

        private void WriteVoid()
        {
            this.Write("void");
        }
    }
}
