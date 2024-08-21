using System;
using Fonet.DataTypes;

namespace Fonet.Layout
{
    internal class BorderAndPadding : ICloneable
    {
        public const int TOP = 0;
        public const int RIGHT = 1;
        public const int BOTTOM = 2;
        public const int LEFT = 3;

        internal class ResolvedCondLength : ICloneable
        {
            internal int iLength;
            internal bool bDiscard;

            private ResolvedCondLength(int iLength, bool bDiscard)
            {
                this.iLength = iLength;
                this.bDiscard = bDiscard;
            }

            internal ResolvedCondLength(CondLength length)
            {
                bDiscard = length.IsDiscard();
                iLength = length.MValue();
            }

            public object Clone()
            {
                return new ResolvedCondLength(this.iLength, this.bDiscard);
            }

        }

        public object Clone()
        {
            BorderAndPadding bp = new BorderAndPadding
            {
                padding = (ResolvedCondLength[])padding.Clone(),
                borderInfo = (BorderInfo[])borderInfo.Clone()
            };
            for (int i = 0; i < padding.Length; i++)
            {
                if (padding[i] != null)
                {
                    bp.padding[i] = (ResolvedCondLength)padding[i].Clone();
                }
                if (borderInfo[i] != null)
                {
                    bp.borderInfo[i] = (BorderInfo)borderInfo[i].Clone();
                }
            }
            return bp;
        }

        internal class BorderInfo : ICloneable
        {
            internal int mStyle;
            internal ColorType mColor;
            internal ResolvedCondLength mWidth;

            internal BorderInfo(int style, CondLength width, ColorType color)
            {
                mStyle = style;
                mWidth = new ResolvedCondLength(width);
                mColor = color;
            }

            private BorderInfo(int style, ResolvedCondLength width, ColorType color)
            {
                mStyle = style;
                mWidth = width;
                mColor = color;
            }

            public object Clone()
            {
                return new BorderInfo(
                    mStyle, (ResolvedCondLength)mWidth.Clone(), (ColorType)mColor.Clone());
            }
        }

        private BorderInfo[] borderInfo = new BorderInfo[4];
        private ResolvedCondLength[] padding = new ResolvedCondLength[4];

        public BorderAndPadding()
        {
        }

        public void SetBorder(int side, int style, CondLength width,
                              ColorType color)
        {
            borderInfo[side] = new BorderInfo(style, width, color);
        }

        public void SetPadding(int side, CondLength width)
        {
            padding[side] = new ResolvedCondLength(width);
        }

        public void SetPaddingLength(int side, int iLength)
        {
            padding[side].iLength = iLength;
        }

        public void SetBorderLength(int side, int iLength)
        {
            borderInfo[side].mWidth.iLength = iLength;
        }

        public int GetBorderLeftWidth(bool bDiscard)
        {
            return GetBorderWidth(LEFT, bDiscard);
        }

        public int GetBorderRightWidth(bool bDiscard)
        {
            return GetBorderWidth(RIGHT, bDiscard);
        }

        public int GetBorderTopWidth(bool bDiscard)
        {
            return GetBorderWidth(TOP, bDiscard);
        }

        public int GetBorderBottomWidth(bool bDiscard)
        {
            return GetBorderWidth(BOTTOM, bDiscard);
        }

        public int GetPaddingLeft(bool bDiscard)
        {
            return GetPadding(LEFT, bDiscard);
        }

        public int GetPaddingRight(bool bDiscard)
        {
            return GetPadding(RIGHT, bDiscard);
        }

        public int GetPaddingBottom(bool bDiscard)
        {
            return GetPadding(BOTTOM, bDiscard);
        }

        public int GetPaddingTop(bool bDiscard)
        {
            return GetPadding(TOP, bDiscard);
        }


        private int GetBorderWidth(int side, bool bDiscard)
        {
            if ((borderInfo[side] == null)
                || (bDiscard && borderInfo[side].mWidth.bDiscard))
            {
                return 0;
            }
            else
            {
                return borderInfo[side].mWidth.iLength;
            }
        }

        public ColorType GetBorderColor(int side)
        {
            if (borderInfo[side] != null)
            {
                return borderInfo[side].mColor;
            }
            else
            {
                return null;
            }
        }

        public int GetBorderStyle(int side)
        {
            if (borderInfo[side] != null)
            {
                return borderInfo[side].mStyle;
            }
            else
            {
                return 0;
            }
        }

        private int GetPadding(int side, bool bDiscard)
        {
            if ((padding[side] == null) || (bDiscard && padding[side].bDiscard))
            {
                return 0;
            }
            else
            {
                return padding[side].iLength;
            }
        }

    }
}