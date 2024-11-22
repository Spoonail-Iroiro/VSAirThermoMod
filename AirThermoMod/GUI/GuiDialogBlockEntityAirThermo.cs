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
    internal class GuiDialogBlockEntityAirThermo : GuiDialogBlockEntity {
        List<string> testTexts = new() { "1", "2", "3" };
        ElementBounds dynamicBounds;

        public GuiDialogBlockEntityAirThermo(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, blockEntityPos, capi) {
            if (IsDuplicate) return;

            SetupDialog();
        }

        void SetupDialog() {
            var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var bounds1 = ElementBounds.Fixed(0, 30, 100, 8);


            var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var tableBounds = ElementBounds.Fixed(0, 0, 100, 100);
            tableBounds.FixedUnder(bounds1);

            ClearComposers();
            SingleComposer = capi.Gui.CreateCompo("blockentityairthermo" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Air ThermoMeter", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddStaticElement(new GuiElementBar(capi, 0.3, 0.8, bounds1, GuiStyle.FoodBarColor))
                    .BeginChildElements(tableBounds);
            //.AddStaticText("Hello, GUI!", CairoFont.WhiteDetailText(), bounds1);

            string[][] testTable = new string[][] {
                new[]{"ABC", "BCD", "CAB"},
                new[]{"2", "3", "4"},
                new[]{"3", "4", "5"},
            };

            AddTable(
                SingleComposer,
                tableBounds,
                testTable,
                0,
                0,
                new int[] { 40, 40, 40 },
                20,
                new string[] { "col1", "col2", "col3" }
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

        void AddTable(GuiComposer composer, ElementBounds parentBound, string[][] tableSource, int x, int y, int[] columnWidth, int rowHeight, string[] columnTitles = null) {
            var cellBounds = new ElementBounds[3];
            var titleHeight = 24;

            if (columnTitles != null) {
                ElementBounds titleCellBounds = null;
                for (int i = 0; i < columnWidth.Length; ++i) {
                    if (i == 0) {
                        titleCellBounds = ElementBounds.Fixed(x, y, columnWidth[i], titleHeight);
                    }
                    else {
                        titleCellBounds = titleCellBounds.RightCopy();
                    }

                    parentBound.WithChild(titleCellBounds);
                    composer.AddStaticText(columnTitles[i], TableTitleText(), titleCellBounds);
                }

                // Table content starts under title
                y += titleHeight;
            }

            // Calculate initial ElementBounds for each column
            for (int i = 0; i < columnWidth.Length; ++i) {
                if (i == 0) {
                    cellBounds[i] = ElementBounds.Fixed(x, y, columnWidth[i], rowHeight);
                }
                else {
                    cellBounds[i] = cellBounds[i - 1].RightCopy();
                }
            }

            int columnCount = 0;
            foreach (var row in tableSource) {
                if (columnCount == 0) {
                    columnCount = row.Length;
                    if (columnWidth.Length < columnCount) throw new ArgumentException($"tableSource has {columnCount} columns, but only {columnWidth.Length} columnWidth specified");
                }

                for (int i = 0; i < columnCount; i++) {
                    parentBound.WithChild(cellBounds[i]);
                    composer.AddStaticText(row[i], CairoFont.WhiteDetailText(), cellBounds[i]);
                    cellBounds[i] = cellBounds[i].BelowCopy();
                }
            }
        }

        private void OnTitleBarClose() {
            TryClose();
        }
    }
}
