using AirThermoMod.Common;
using AirThermoMod.Core;
using MNGui.DialogBuilders;
using MNGui.GuiElements;
using MNGui.Layouts;
using MNGui.Layouts.Extensions;
using MNGui.Std;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace AirThermoMod.GUI {
    /// <summary>
    /// Record for holding the start and end values for temperature bar 
    /// </summary>
    /// <param name="Start"></param>
    /// <param name="End"></param>
    record class BarValue(double Start, double End);

    /// <summary>
    /// GUI dialog class opened on interacting with thermometer block
    /// </summary>
    internal class GuiDialogBlockEntityAirThermo : GuiDialogBlockEntity {
        public ContainerDialogController? DialogController { get; private set; }

        AirThermoModModSystem mod;

        const int tableHorizontalGap = 10;

        /// <summary>
        /// Event triggered when 'Reverse Order' button is clicked
        /// </summary>
        public Func<bool>? ReverseOrderButtonClicked { get; set; }

        public GuiDialogBlockEntityAirThermo(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi, List<TemperatureSample> samples, string order) : base(dialogTitle, blockEntityPos, capi) {
            mod = capi.ModLoader.GetModSystem<AirThermoModModSystem>();
            if (IsDuplicate) return;

            SetupDialog(samples, order);
        }

        public LayoutWithElementBounds BuildTemperatureTableBody(
                object[][] tableSource,
                int[] columnWidthes,
                int rowHeight
            ) {
            if (tableSource.Length == 0) {
                return new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)));
            }

            var columnCount = columnWidthes.Length;

            // To achieve center-aligned <right-aligned digit column>, here's a bit tricky approach
            // Create a vertical layout (innerColumnLayouts) containing all cells in a column, then
            // put it into another VerticalLayout (contentColumnLayout)
            // So, usually table is rows of cells, but this table is columns of cells
            var innerColumnLayouts = Enumerable.Range(0, columnCount)
                .Select(i => new VerticalLayout(capi)
                    // Horizontal is MinSize, to make the width max width of all cell in the column
                    .WithHorizontalSizePolicy(SizePolicy.MinSize)
                    .WithAlignment(
                        i switch {
                            1 => AlignmentHorizontal.Right,
                            2 => AlignmentHorizontal.Right,
                            _ => AlignmentHorizontal.Left
                        }
                    )
                )
                .ToList();

            // Creates each column
            foreach (var row in tableSource) {
                for (var index = 0; index < row.Length; ++index) {
                    var target = row[index];
                    var targetInnerColumnLayout = innerColumnLayouts[index];
                    var columnWidth = columnWidthes[index];

                    if (target is string text) {
                        var element = new MNGuiElementStaticText(
                                capi,
                                text,
                                ElementBounds.FixedSize(columnWidth, rowHeight),
                                font: CairoFont.WhiteDetailText(),
                                autoWrap: false
                            );
                        if (index == 0) {
                            targetInnerColumnLayout.Add(element);
                        }
                        else {
                            element.WithAutoBoxSize();
                            // Needs "cell" layout to align vertical (very slight difference though)
                            var cell = new HorizontalLayout(capi)
                                    .WithAlignment(vAlign: AlignmentVertical.Middle)
                                    .WithHorizontalSizePolicy(SizePolicy.MinSize)
                                    .WithFixedSize(height: rowHeight)
                                    .Add(element);
                            targetInnerColumnLayout.Add(cell);
                        }
                    }
                    else if (target is BarValue bv) {
                        var element = new GuiElementBar(capi, bv.Start, bv.End, ElementBounds.FixedSize(columnWidth, rowHeight), GuiStyle.FoodBarColor);
                        targetInnerColumnLayout.Add(element);
                    }
                    else {
                        throw new Exception($"Unsupported table content");
                    }
                }
            }

            var bodyLayout = new HorizontalLayout(capi, tableHorizontalGap);

            // Put the column into outer VerticalLayout, and add it to the main HorizontalLayout
            foreach (var (innerColumnLayout, columnWidth) in Enumerable.Zip(innerColumnLayouts, columnWidthes)) {
                var contentColumnLayout = new VerticalLayout(capi)
                    .WithAlignment(hAlign: AlignmentHorizontal.Center)
                    .WithFixedSize(width: columnWidth)
                    .Add(
                        innerColumnLayout.ChildLayouts.Count > 0 ?
                            innerColumnLayout :
                            new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)))
                    );

                bodyLayout.Add(contentColumnLayout);
            }

            return bodyLayout;
        }

        public LayoutWithElementBounds BuildTemperatureTable(
                object[][] tableSource,
                int[] columnWidthes,
                int rowHeight,
                string[] columnTitles,
                string allTimeMinStr,
                string allTimeMaxStr
            ) {
            var elementStd = new ElementStd(capi);
            var tableTitleFont = CairoFont.WhiteSmallText()
                .WithWeight(Cairo.FontWeight.Bold)
                .WithOrientation(EnumTextOrientation.Center);

            var titleRow = new HorizontalLayout(capi, tableHorizontalGap);

            var titleRowHeight = 24;
            for (var i = 0; i < columnWidthes.Length; i++) {
                var columnTitle = columnTitles[i];
                var columnWidth = columnWidthes[i];
                if (i < columnWidthes.Length - 1) {
                    // Not last column
                    var element = new MNGuiElementStaticText(
                            capi,
                            columnTitle,
                            ElementBounds.FixedSize(columnWidth, titleRowHeight),
                            font: tableTitleFont,
                            orientation: tableTitleFont.Orientation
                        );
                    titleRow.Add(element);
                }
                else {
                    // Last column - shows min/max temp over retention period
                    var layout = new HorizontalLayout(capi)
                        .WithFixedSize(columnWidth, titleRowHeight)
                        .WithAlignment(vAlign: AlignmentVertical.Bottom)
                        .Add(elementStd.TextAutoBoxSize(allTimeMinStr))
                        .Add(
                            new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)))
                                .WithHorizontalSizePolicy(SizePolicy.Stretch)
                        )
                        .Add(elementStd.TextAutoBoxSize(allTimeMaxStr));
                    titleRow.Add(layout);
                }
            }

            var bodyLayout = BuildTemperatureTableBody(tableSource, columnWidthes, rowHeight);

            var tableLayout = new VerticalLayout(capi)
                .Add(
                    titleRow
                )
                .Add(
                    bodyLayout
                );

            return tableLayout;
        }

        public LayoutWithElementBounds BuildTemperatureTableFromSamples(List<TemperatureSample> samples, string order) {
            // Initialize temperature stats calculator with world time scale
            var statsCalc = new TemperatureStats(
                new Common.VSTimeScale { DaysPerMonth = capi.World.Calendar.DaysPerMonth, HoursPerDay = capi.World.Calendar.HoursPerDay }
            );
            // Calculate daily min and max temperature
            var dailyMinAndMax = statsCalc.DailyMinAndMax(samples, order);
            // Determine overall min and max temperatures to show above the temperature bars
            double allTimeMin = dailyMinAndMax.Select(stat => (double?)stat.Min).Min() ?? 0;
            double allTimeMax = dailyMinAndMax.Select(stat => (double?)stat.Max).Max() ?? 1;
            // Prepare data for the table
            var table = dailyMinAndMax
                .Select(stat => new object[] { TimeUtil.VSDateTimeToYearMonthDay(stat.DateTime), mod.FormatTemperature(stat.Min), mod.FormatTemperature(stat.Max), new BarValue(stat.RateMin, stat.RateMax) })
                .ToArray();

            var unit = mod.GetTemperatureUnitString();

            // Some style settings
            var columnWidth = new int[] { 150, 90, 90, 120 };
            var tableTitle = new string[] { "Date", $"Min [{unit}]", $"Max [{unit}]", "" };

            var container = SingleComposer.GetContainer("scroll-content");

            // Get table as container element
            var tableLayout = BuildTemperatureTable(
                    table,
                    columnWidth,
                    20,
                    tableTitle,
                    mod.FormatTemperature(allTimeMin),
                    mod.FormatTemperature(allTimeMax)
                );

            return tableLayout;
        }

        public LayoutWithElementBounds SetupLayout() {
            var layoutBuilder = new InsetContainerLayoutBuilder(capi, "container-table")
                .WithSizeFitToChildren(BoxSide.Horizontal)
                .WithSizeFixed(BoxSide.Vertical, 400)
                .WithInset(false);

            var mainLayout = new VerticalLayout(capi, 10)
                .Add(
                    // Control area
                    new HorizontalLayout(capi, hAlign: AlignmentHorizontal.Center)
                        .Add(
                            new MNGuiElementTextButton(capi, "Reverse order", ElementBounds.FixedSize(120, 25), font: CairoFont.WhiteSmallText()),
                            "button-reverseorder"
                        )
                )
                .Add(
                    layoutBuilder.Build()
                );

            return mainLayout;
        }

        public void SetupDialog(List<TemperatureSample> samples, string order) {
            var dialogBuilder = new ContainerDialogBuilder();
            var mainLayout = SetupLayout();
            dialogBuilder.SetChildLayout(mainLayout);

            ClearComposers();
            SingleComposer = dialogBuilder.Layout(capi, this);

            DialogController = new ContainerDialogController(capi, SingleComposer, mainLayout);

            var tableContainer = DialogController.GetElement<MNGuiElementInnerLayoutContainer>("container-table");
            if (tableContainer == null) throw new InvalidOperationException("Couldn't find container-table!");

            var tableLayout = BuildTemperatureTableFromSamples(samples, order);

            tableContainer.ApplyNewLayoutImmediately(tableLayout);

            var reverseOrderButton = DialogController.GetElement<MNGuiElementTextButton>("button-reverseorder");
            if (reverseOrderButton == null) throw new InvalidOperationException("Couldn't find button-reverse!");
            reverseOrderButton.EventClicked = OnReverseOrderButtonClicked;
        }

        public void UpdateTable(List<TemperatureSample> samples, string order) {
            if (!IsOpened()) return;
            var tableContainer = DialogController?.GetElement<MNGuiElementInnerLayoutContainer>("container-table");
            if (tableContainer == null) throw new InvalidOperationException("Couldn't find container-table!");

            var tableLayout = BuildTemperatureTableFromSamples(samples, order);

            tableContainer.SetNewLayout(tableLayout);
        }

        bool OnReverseOrderButtonClicked() {
            return ReverseOrderButtonClicked?.Invoke() ?? true;
        }
    }
}
