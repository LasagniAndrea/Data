namespace Fonet.Layout
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Text;
    using Fonet.Fo.Flow;
    using Fonet.Fo.Properties;
    using Fonet.Layout.Inline;
    using Fonet.Render.Pdf;
    using Fonet.Util;

    internal class LineArea : Area
    {
        protected int lineHeight;
        protected int halfLeading;
        protected int nominalFontSize;
        protected int nominalGlyphHeight;
        protected int allocationHeight;
        protected int startIndent;
        protected int endIndent;
        private readonly int placementOffset;
        private FontState currentFontState;
        private float red, green, blue;
        private int wrapOption;
        private int whiteSpaceCollapse;
        private int vAlign;
        private HyphenationProps hyphProps;
        protected int finalWidth = 0;
        protected int embeddedLinkStart = 0;
        protected const int NOTHING = 0;
        protected const int WHITESPACE = 1;
        protected const int TEXT = 2;
        protected const int MULTIBYTECHAR = 3;
        protected int prev = NOTHING;
        protected int spaceWidth = 0;
        protected ArrayList pendingAreas = new ArrayList();
        protected int pendingWidth = 0;
        protected bool prevUlState = false;
        protected bool prevOlState = false;
        protected bool prevLTState = false;

        public LineArea(FontState fontState, int lineHeight, int allocationWidth, int startIndent, int endIndent, LineArea prevLineArea)
            : base(fontState)
        {
            this.currentFontState = fontState;
            this.lineHeight = lineHeight;
            this.nominalFontSize = fontState.FontSize;
            this.nominalGlyphHeight = fontState.Ascender - fontState.Descender;

            this.placementOffset = fontState.Ascender;
            this.contentRectangleWidth = allocationWidth - startIndent
                - endIndent;
            this.fontState = fontState;

            this.allocationHeight = this.nominalGlyphHeight;
            this.halfLeading = this.lineHeight - this.allocationHeight;

            this.startIndent = startIndent;
            this.endIndent = endIndent;

            if (prevLineArea != null)
            {
                IEnumerator e = prevLineArea.pendingAreas.GetEnumerator();
                Box b = null;
                bool eatMoreSpace = true;
                int eatenWidth = 0;

                while (eatMoreSpace)
                {
                    if (e.MoveNext())
                    {
                        b = (Box)e.Current;
                        if (b is InlineSpace isp)
                        {
                            if (isp.IsEatable())
                            {
                                eatenWidth += isp.GetSize();
                            }
                            else
                            {
                                eatMoreSpace = false;
                            }
                        }
                        else
                        {
                            eatMoreSpace = false;
                        }
                    }
                    else
                    {
                        eatMoreSpace = false;
                        b = null;
                    }
                }

                while (b != null)
                {
                    pendingAreas.Add(b);
                    if (e.MoveNext())
                    {
                        b = (Box)e.Current;
                    }
                    else
                    {
                        b = null;
                    }
                }
                pendingWidth = prevLineArea.GetPendingWidth() - eatenWidth;
            }
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderLineArea(this);
        }

        public int AddPageNumberCitation(string refid)
        {
            int width = currentFontState.GetWidth(currentFontState.MapCharacter(' '));


            PageNumberInlineArea pia = new PageNumberInlineArea(currentFontState, this.red, this.green, this.blue, refid, width);

            pia.SetYOffset(placementOffset);
            pendingAreas.Add(pia);
            pendingWidth += width;
            prev = TEXT;

            return -1;
        }

        public int AddText(char[] odata, int start, int end, LinkSet ls,
                           TextState textState)
        {
            if (start == -1)
            {
                return -1;
            }
            bool overrun = false;

            int wordStart = start;
            int wordLength = 0;
            int wordWidth = 0;
            int whitespaceWidth = GetCharWidth(' ');

            char[] data = new char[odata.Length];
            char[] dataCopy = new char[odata.Length];
            Array.Copy(odata, data, odata.Length);
            Array.Copy(odata, dataCopy, odata.Length);
            for (int i = start; i < end; i++)
            {
                int charWidth;
                char c = data[i];

                bool isText;
                bool isMultiByteChar;
                if (!(IsSpace(c) || (c == '\n') || (c == '\r') || (c == '\t') || (c == '\u2028')))
                {
                    charWidth = GetCharWidth(c);
                    isText = true;
                    isMultiByteChar = (c > 127);
                    if (charWidth <= 0 && c != '\u200B' && c != '\uFEFF')
                    {
                        charWidth = whitespaceWidth;
                    }
                }
                else
                {
                    if ((c == '\n') || (c == '\r') || (c == '\t'))
                    {
                        charWidth = whitespaceWidth;
                    }
                    else
                    {
                        charWidth = GetCharWidth(c);
                    }

                    isText = false;
                    isMultiByteChar = false;

                    if (prev == WHITESPACE)
                    {
                        if (this.whiteSpaceCollapse == WhiteSpaceCollapse.FALSE)
                        {
                            if (IsSpace(c))
                            {
                                spaceWidth += GetCharWidth(c);
                            }
                            else if (c == '\n' || c == '\u2028')
                            {
                                if (spaceWidth > 0)
                                {
                                    InlineSpace isp = new InlineSpace(spaceWidth);
                                    isp.SetUnderlined(textState.GetUnderlined());
                                    isp.SetOverlined(textState.GetOverlined());
                                    isp.SetLineThrough(textState.GetLineThrough());
                                    AddChild(isp);
                                    finalWidth += spaceWidth;
                                    spaceWidth = 0;
                                }
                                return i + 1;
                            }
                            else if (c == '\t')
                            {
                                spaceWidth += 8 * whitespaceWidth;
                            }
                        }
                        else if (c == '\u2028')
                        {
                            if (spaceWidth > 0)
                            {
                                InlineSpace isp = new InlineSpace(spaceWidth);
                                isp.SetUnderlined(textState.GetUnderlined());
                                isp.SetOverlined(textState.GetOverlined());
                                isp.SetLineThrough(textState.GetLineThrough());
                                AddChild(isp);
                                finalWidth += spaceWidth;
                                spaceWidth = 0;
                            }
                            return i + 1;
                        }

                    }
                    else if (prev == TEXT || prev == MULTIBYTECHAR)
                    {
                        if (spaceWidth > 0)
                        {
                            InlineSpace isp = new InlineSpace(spaceWidth);
                            if (prevUlState)
                            {
                                isp.SetUnderlined(textState.GetUnderlined());
                            }
                            if (prevOlState)
                            {
                                isp.SetOverlined(textState.GetOverlined());
                            }
                            if (prevLTState)
                            {
                                isp.SetLineThrough(textState.GetLineThrough());
                            }
                            AddChild(isp);
                            finalWidth += spaceWidth;
                            spaceWidth = 0;
                        }

                        IEnumerator e = pendingAreas.GetEnumerator();
                        while (e.MoveNext())
                        {
                            Box box = (Box)e.Current;
                            if (box is InlineArea area)
                            {
                                if (ls != null)
                                {
                                    Rectangle lr =
                                        new Rectangle(finalWidth, 0,
                                                      area.GetContentWidth(),
                                                      fontState.FontSize);
                                    ls.AddRect(lr, this, area);
                                }
                            }
                            AddChild(box);
                        }

                        finalWidth += pendingWidth;

                        pendingWidth = 0;
                        pendingAreas = new ArrayList();

                        if (wordLength > 0)
                        {
                            AddSpacedWord(new String(data, wordStart, wordLength),
                                          ls, finalWidth, 0, textState, false);
                            finalWidth += wordWidth;

                            wordWidth = 0;
                        }

                        prev = WHITESPACE;

                        embeddedLinkStart = 0;

                        spaceWidth = GetCharWidth(c);

                        if (this.whiteSpaceCollapse == WhiteSpaceCollapse.FALSE)
                        {
                            if (c == '\n' || c == '\u2028')
                            {
                                return i + 1;
                            }
                            else if (c == '\t')
                            {
                                spaceWidth = whitespaceWidth;
                            }
                        }
                        else if (c == '\u2028')
                        {
                            return i + 1;
                        }
                    }
                    else
                    {
                        if (this.whiteSpaceCollapse == WhiteSpaceCollapse.FALSE)
                        {
                            if (IsSpace(c))
                            {
                                prev = WHITESPACE;
                                spaceWidth = GetCharWidth(c);
                            }
                            else if (c == '\n')
                            {
                                InlineSpace isp = new InlineSpace(spaceWidth);
                                AddChild(isp);
                                return i + 1;
                            }
                            else if (c == '\t')
                            {
                                prev = WHITESPACE;
                                spaceWidth = 8 * whitespaceWidth;
                            }

                        }
                        else
                        {
                            wordStart++;
                        }
                    }

                }

                if (isText)
                {

                    int curr = isMultiByteChar ? MULTIBYTECHAR : TEXT;
                    if (prev == WHITESPACE)
                    {
                        wordWidth = charWidth;
                        if ((finalWidth + spaceWidth + wordWidth)
                            > this.GetContentWidth())
                        {
                            if (overrun)
                            {
                                FonetDriver.ActiveDriver.FireFonetWarning(
                                    "Area contents overflows area");
                            }
                            if (this.wrapOption == WrapOption.WRAP)
                            {
                                return i;
                            }
                        }
                        prev = curr;
                        wordStart = i;
                        wordLength = 1;
                    }
                    else if (prev == TEXT || prev == MULTIBYTECHAR)
                    {
                        if (prev == TEXT && curr == TEXT || !CanBreakMidWord())
                        {
                            wordLength++;
                            wordWidth += charWidth;
                        }
                        else
                        {
                            InlineSpace isp = new InlineSpace(spaceWidth);
                            if (prevUlState)
                            {
                                isp.SetUnderlined(textState.GetUnderlined());
                            }
                            if (prevOlState)
                            {
                                isp.SetOverlined(textState.GetOverlined());
                            }
                            if (prevLTState)
                            {
                                isp.SetLineThrough(textState.GetLineThrough());
                            }
                            AddChild(isp);
                            finalWidth += spaceWidth;
                            spaceWidth = 0;

                            IEnumerator e = pendingAreas.GetEnumerator();
                            while (e.MoveNext())
                            {
                                Box box = (Box)e.Current;
                                if (box is InlineArea area)
                                {
                                    if (ls != null)
                                    {
                                        Rectangle lr =
                                            new Rectangle(finalWidth, 0,
                                                          area.GetContentWidth(),
                                                          fontState.FontSize);
                                        ls.AddRect(lr, this, area);
                                    }
                                }
                                AddChild(box);
                            }

                            finalWidth += pendingWidth;

                            pendingWidth = 0;
                            pendingAreas = new ArrayList();

                            if (wordLength > 0)
                            {
                                AddSpacedWord(new String(data, wordStart, wordLength),
                                              ls, finalWidth, 0, textState, false);
                                finalWidth += wordWidth;
                            }
                            spaceWidth = 0;
                            wordStart = i;
                            wordLength = 1;
                            wordWidth = charWidth;
                        }
                        prev = curr;
                    }
                    else
                    {
                        prev = curr;
                        wordStart = i;
                        wordLength = 1;
                        wordWidth = charWidth;
                    }

                    if ((finalWidth + spaceWidth + pendingWidth + wordWidth)
                        > this.GetContentWidth())
                    {
                        if (this.wrapOption == WrapOption.WRAP)
                        {
                            if (wordStart == start)
                            {
                                overrun = true;
                                if (finalWidth > 0)
                                {
                                    return wordStart;
                                }
                            }
                            else
                            {
                                return wordStart;
                            }

                        }
                    }
                }
            }

            if (prev == TEXT || prev == MULTIBYTECHAR)
            {
                if (spaceWidth > 0)
                {
                    InlineSpace pis = new InlineSpace(spaceWidth);
                    pis.SetEatable(true);
                    if (prevUlState)
                    {
                        pis.SetUnderlined(textState.GetUnderlined());
                    }
                    if (prevOlState)
                    {
                        pis.SetOverlined(textState.GetOverlined());
                    }
                    if (prevLTState)
                    {
                        pis.SetLineThrough(textState.GetLineThrough());
                    }
                    pendingAreas.Add(pis);
                    pendingWidth += spaceWidth;
                    spaceWidth = 0;
                }

                AddSpacedWord(new String(data, wordStart, wordLength), ls,
                              finalWidth + pendingWidth,
                              spaceWidth, textState, true);

                embeddedLinkStart += wordWidth;
            }

            if (overrun)
            {
                FonetDriver.ActiveDriver.FireFonetWarning(
                    "Area contents overflows area");
            }
            return -1;
        }

        public void AddLeader(int leaderPattern, int leaderLengthOptimum, int leaderLengthMaximum,int ruleStyle, int ruleThickness,int leaderPatternWidth, int leaderAlignment)
        {
            WordArea leaderPatternArea;
            int leaderLength = 0;
            char dotIndex = '.';
            int dotWidth = currentFontState.GetWidth(currentFontState.MapCharacter(dotIndex));
            char whitespaceIndex = ' ';
            _ = currentFontState.GetWidth(currentFontState.MapCharacter(whitespaceIndex));

            int remainingWidth = this.GetRemainingWidth();

            if ((remainingWidth <= leaderLengthOptimum)
                || (remainingWidth <= leaderLengthMaximum))
            {
                leaderLength = remainingWidth;
            }
            else if ((remainingWidth > leaderLengthOptimum)
                && (remainingWidth > leaderLengthMaximum))
            {
                leaderLength = leaderLengthMaximum;
            }
            else if ((leaderLengthOptimum > leaderLengthMaximum)
                && (leaderLengthOptimum < remainingWidth))
            {
                leaderLength = leaderLengthOptimum;
            }

            if (leaderLength <= 0)
            {
                return;
            }

            switch (leaderPattern)
            {
                case LeaderPattern.SPACE:
                    InlineSpace spaceArea = new InlineSpace(leaderLength);
                    pendingAreas.Add(spaceArea);
                    break;
                case LeaderPattern.RULE:
                    LeaderArea leaderArea = new LeaderArea(fontState, red, green,
                                                           blue, leaderLength,
                                                           leaderPattern,
                                                           ruleThickness, ruleStyle);
                    leaderArea.SetYOffset(placementOffset);
                    pendingAreas.Add(leaderArea);
                    break;
                case LeaderPattern.DOTS:
                    if (leaderPatternWidth < dotWidth)
                    {
                        leaderPatternWidth = 0;
                    }
                    if (leaderPatternWidth == 0)
                    {
                        pendingAreas.Add(this.BuildSimpleLeader(dotIndex,
                                                                leaderLength));
                    }
                    else
                    {
                        if (leaderAlignment == LeaderAlignment.REFERENCE_AREA)
                        {
                            int spaceBeforeLeader =
                                this.GetLeaderAlignIndent(leaderPatternWidth);
                            if (spaceBeforeLeader != 0)
                            {
                                pendingAreas.Add(new InlineSpace(spaceBeforeLeader,
                                                                 false));
                                pendingWidth += spaceBeforeLeader;
                                leaderLength -= spaceBeforeLeader;
                            }
                        }

                        InlineSpace spaceBetweenDots =
                            new InlineSpace(leaderPatternWidth - dotWidth, false);

                        leaderPatternArea = new WordArea(currentFontState, this.red,
                                                         this.green, this.blue,
                                                         ".", dotWidth);
                        leaderPatternArea.SetYOffset(placementOffset);
                        int dotsFactor =
                            (int)Math.Floor(((double)leaderLength)
                                / ((double)leaderPatternWidth));

                        for (int i = 0; i < dotsFactor; i++)
                        {
                            pendingAreas.Add(leaderPatternArea);
                            pendingAreas.Add(spaceBetweenDots);
                        }
                        pendingAreas.Add(new InlineSpace(leaderLength
                            - dotsFactor
                                * leaderPatternWidth));
                    }
                    break;
                case LeaderPattern.USECONTENT:
                    FonetDriver.ActiveDriver.FireFonetError(
                        "leader-pattern=\"use-content\" not supported by this version of FO.NET");
                    return;
            }
            pendingWidth += leaderLength;
            prev = TEXT;
        }

        public void AddPending()
        {
            if (spaceWidth > 0)
            {
                AddChild(new InlineSpace(spaceWidth));
                finalWidth += spaceWidth;
                spaceWidth = 0;
            }

            foreach (Box box in pendingAreas)
            {
                AddChild(box);
            }

            finalWidth += pendingWidth;

            pendingWidth = 0;
            pendingAreas = new ArrayList();
        }

        public void Align(int type)
        {
            int padding;
            switch (type)
            {
                case TextAlign.START:
                    padding = this.GetContentWidth() - finalWidth;
                    endIndent += padding;
                    break;
                case TextAlign.END:
                    padding = this.GetContentWidth() - finalWidth;
                    startIndent += padding;
                    break;
                case TextAlign.CENTER:
                    padding = (this.GetContentWidth() - finalWidth) / 2;
                    startIndent += padding;
                    endIndent += padding;
                    break;
                case TextAlign.JUSTIFY:
                    int spaceCount = 0;
                    foreach (Box b in children)
                    {
                        if (b is InlineSpace space)
                        {
                            if (space.GetResizeable())
                            {
                                spaceCount++;
                            }
                        }
                    }
                    if (spaceCount > 0)
                    {
                        padding = (this.GetContentWidth() - finalWidth) / spaceCount;
                    }
                    else
                    {
                        padding = 0;
                    }
                    spaceCount = 0;
                    foreach (Box b in children)
                    {
                        if (b is InlineSpace space)
                        {
                            if (space.GetResizeable())
                            {
                                space.SetSize(space.GetSize() + padding);
                                spaceCount++;
                            }
                        }
                        else if (b is InlineArea area)
                        {
                            area.SetXOffset(spaceCount * padding);
                        }

                    }
                    break;
            }
        }

        public void CalcVerticalAlign()
        {
            _ = -this.placementOffset;
            int maxHeight = this.allocationHeight;
            foreach (Box b in children)
            {
                if (b is InlineArea ia)
                {
                    if (ia is WordArea)
                    {
                        ia.SetYOffset(placementOffset);
                    }
                    if (ia.GetHeight() > maxHeight)
                    {
                        maxHeight = ia.GetHeight();
                    }
                    int vert = ia.GetVerticalAlign();
                    if (vert == VerticalAlign.SUPER)
                    {
                        int fh = fontState.Ascender;
                        ia.SetYOffset((int)(placementOffset - (2 * fh / 3.0)));
                    }
                    else if (vert == VerticalAlign.SUB)
                    {
                        int fh = fontState.Ascender;
                        ia.SetYOffset((int)(placementOffset + (2 * fh / 3.0)));
                    }
                }
                else
                {
                }
            }
            this.allocationHeight = maxHeight;
        }

        public void ChangeColor(float red, float green, float blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public void ChangeFont(FontState fontState)
        {
            this.currentFontState = fontState;
        }

        public void ChangeWhiteSpaceCollapse(int whiteSpaceCollapse)
        {
            this.whiteSpaceCollapse = whiteSpaceCollapse;
        }

        public void ChangeWrapOption(int wrapOption)
        {
            this.wrapOption = wrapOption;
        }

        public void ChangeVerticalAlign(int vAlign)
        {
            this.vAlign = vAlign;
        }

        public int GetEndIndent()
        {
            return endIndent;
        }

        public override int GetHeight()
        {
            return this.allocationHeight;
        }

        public int GetPlacementOffset()
        {
            return this.placementOffset;
        }

        public int GetStartIndent()
        {
            return startIndent;
        }

        public bool IsEmpty()
        {
            return !(pendingAreas.Count > 0 || children.Count > 0);
        }

        public ArrayList GetPendingAreas()
        {
            return pendingAreas;
        }

        public int GetPendingWidth()
        {
            return pendingWidth;
        }

        public void SetPendingAreas(ArrayList areas)
        {
            pendingAreas = areas;
        }

        public void SetPendingWidth(int width)
        {
            pendingWidth = width;
        }

        public void ChangeHyphenation(HyphenationProps hyphProps)
        {
            this.hyphProps = hyphProps;
        }

        private InlineArea BuildSimpleLeader(char c, int leaderLength)
        {
            int width = this.currentFontState.GetWidth(currentFontState.MapCharacter(c));
            if (width == 0)
            {
                FonetDriver.ActiveDriver.FireFonetError(
                    "char '" + c + "' has width 0. Using width 100 instead.");
                width = 100;
            }
            int factor = (int)Math.Floor((decimal)leaderLength / width);
            char[] leaderChars = new char[factor];
            for (int i = 0; i < factor; i++)
            {
                leaderChars[i] = c;
            }
            WordArea leaderPatternArea = new WordArea(currentFontState, this.red,
                                                      this.green, this.blue,
                                                      new String(leaderChars),
                                                      leaderLength);
            leaderPatternArea.SetYOffset(placementOffset);
            return leaderPatternArea;
        }

        private int GetLeaderAlignIndent(int leaderPatternWidth)
        {
            double position = GetCurrentXPosition();
            double nextRepeatedLeaderPatternCycle = Math.Ceiling(position
                / leaderPatternWidth);
            double difference =
                (leaderPatternWidth * nextRepeatedLeaderPatternCycle) - position;
            return (int)difference;
        }

        private int GetCurrentXPosition()
        {
            return finalWidth + spaceWidth + startIndent + pendingWidth;
        }

        private string GetHyphenationWord(char[] characters, int wordStart)
        {
            bool wordendFound = false;
            int counter = 0;
            char[] newWord = new char[characters.Length];
            while ((!wordendFound)
                && ((wordStart + counter) < characters.Length))
            {
                char tk = characters[wordStart + counter];
                if (Char.IsLetter(tk))
                {
                    newWord[counter] = tk;
                    counter++;
                }
                else
                {
                    wordendFound = true;
                }
            }
            return new String(newWord, 0, counter);
        }

        private int GetWordWidth(string word)
        {
            if (word == null)
            {
                return 0;
            }
            int width = 0;
            foreach (char c in word)
            {
                width += GetCharWidth(c);
            }
            return width;
        }

        public int GetRemainingWidth()
        {
            return this.GetContentWidth() + startIndent - this.GetCurrentXPosition();
        }

        public void SetLinkSet(LinkSet ls)
        {
        }

        public void AddInlineArea(InlineArea box, LinkSet ls)
        {
            AddPending();
            AddChild(box);
            if (ls != null)
            {
                Rectangle lr = new Rectangle(finalWidth, 0, box.GetContentWidth(), box.GetContentHeight());
                ls.AddRect(lr, this, box);
            }
            prev = TEXT;
            finalWidth += box.GetContentWidth();
        }

        public void AddInlineSpace(InlineSpace isp, int spaceWidth)
        {
            AddChild(isp);
            finalWidth += spaceWidth;
        }

        public int AddCharacter(char data, bool ul)
        {
            int remainingWidth = this.GetRemainingWidth();
            int width =
                this.currentFontState.GetWidth(currentFontState.MapCharacter(data));
            if (width > remainingWidth)
            {
                return Character.DOESNOT_FIT;
            }
            else
            {
                if (Char.IsWhiteSpace(data)
                    && whiteSpaceCollapse == WhiteSpaceCollapse.TRUE)
                {
                    return Character.OK;
                }
                WordArea ia = new WordArea(currentFontState, this.red, this.green,
                      this.blue, data.ToString(),
                      width);
                ia.SetYOffset(placementOffset);
                ia.SetUnderlined(ul);
                pendingAreas.Add(ia);
                if (Char.IsWhiteSpace(data))
                {
                    this.spaceWidth = +width;
                    prev = LineArea.WHITESPACE;
                }
                else
                {
                    pendingWidth += width;
                    prev = LineArea.TEXT;
                }
                return Character.OK;
            }
        }

        private void AddMapWord(char startChar, StringBuilder wordBuf)
        {
            StringBuilder mapBuf = new StringBuilder(wordBuf.Length);
            for (int i = 0; i < wordBuf.Length; i++)
            {
                mapBuf.Append(currentFontState.MapCharacter(wordBuf[i]));
            }

            AddWord(startChar, mapBuf);
        }

        private void AddWord(char startChar, StringBuilder wordBuf)
        {
            string word = (wordBuf != null) ? wordBuf.ToString() : "";
            WordArea hia;
            int startCharWidth = GetCharWidth(startChar);

            if (IsAnySpace(startChar))
            {
                this.AddChild(new InlineSpace(startCharWidth));
            }
            else
            {
                hia = new WordArea(currentFontState, this.red, this.green,
                                   this.blue,
                                   startChar.ToString(), 1);
                hia.SetYOffset(placementOffset);
                this.AddChild(hia);
            }
            int wordWidth = this.GetWordWidth(word);
            hia = new WordArea(currentFontState, this.red, this.green, this.blue,
                               word, word.Length);
            hia.SetYOffset(placementOffset);
            this.AddChild(hia);

            finalWidth += startCharWidth + wordWidth;
        }

        private bool CanBreakMidWord()
        {
            bool ret = false;
            if (hyphProps != null && hyphProps.language != null
                && !hyphProps.language.Equals("NONE"))
            {
                string lang = hyphProps.language.ToLower();
                if ("zh".Equals(lang) || "ja".Equals(lang) || "ko".Equals(lang)
                    || "vi".Equals(lang))
                {
                    ret = true;
                }
            }
            return ret;
        }

        private int GetCharWidth(char c)
        {
            int width;

            if ((c == '\n') || (c == '\r') || (c == '\t') || (c == '\u00A0'))
            {
                width = GetCharWidth(' ');
            }
            else
            {
                width = currentFontState.GetWidth(currentFontState.MapCharacter(c));
                if (width <= 0)
                {
                    int em = currentFontState.GetWidth(currentFontState.MapCharacter('m'));
                    int en = currentFontState.GetWidth(currentFontState.MapCharacter('n'));
                    if (em <= 0)
                    {
                        em = 500 * currentFontState.FontSize;
                    }
                    if (en <= 0)
                    {
                        en = em - 10;
                    }

                    if (c == ' ')
                    {
                        width = em;
                    }
                    if (c == '\u2000')
                    {
                        width = en;
                    }
                    if (c == '\u2001')
                    {
                        width = em;
                    }
                    if (c == '\u2002')
                    {
                        width = em / 2;
                    }
                    if (c == '\u2003')
                    {
                        width = currentFontState.FontSize;
                    }
                    if (c == '\u2004')
                    {
                        width = em / 3;
                    }
                    if (c == '\u2005')
                    {
                        width = em / 4;
                    }
                    if (c == '\u2006')
                    {
                        width = em / 6;
                    }
                    if (c == '\u2007')
                    {
                        width = GetCharWidth(' ');
                    }
                    if (c == '\u2008')
                    {
                        width = GetCharWidth('.');
                    }
                    if (c == '\u2009')
                    {
                        width = em / 5;
                    }
                    if (c == '\u200A')
                    {
                        width = 5;
                    }
                    if (c == '\u200B')
                    {
                        width = 100;
                    }
                    if (c == '\u202F')
                    {
                        width = GetCharWidth(' ') / 2;
                    }
                    if (c == '\u3000')
                    {
                        width = GetCharWidth(' ') * 2;
                    }
                }
            }

            return width;
        }

        private bool IsSpace(char c)
        {
            if (c == ' ' || c == '\u2000' || // en quad
                c == '\u2001' || // em quad
                c == '\u2002' || // en space
                c == '\u2003' || // em space
                c == '\u2004' || // three-per-em space
                c == '\u2005' || // four--per-em space
                c == '\u2006' || // six-per-em space
                c == '\u2007' || // figure space
                c == '\u2008' || // punctuation space
                c == '\u2009' || // thin space
                c == '\u200A' || // hair space
                c == '\u200B') // zero width space
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsNBSP(char c)
        {
            if (c == '\u00A0' || c == '\u202F' || // narrow no-break space
                c == '\u3000' || // ideographic space
                c == '\uFEFF')
            { // zero width no-break space
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsAnySpace(char c)
        {
            bool ret = (IsSpace(c) || IsNBSP(c));
            return ret;
        }

        private void AddSpacedWord(string word, LinkSet ls, int startw,
                                   int spacew, TextState textState,
                                   bool addToPending)
        {
            /*
             * Split string based on four delimeters:
             * \u00A0 - Latin1 NBSP (Non breaking space)
             * \u202F - unknown reserved character according to Unicode Standard
             * \u3000 - CJK IDSP (Ideographic space)
             * \uFEFF - Arabic ZWN BSP (Zero width no break space)
             */
            StringTokenizer st = new StringTokenizer(word, "\u00A0\u202F\u3000\uFEFF", true);
            int extraw = 0;
            while (st.MoveNext())
            {
                string currentWord = (string)st.Current;

                if (currentWord.Length == 1
                    && (IsNBSP(currentWord[0])))
                {
                    // Add an InlineSpace
                    int spaceWidth = GetCharWidth(currentWord[0]);
                    if (spaceWidth > 0)
                    {
                        InlineSpace ispace = new InlineSpace(spaceWidth);
                        extraw += spaceWidth;
                        if (prevUlState)
                        {
                            ispace.SetUnderlined(textState.GetUnderlined());
                        }
                        if (prevOlState)
                        {
                            ispace.SetOverlined(textState.GetOverlined());
                        }
                        if (prevLTState)
                        {
                            ispace.SetLineThrough(textState.GetLineThrough());
                        }

                        if (addToPending)
                        {
                            pendingAreas.Add(ispace);
                            pendingWidth += spaceWidth;
                        }
                        else
                        {
                            AddChild(ispace);
                        }
                    }
                }
                else
                {
                    WordArea ia = new WordArea(currentFontState, this.red,
                                               this.green, this.blue,
                                               currentWord,
                                               GetWordWidth(currentWord));
                    ia.SetYOffset(placementOffset);
                    ia.SetUnderlined(textState.GetUnderlined());
                    prevUlState = textState.GetUnderlined();
                    ia.SetOverlined(textState.GetOverlined());
                    prevOlState = textState.GetOverlined();
                    ia.SetLineThrough(textState.GetLineThrough());
                    prevLTState = textState.GetLineThrough();
                    ia.SetVerticalAlign(vAlign);

                    if (addToPending)
                    {
                        pendingAreas.Add(ia);
                        pendingWidth += GetWordWidth(currentWord);
                    }
                    else
                    {
                        AddChild(ia);
                    }
                    if (ls != null)
                    {
                        Rectangle lr = new Rectangle(startw + extraw, spacew,
                                                     ia.GetContentWidth(),
                                                     fontState.FontSize);
                        ls.AddRect(lr, this, ia);
                    }
                }
            }
        }
    }
}