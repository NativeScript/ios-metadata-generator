namespace TypeScript.Declarations.Writers
{
    using System;

    internal class Indent
    {
        private string text;

        public Indent()
        {
            this.text = string.Empty;
        }

        public static implicit operator string(Indent indent)
        {
            return indent.text;
        }

        public void Increase()
        {
            this.text += "\t";
        }

        public void Decrease()
        {
            this.text = this.text.Substring(0, this.text.Length - 1);
        }

        public override string ToString()
        {
            return this.text;
        }

        public IDisposable EnterScope()
        {
            this.Increase();
            return new IndentScope(this);
        }

        private sealed class IndentScope : IDisposable
        {
            private Indent owner;

            public IndentScope(Indent owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException("The indent scope has already been disposed.");
                }

                this.owner.Decrease();
                this.owner = null;
            }
        }
    }
}
