namespace Fonet.Layout
{
    internal class TextState
    {
        protected bool underlined;

        protected bool overlined;

        protected bool linethrough;

        public TextState()
        {
        }

        public bool GetUnderlined()
        {
            return underlined;
        }

        public void SetUnderlined(bool ul)
        {
            this.underlined = ul;
        }

        public bool GetOverlined()
        {
            return overlined;
        }

        public void SetOverlined(bool ol)
        {
            this.overlined = ol;
        }

        public bool GetLineThrough()
        {
            return linethrough;
        }

        public void SetLineThrough(bool lt)
        {
            this.linethrough = lt;
        }

    }
}