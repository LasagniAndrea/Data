using System;
using Fonet.DataTypes;
using Fonet.Fo.Properties;
using Fonet.Layout;

namespace Fonet.Fo
{
    internal class FOText : FONode
    {
        protected char[] ca;
        protected int start;
        protected int length;

        private FontState fs;
        private float red;
        private float green;
        private float blue;
        private int wrapOption;
        private int whiteSpaceCollapse;
        private int verticalAlign;

        protected bool underlined = false;
        protected bool overlined = false;
        protected bool lineThrough = false;

        private TextState ts;

        public FOText(char[] chars, int s, int e, FObj parent)
            : base(parent)
        {
            this.start = 0;
            this.ca = new char[e - s];
            for (int i = s; i < e; i++)
            {
                ca[i - s] = chars[i];
            }
            this.length = e - s;
        }

        public void SetUnderlined(bool ul)
        {
            this.underlined = ul;
        }

        public void SetOverlined(bool ol)
        {
            this.overlined = ol;
        }

        public void SetLineThrough(bool lt)
        {
            this.lineThrough = lt;
        }

        public bool WillCreateArea()
        {
            this.whiteSpaceCollapse =
                this.parent.properties.GetProperty("white-space-collapse").GetEnum();
            if (this.whiteSpaceCollapse == WhiteSpaceCollapse.FALSE
                && length > 0)
            {
                return true;
            }

            for (int i = start; i < start + length; i++)
            {
                char ch = ca[i];
                if (!((ch == ' ') || (ch == '\n') || (ch == '\r')
                    || (ch == '\t')))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool MayPrecedeMarker()
        {
            for (int i = 0; i < ca.Length; i++)
            {
                char ch = ca[i];
                if ((ch != ' ') || (ch != '\n') || (ch != '\r') || (ch != '\t'))
                {
                    return true;
                }
            }
            return false;
        }

        public override Status Layout(Area area)
        {
            if (!(area is BlockArea))
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "Text outside block area" + new String(ca, start, length));
                return new Status(Status.OK);
            }
            if (this.marker == MarkerStart)
            {
                string fontFamily =
                    this.parent.properties.GetProperty("font-family").GetString();
                string fontStyle =
                    this.parent.properties.GetProperty("font-style").GetString();
                string fontWeight =
                    this.parent.properties.GetProperty("font-weight").GetString();
                int fontSize =
                    this.parent.properties.GetProperty("font-size").GetLength().MValue();
                int fontVariant =
                    this.parent.properties.GetProperty("font-variant").GetEnum();

                int letterSpacing =
                    this.parent.properties.GetProperty("letter-spacing").GetLength().MValue();
                this.fs = new FontState(area.GetFontInfo(), fontFamily,
                                        fontStyle, fontWeight, fontSize,
                                        fontVariant, letterSpacing);

                ColorType c = this.parent.properties.GetProperty("color").GetColorType();
                this.red = c.Red;
                this.green = c.Green;
                this.blue = c.Blue;

                this.verticalAlign =
                    this.parent.properties.GetProperty("vertical-align").GetEnum();

                this.wrapOption =
                    this.parent.properties.GetProperty("wrap-option").GetEnum();
                this.whiteSpaceCollapse =
                    this.parent.properties.GetProperty("white-space-collapse").GetEnum();
                this.ts = new TextState();
                ts.SetUnderlined(underlined);
                ts.SetOverlined(overlined);
                ts.SetLineThrough(lineThrough);

                this.marker = this.start;
            }
            int orig_start = this.marker;
            this.marker = AddText((BlockArea)area, fs, red, green, blue,
                                  wrapOption, this.GetLinkSet(),
                                  whiteSpaceCollapse, ca, this.marker, length,
                                  ts, verticalAlign);
            if (this.marker == -1)
            {
                return new Status(Status.OK);
            }
            else if (this.marker != orig_start)
            {
                return new Status(Status.AREA_FULL_SOME);
            }
            else
            {
                return new Status(Status.AREA_FULL_NONE);
            }
        }

        public static int AddText(BlockArea ba, FontState fontState, float red,
                                  float green, float blue, int wrapOption,
                                  LinkSet ls, int whiteSpaceCollapse,
                                  char[] data, int start, int end,
                                  TextState textState, int vAlign)
        {
            if (fontState.FontVariant == FontVariant.SMALL_CAPS)
            {
                FontState smallCapsFontState;
                try
                {
                    int smallCapsFontHeight =
                        (int)(((double)fontState.FontSize) * 0.8d);
                    smallCapsFontState = new FontState(fontState.FontInfo,
                                                       fontState.FontFamily,
                                                       fontState.FontStyle,
                                                       fontState.FontWeight,
                                                       smallCapsFontHeight,
                                                       FontVariant.NORMAL);
                }
                catch (FonetException ex)
                {
                    smallCapsFontState = fontState;
                    FonetDriver.ActiveDriver.FireFonetError(
                        "Error creating small-caps FontState: " + ex.Message);
                }

                char c;
                bool isLowerCase;
                int caseStart;
                FontState fontStateToUse;
                for (int i = start; i < end; )
                {
                    caseStart = i;
                    c = data[i];
                    isLowerCase = (Char.IsLetter(c) && Char.IsLower(c));
                    while (isLowerCase == (Char.IsLetter(c) && Char.IsLower(c)))
                    {
                        if (isLowerCase)
                        {
                            data[i] = Char.ToUpper(c);
                        }
                        i++;
                        if (i == end)
                        {
                            break;
                        }
                        c = data[i];
                    }
                    if (isLowerCase)
                    {
                        fontStateToUse = smallCapsFontState;
                    }
                    else
                    {
                        fontStateToUse = fontState;
                    }
                    int index = AddRealText(ba, fontStateToUse, red, green, blue,
                                            wrapOption, ls, whiteSpaceCollapse,
                                            data, caseStart, i, textState,
                                            vAlign);
                    if (index != -1)
                    {
                        return index;
                    }
                }

                return -1;
            }

            return AddRealText(ba, fontState, red, green, blue, wrapOption, ls,
                               whiteSpaceCollapse, data, start, end, textState,
                               vAlign);
        }

        protected static int AddRealText(BlockArea ba, FontState fontState,
                                         float red, float green, float blue,
                                         int wrapOption, LinkSet ls,
                                         int whiteSpaceCollapse, char[] data,
                                         int start, int end, TextState textState,
                                         int vAlign)
        {
            int ts, te;
            char[] ca;

            ts = start;
            te = end;
            ca = data;

            LineArea la = ba.GetCurrentLineArea();
            if (la == null)
            {
                return start;
            }

            la.ChangeFont(fontState);
            la.ChangeColor(red, green, blue);
            la.ChangeWrapOption(wrapOption);
            la.ChangeWhiteSpaceCollapse(whiteSpaceCollapse);
            la.ChangeVerticalAlign(vAlign);
            ba.SetupLinkSet(ls);

            ts = la.AddText(ca, ts, te, ls, textState);

            while (ts != -1)
            {
                la = ba.CreateNextLineArea();
                if (la == null)
                {
                    return ts;
                }
                la.ChangeFont(fontState);
                la.ChangeColor(red, green, blue);
                la.ChangeWrapOption(wrapOption);
                la.ChangeWhiteSpaceCollapse(whiteSpaceCollapse);
                ba.SetupLinkSet(ls);

                ts = la.AddText(ca, ts, te, ls, textState);
            }
            return -1;
        }
    }
}