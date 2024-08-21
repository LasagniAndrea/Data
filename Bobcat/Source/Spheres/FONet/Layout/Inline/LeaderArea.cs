using Fonet.Fo.Properties;
using Fonet.Render.Pdf;

namespace Fonet.Layout.Inline
{
    internal class LeaderArea : InlineArea
    {
        private readonly int ruleThickness;
        private readonly int leaderPattern;
        private readonly int ruleStyle;

        public LeaderArea(
            FontState fontState, float red, float green,
            float blue, int leaderLengthOptimum,
            int leaderPattern, int ruleThickness, int ruleStyle)
            : base(fontState, leaderLengthOptimum, red, green, blue)
        {
            this.leaderPattern = leaderPattern;
            this.ruleStyle = ruleStyle;
            if (ruleStyle == RuleStyle.NONE)
            {
                ruleThickness = 0;
            }
            this.ruleThickness = ruleThickness;
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderLeaderArea(this);
        }

        public int GetRuleThickness()
        {
            return this.ruleThickness;
        }

        public int GetRuleStyle()
        {
            return this.ruleStyle;
        }

        public int GetLeaderPattern()
        {
            return this.leaderPattern;
        }

        public int GetLeaderLength()
        {
            return this.contentRectangleWidth;
        }

    }
}