using AirThermoMod.Common;
using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace AirThermoMod.GUI {
    record class BarValue(double Start, double End);
    internal class GuiDialogBlockEntityAirThermo : GuiDialogBlockEntity {
        List<string> testTexts = new() { "1", "2", "3" };
        ElementBounds dynamicBounds;
        double scrollBarContentFixedY;

        public GuiDialogBlockEntityAirThermo(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi, List<TemperatureSample> samples) : base(dialogTitle, blockEntityPos, capi) {
            if (IsDuplicate) return;

            SetupDialog(samples);
        }

        public void SetupDialog(List<TemperatureSample> samples) {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.Name = "bg";

            var clipBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 10, 480);
            clipBounds.horizontalSizing = ElementSizing.FitToChildren;
            clipBounds.Name = "clip";

            var containerBounds = clipBounds.ForkContainingChild();
            containerBounds.BothSizing = ElementSizing.FitToChildren;
            containerBounds.Name = "container";
            scrollBarContentFixedY = containerBounds.fixedY;

            var scrollBarBounds = clipBounds.CopyOffsetedSibling()
                .WithFixedWidth(20)
                .WithSizing(ElementSizing.Fixed);
            scrollBarBounds.RightOf(clipBounds, 3);

            //var tableBounds = ElementBounds.Fixed(0, 0, 100, 100);
            //tableBounds.BothSizing = ElementSizing.FitToChildren;

            ClearComposers();
            SingleComposer = capi.Gui.CreateCompo("blockentityairthermo" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Air ThermoMeter", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .BeginClip(clipBounds)
                        .AddContainer(containerBounds, "scroll-content")
                    .EndClip()
                    .AddVerticalScrollbar(OnNewScrollbarvalue, scrollBarBounds, "scroll-bar");


            var statsCalc = new TemperatureStats(
                new Common.VSTimeScale { DaysPerMonth = capi.World.Calendar.DaysPerMonth, HoursPerDay = capi.World.Calendar.HoursPerDay }
            );
            var dailyMinAndMax = statsCalc.DailyMinAndMax(samples);
            var allTimeMin = dailyMinAndMax.Min(stat => stat.Min);
            var allTimeMax = dailyMinAndMax.Max(stat => stat.Max);
            var table = dailyMinAndMax
                .Select(stat => new object[] { TimeUtil.VSDateTimeToYearMonthDay(stat.DateTime), $"{stat.Min:F1}", $"{stat.Max:F1}", new BarValue(stat.RateMin, stat.RateMax) })
                .ToArray();

            //object[][] table = new object[][] {
            //    new object[]{"ABC", "BCD", "CAB", new BarValue(0.2, 0.6)},
            //    new object[]{"2", "3", "4", new BarValue(0, 0.3)},
            //    new object[]{"3", "4", "5", new BarValue(0.4,0.6)},
            //};

            var columnWidth = new int[] { 160, 50, 50, 100 };
            var tableTitle = new string[] { "Date", "Min", "Max", "" };

            var container = SingleComposer.GetContainer("scroll-content");

            var tableControl = CreateTable(
                "main",
                table,
                0,
                0,
                columnWidth,
                20,
                tableTitle
            );

            container.Add(tableControl);

            var minAndMaxBounds = ElementBounds.Fixed(0, 0, columnWidth[columnWidth.Length - 1], 20);
            container.Bounds.WithChild(minAndMaxBounds);
            minAndMaxBounds.RightOf(tableControl.Bounds, -columnWidth[columnWidth.Length - 1]);

            var minAndMaxFont = CairoFont.WhiteDetailText();
            var minBound = minAndMaxBounds.ForkContainingChild().WithAlignment(EnumDialogArea.LeftMiddle);
            var maxBound = minAndMaxBounds.ForkContainingChild().WithAlignment(EnumDialogArea.RightMiddle);
            var minText = new GuiElementStaticText(capi, $"{allTimeMin:F1}", minAndMaxFont.Orientation, minBound, minAndMaxFont);
            container.Add(minText);
            var maxText = new GuiElementStaticText(capi, $"{allTimeMax:F1}", minAndMaxFont.Orientation, maxBound, minAndMaxFont);
            container.Add(maxText);
            minText.AutoBoxSize();
            maxText.AutoBoxSize();

            SingleComposer
                .EndChildElements()
                .Compose();

            SingleComposer.GetScrollbar("scroll-bar").SetHeights((float)scrollBarBounds.fixedHeight, (float)containerBounds.OuterHeight);
        }

        CairoFont TableTitleText() {
            return new CairoFont {
                Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
                FontWeight = Cairo.FontWeight.Bold,
                Fontname = GuiStyle.StandardFontName,
                UnscaledFontsize = GuiStyle.SmallFontSize
            };
        }

        GuiElementContainer CreateTable(string name, object[][] tableSource, int x, int y, int[] columnWidth, int rowHeight, string[] columnTitles = null) {
            var containerBounds = ElementBounds.Fixed(0, 0, 1, 1).WithSizing(ElementSizing.FitToChildren);
            containerBounds.Name = $"table-{name}";
            var container = new GuiElementContainer(capi, containerBounds);

            var titleHeight = 24;
            var titleFont = TableTitleText();

            if (columnTitles != null) {
                ElementBounds titleCellBounds = null;
                for (int i = 0; i < columnWidth.Length; ++i) {
                    var previous = titleCellBounds;
                    titleCellBounds = ElementBounds.Fixed(x, y, columnWidth[i], titleHeight);
                    if (i != 0) {
                        titleCellBounds.FixedRightOf(previous);
                    }

                    container.Add(new GuiElementStaticText(capi, columnTitles[i], titleFont.Orientation, titleCellBounds, titleFont));
                }

                // Table content starts under title
                y += titleHeight;
            }

            if (tableSource.Length == 0) {
                return container;
            }

            int columnCount = tableSource[0].Length;

            var cellBounds = new ElementBounds[columnCount];

            // Calculate initial ElementBounds for each column
            for (int i = 0; i < columnWidth.Length; ++i) {
                cellBounds[i] = ElementBounds.Fixed(x, y, columnWidth[i], rowHeight);
                if (i != 0) {
                    cellBounds[i].FixedRightOf(cellBounds[i - 1]);
                    //cellBounds[i] = cellBounds[i - 1].RightCopy();
                }
            }
            var fixedCellFont = CairoFont.WhiteDetailText();

            foreach (var row in tableSource) {
                for (int i = 0; i < columnCount; i++) {
                    if (row[i] is string content) {
                        // TODO: Specify format by args
                        var cellFont = i == 0 ? fixedCellFont.Clone() : fixedCellFont;
                        var text = new GuiElementStaticText(capi, content, cellFont.Orientation, cellBounds[i], cellFont);
                        container.Add(text);
                        // TODO: Specify format by args
                        if (i == 0) text.AutoFontSize();
                    }
                    else if (row[i] is BarValue bv) {
                        container.Add(new GuiElementBar(capi, bv.Start, bv.End, cellBounds[i], GuiStyle.FoodBarColor));
                    }
                    else {
                        throw new Exception($"Unsupported table content");
                    }
                    cellBounds[i] = cellBounds[i].BelowCopy();
                }
            }

            return container;
        }

        new void OnNewScrollbarvalue(float value) {
            var scrollContent = SingleComposer.GetContainer("scroll-content");
            scrollContent.Bounds.fixedY = scrollBarContentFixedY - value;
            scrollContent.Bounds.CalcWorldBounds();
        }


        private void OnTitleBarClose() {
            TryClose();
        }
    }
}
