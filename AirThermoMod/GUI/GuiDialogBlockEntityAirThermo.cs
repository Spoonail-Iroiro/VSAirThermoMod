using AirThermoMod.Common;
using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace AirThermoMod.GUI {
    record class BarValue(double Start, double End);
    internal class GuiDialogBlockEntityAirThermo : GuiDialogBlockEntity {
        List<string> testTexts = new() { "1", "2", "3" };
        ElementBounds dynamicBounds;

        public GuiDialogBlockEntityAirThermo(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi, List<TemperatureSample> samples) : base(dialogTitle, blockEntityPos, capi) {
            if (IsDuplicate) return;

            SetupDialog(samples);
        }

        public void SetupDialog(List<TemperatureSample> samples) {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var bounds1 = ElementBounds.Fixed(0, 30, 100, 8).WithFixedPadding(3, 3);

            var tableBounds = ElementBounds.Fixed(0, 0, 100, 100);
            tableBounds.BothSizing = ElementSizing.FitToChildren;

            ClearComposers();
            SingleComposer = capi.Gui.CreateCompo("blockentityairthermo" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Air ThermoMeter", OnTitleBarClose)
                .BeginChildElements(bgBounds);
            //.AddStaticElement(new GuiElementBar(capi, 0.3, 0.8, bounds1, GuiStyle.FoodBarColor));

            //for (var i = 0; i < 10; i++) {
            //    SingleComposer.AddStaticElement(new GuiElementBar(capi, 0.0, 0.1 * (i + 1), bounds1, GuiStyle.FoodBarColor));
            //    bounds1 = bounds1.BelowCopy();
            //}

            tableBounds.FixedUnder(bounds1);

            SingleComposer.BeginChildElements(tableBounds);
            //.AddStaticText("Hello, GUI!", CairoFont.WhiteDetailText(), bounds1);

            var statsCalc = new TemperatureStats(
                new Common.VSTimeScale { DaysPerMonth = capi.World.Calendar.DaysPerMonth, HoursPerDay = capi.World.Calendar.HoursPerDay }
            );
            var dailyMinAndMax = statsCalc.DailyMinAndMax(samples);
            var table = dailyMinAndMax
                .Select(stat => new object[] { stat.DateTime.PrettyDate(), $"{stat.Min:F1}", $"{stat.Max:F1}", new BarValue(stat.RateMin, stat.RateMax) })
                .ToArray();

            //object[][] table = new object[][] {
            //    new object[]{"ABC", "BCD", "CAB", new BarValue(0.2, 0.6)},
            //    new object[]{"2", "3", "4", new BarValue(0, 0.3)},
            //    new object[]{"3", "4", "5", new BarValue(0.4,0.6)},
            //};

            var columnWidth = new int[] { 200, 40, 40, 100 };
            var tableTitle = new string[] { "Date", "Min", "Max", "Bar" };

            AddTable(
                SingleComposer,
                table,
                0,
                0,
                columnWidth,
                20,
                tableTitle
            );

            SingleComposer
                    .EndChildElements()
                .EndChildElements()
                .Compose();
        }

        CairoFont TableTitleText() {
            return new CairoFont {
                Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
                FontWeight = Cairo.FontWeight.Bold,
                Fontname = GuiStyle.StandardFontName,
                UnscaledFontsize = GuiStyle.SmallFontSize
            };
        }

        void AddTable(GuiComposer composer, object[][] tableSource, int x, int y, int[] columnWidth, int rowHeight, string[] columnTitles = null) {
            var titleHeight = 24;

            if (columnTitles != null) {
                ElementBounds titleCellBounds = null;
                for (int i = 0; i < columnWidth.Length; ++i) {
                    var previous = titleCellBounds;
                    titleCellBounds = ElementBounds.Fixed(x, y, columnWidth[i], titleHeight);
                    if (i != 0) {
                        titleCellBounds.FixedRightOf(previous);
                    }

                    composer.AddStaticText(columnTitles[i], TableTitleText(), titleCellBounds);
                }

                // Table content starts under title
                y += titleHeight;
            }

            if (tableSource.Length == 0) {
                return;
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

            foreach (var row in tableSource) {
                for (int i = 0; i < columnCount; i++) {
                    if (row[i] is string content) {
                        composer.AddStaticText(content, CairoFont.WhiteDetailText(), cellBounds[i]);
                    }
                    else if (row[i] is BarValue bv) {
                        composer.AddStaticElement(new GuiElementBar(capi, bv.Start, bv.End, cellBounds[i], GuiStyle.FoodBarColor));
                    }
                    else {
                        throw new Exception($"Unsupported table content");
                    }
                    cellBounds[i] = cellBounds[i].BelowCopy();
                }
            }
        }

        internal static object[][] DailyMinAndMaxToTable(IEnumerable<(VSDateTime dateTime, double min, double max)> dailyMinAndMax) {
            var maxAllTime = dailyMinAndMax.Max(stat => stat.max);
            var minAllTime = dailyMinAndMax.Min(stat => stat.min);
            var rangeAllTime = maxAllTime - minAllTime;
            if (rangeAllTime < 0.1) {
                rangeAllTime = 0.1;
            }

            var table = dailyMinAndMax
                .Select(stat => new object[] { stat.dateTime.PrettyDate(), $"{stat.min}", $"{stat.max}", new BarValue((stat.min - minAllTime) / rangeAllTime, (stat.max - minAllTime) / rangeAllTime) })
                .ToArray();

            return table;
        }


        private void OnTitleBarClose() {
            TryClose();
        }
    }
}
