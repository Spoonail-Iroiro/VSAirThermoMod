using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace AirThermoMod.GUI {
    internal class GuiElementBar : GuiElementTextBase {

        double[] color;

        // Start point of bar value in bar (0.0 to 1.0)
        double barStart = 0;

        // End point of bar value in bar (0.0 to 1.0)
        double barEnd = 0;

        public GuiElementBar(ICoreClientAPI capi, double barStart, double barEnd, ElementBounds bounds, double[] color) : base(capi, "", CairoFont.WhiteDetailText(), bounds) {
            this.color = color;
            this.barStart = barStart;
            this.barEnd = barEnd;
        }

        public override void ComposeElements(Context ctx, ImageSurface surface) {
            Bounds.CalcWorldBounds();

            ctx.Operator = Operator.Over;
            GuiElement.RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1.0);
            ctx.SetSourceRGBA(0.15, 0.15, 0.15, 1.0);
            ctx.Fill();
            EmbossRoundRectangleElement(ctx, Bounds, inverse: false, 3, 1);
            ComposeValue(ctx, surface);
        }

        void ComposeValue(Context ctx, ImageSurface surface) {
            Bounds.CalcWorldBounds();

            double barValueLengthOuter = Bounds.InnerWidth * (barEnd - barStart);
            double barValueXOuter = Bounds.drawX + Bounds.InnerWidth * barStart;

            GuiElement.RoundRectangle(ctx, barValueXOuter, Bounds.drawY, barValueLengthOuter, Bounds.InnerHeight, 1.0);
            ctx.SetSourceRGB(color[0], color[1], color[2]);
            ctx.FillPreserve();
            ctx.SetSourceRGB(color[0] * 0.4, color[1] * 0.4, color[2] * 0.4);
            ctx.LineWidth = GuiElement.scaled(3.0);

            double barValueLengthInner = Bounds.InnerWidth * (barEnd - barStart);
            double barValueXInner = Bounds.drawX + Bounds.InnerWidth * barStart;
            EmbossRoundRectangleElement(ctx, barValueXInner, Bounds.drawY, barValueLengthInner, Bounds.InnerHeight, inverse: false, 2, 1);
        }

        public override void Dispose() {
            base.Dispose();
        }
    }
}
