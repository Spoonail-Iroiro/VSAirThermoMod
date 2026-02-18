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
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using Vintagestory.Server;
using MNGui.GuiElements;
using MNGui.Layouts;
using MNGui.Layouts.Extensions;
using MNGui.Std;
using MNGui.DialogBuilders;
using Vintagestory.ServerMods;
using MNGui.Extensions;

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
        double scrollBarContentFixedY;

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

        public LayoutWithElementBounds CreateTableBody(
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
                                CairoFont.WhiteDetailText(),
                                EnumTextOrientation.Left
                            );
                        if (index == 0) {
                            element.WithAutoFontSize();
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
                            //new WrapperElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(10, rowHeight).WithHorizontalSizing(ElementSizing.FitToChildren)))
                            //.Add(
                            //);
                            //element.Bounds.WithFixedHeight(rowHeight);
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

            foreach (var (innerColumnLayout, columnWidth) in Enumerable.Zip(innerColumnLayouts, columnWidthes)) {
                // Put the column into Outer VerticalLayout, and add it to the main HorizontalLayout
                var contentColumnLayout = new VerticalLayout(capi)
                    .WithAlignment(hAlign: AlignmentHorizontal.Center)
                    .WithFixedSize(width: columnWidth)
                    //.Add(
                    //    new GuiElementDummy(capi, ElementBounds.FixedSize(columnWidth, 1))
                    //)
                    .Add(
                        innerColumnLayout.ChildLayouts.Count > 0 ?
                            innerColumnLayout :
                            new ElementLayout(new GuiElementDummy(capi, ElementBounds.FixedSize(1, 1)))
                    );

                bodyLayout.Add(contentColumnLayout);
            }

            return bodyLayout;
        }

        public LayoutWithElementBounds CreateTableLayout(
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
                    //var element = new GuiElementStaticText(capi, columnTitle, tableTitleFont.Orientation, ElementBounds.FixedSize(colWidth, 24), font: tableTitleFont);
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

            var bodyLayout = CreateTableBody(tableSource, columnWidthes, rowHeight);

            var tableLayout = new VerticalLayout(capi)
                .Add(
                    titleRow
                )
                .Add(
                    bodyLayout
                );

            return tableLayout;
        }

        public LayoutWithElementBounds GetTableLayout(List<TemperatureSample> samples, string order) {
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
            var tableLayout = CreateTableLayout(
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
            //.WithInitialLayout(GetTableLayout(samples, order));

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

            var tableLayout = GetTableLayout(samples, order);

            tableContainer.ApplyNewLayoutImmediately(tableLayout);

            var reverseOrderButton = DialogController.GetElement<MNGuiElementTextButton>("button-reverseorder");
            if (reverseOrderButton == null) throw new InvalidOperationException("Couldn't find button-reverse!");
            reverseOrderButton.EventClicked = OnReverseOrderButtonClicked;
        }

        public void UpdateTable(List<TemperatureSample> samples, string order) {
            if (!IsOpened()) return;
            var tableContainer = DialogController?.GetElement<MNGuiElementInnerLayoutContainer>("container-table");
            if (tableContainer == null) throw new InvalidOperationException("Couldn't find container-table!");

            var tableLayout = GetTableLayout(samples, order);

            tableContainer.SetNewLayout(tableLayout);
        }

        public void _SetupDialog(List<TemperatureSample> samples, string order) {
            var mod = capi.ModLoader.GetModSystem<AirThermoModModSystem>();

            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.Name = "bg";

            // Bounds for the control area (now contains only "Reverse Order" button).
            var controlAreaBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, 10, 30).WithSizing(ElementSizing.FitToChildren, ElementSizing.FitToChildren);
            bgBounds.WithChildren(controlAreaBounds);

            var reverseOrderbuttonBounds = ElementBounds.Fixed(480 / 2 - 60, 0, 120, 25);
            controlAreaBounds.WithChildren(reverseOrderbuttonBounds);
            var reverseOrderButton = new GuiElementTextButton(capi, "Reverse order", CairoFont.WhiteSmallText(), CairoFont.WhiteSmallText(), OnReverseOrderButtonClicked, reverseOrderbuttonBounds);

            // To use scrollbar, bounds for clipping is required
            var clipBounds = ElementBounds.Fixed(0, 0, 10, 400);
            clipBounds.FixedUnder(controlAreaBounds, 10);
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

            ClearComposers();
            SingleComposer = capi.Gui.CreateCompo("blockentityairthermo" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddInteractiveElement(reverseOrderButton)
                    .BeginClip(clipBounds)
                        .AddContainer(containerBounds, "scroll-content")
                    .EndClip()
                    .AddVerticalScrollbar(OnNewScrollbarvalue, scrollBarBounds, "scroll-bar");

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

            // Min and max temperature over all time (above temperature bars)
            //   Bounds
            var minAndMaxBounds = ElementBounds.Fixed(0, 0, columnWidth[columnWidth.Length - 1], 24);
            container.Bounds.WithChild(minAndMaxBounds);
            minAndMaxBounds.RightOf(tableControl.Bounds, -columnWidth[columnWidth.Length - 1]);
            //   Content
            var minAndMaxFont = CairoFont.WhiteDetailText();
            var minBound = minAndMaxBounds.ForkContainingChild().WithAlignment(EnumDialogArea.LeftBottom);
            var maxBound = minAndMaxBounds.ForkContainingChild().WithAlignment(EnumDialogArea.RightBottom);
            var minText = new GuiElementStaticText(capi, mod.FormatTemperature(allTimeMin), minAndMaxFont.Orientation, minBound, minAndMaxFont);
            container.Add(minText);
            var maxText = new GuiElementStaticText(capi, mod.FormatTemperature(allTimeMax), minAndMaxFont.Orientation, maxBound, minAndMaxFont);
            container.Add(maxText);
            minText.AutoBoxSize();
            maxText.AutoBoxSize();

            SingleComposer
                .EndChildElements()
                .Compose();

            // Scroll bar setting
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

        /// <summary>
        /// Create table as container element
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tableSource"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="columnWidth"></param>
        /// <param name="rowHeight"></param>
        /// <param name="columnTitles"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        GuiElementContainer CreateTable(string name, object[][] tableSource, int x, int y, int[] columnWidth, int rowHeight, string[]? columnTitles = null) {
            var containerBounds = ElementBounds.Fixed(0, 0, 1, 1).WithSizing(ElementSizing.FitToChildren);
            containerBounds.Name = $"table-{name}";
            var container = new GuiElementContainer(capi, containerBounds);
            var fixedMargin = 10;

            var titleHeight = 24;
            var titleFont = TableTitleText();

            if (columnTitles != null) {
                ElementBounds? titleCellBounds = null;
                for (int i = 0; i < columnWidth.Length; ++i) {
                    var previous = titleCellBounds;
                    titleCellBounds = ElementBounds.Fixed(x, y, columnWidth[i], titleHeight);
                    if (i != 0) {
                        titleCellBounds.FixedRightOf(previous, fixedMargin);
                    }

                    container.Bounds.WithChildren(titleCellBounds);
                    var titleTextBounds = titleCellBounds.ForkContainingChild().WithAlignment(EnumDialogArea.CenterTop);
                    var titleText = new GuiElementStaticText(capi, columnTitles[i], titleFont.Orientation, titleTextBounds, titleFont);
                    titleText.AutoBoxSize();
                    container.Add(titleText);
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
                    cellBounds[i].FixedRightOf(cellBounds[i - 1], fixedMargin);
                    //cellBounds[i] = cellBounds[i - 1].RightCopy();
                }
            }
            var fixedCellFont = CairoFont.WhiteDetailText();

            var columnFonts = Enumerable.Range(0, columnCount).Select(_ => fixedCellFont.Clone()).ToList();
            columnFonts[1].WithOrientation(EnumTextOrientation.Right);
            columnFonts[2].WithOrientation(EnumTextOrientation.Right);

            foreach (var row in tableSource) {
                for (int i = 0; i < columnCount; i++) {
                    if (row[i] is string content) {
                        // TODO: Specify format by args
                        var cellFont = columnFonts[i];

                        container.Bounds.WithChild(cellBounds[i]);
                        var textBounds = cellBounds[i].ForkContainingChild();
                        var text = new GuiElementStaticText(capi, content, cellFont.Orientation, textBounds, cellFont);
                        if (i == 1 || i == 2) {
                            textBounds.Alignment = EnumDialogArea.RightMiddle;
                            textBounds.WithFixedAlignmentOffset(-30, 0);
                            text.AutoBoxSize();
                        }
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

        bool OnReverseOrderButtonClicked() {
            if (ReverseOrderButtonClicked == null) {
                return true;
            }

            return ReverseOrderButtonClicked();
        }
        private void OnTitleBarClose() {
            TryClose();
        }
    }
}
