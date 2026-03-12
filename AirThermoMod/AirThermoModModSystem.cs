using AirThermoMod.BlockEntities;
using AirThermoMod.Blocks;
using AirThermoMod.Common;
using AirThermoMod.Config;
using AirThermoMod.Items;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace AirThermoMod {
    // Main mod system class for Thermometer mod
    public class AirThermoModModSystem : ModSystem {

        ICoreServerAPI? sapi;
        ICoreClientAPI? capi;

        bool isFahrenheitModEnabled = false;

        public AirThermoModConfig? Config { get; private set; }

        public AirThermoModClientConfig? ClientConfig { get; private set; }

        public bool IsTemperatureUnitFahrenheit {
            get {
                var isUnspecified = ClientConfig == null || ClientConfig.unitSetting == TemperatureUnitSetting.Unspecified;
                return (isUnspecified && isFahrenheitModEnabled) || ClientConfig?.unitSetting == TemperatureUnitSetting.Fahrenheit;
            }
        }

        static string ROOT_COMMAND_NAME = "thermo";

        public static string ModID { get; private set; } = "";

        public override void Start(ICoreAPI api) {
            ModID = Mod.Info.ModID;
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermo", typeof(BlockAirThermo));
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermoUpper", typeof(BlockAirThermoUpper));
            api.RegisterBlockEntityClass(Mod.Info.ModID + ".BEAirThermo", typeof(BEAirThermo));
            api.RegisterItemClass(Mod.Info.ModID + ".ItemRecordedChartPaper", typeof(ItemRecordedChartPaper));
        }

        public override void StartServerSide(ICoreServerAPI api) {
            sapi = api;

            // Load or initialize the config file
            var configName = Mod.Info.ModID + ".json";
            Config = api.LoadModConfig<AirThermoModConfig>(configName);
            if (Config == null) {
                Config = new AirThermoModConfig();
                api.StoreModConfig(Config, configName);
            }

            // Define main console command 
            var baseCommand = api.ChatCommands
                .Create(ROOT_COMMAND_NAME)
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer();

            // Define sub command `force-sample-all` 
            baseCommand.BeginSubCommand("force-sample-all")
                .WithDescription(Lang.Get(TrUtil.LK("commanddesc-force-sample-all")))
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .HandleWith(CmdForceSampleAll);
        }

        public override void StartClientSide(ICoreClientAPI api) {
            capi = api;

            var parsers = api.ChatCommands.Parsers;

            var configName = Mod.Info.ModID + "-client.json";
            ClientConfig = api.LoadModConfig<AirThermoModClientConfig>(configName);
            if (ClientConfig == null) {
                ClientConfig = new AirThermoModClientConfig();
                api.StoreModConfig(ClientConfig, configName);
            }

            if (capi.ModLoader.IsModEnabled("freedomunits")) {
                isFahrenheitModEnabled = true;
            }

            var baseCommand = api.ChatCommands
                .Create(ROOT_COMMAND_NAME)
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer();

            baseCommand.BeginSubCommand("unit")
                .WithDescription(Lang.Get(TrUtil.LK("commanddesc-unit")))
                .WithArgs(parsers.OptionalWordRange("unit", ["u", "unspecified", "c", "celsius", "f", "fahrenheit"]))
                .HandleWith(args => {
                    if (ClientConfig == null) {
                        return TextCommandResult.Error($"Can't change client config");
                    }
                    var resultSB = new StringBuilder();

                    var specifiedUnit = (string)args.Parsers[0].GetValue();

                    if (specifiedUnit != null) {
                        if (AirThermoModClientConfig.TryParseTemperatureUnitSetting(specifiedUnit, out TemperatureUnitSetting unitSetting)) {
                            ClientConfig.unitSetting = unitSetting;
                            capi.StoreModConfig(ClientConfig, configName);
                        }
                        else {
                            return TextCommandResult.Error($"Unrecognized argument");
                        }
                    }

                    resultSB.AppendLine(Lang.Get(TrUtil.LK("commandresult-currentunitsetting"), ClientConfig.unitSetting.Tr()));
                    if (ClientConfig.unitSetting == TemperatureUnitSetting.Unspecified) {
                        if (IsTemperatureUnitFahrenheit) {
                            resultSB.AppendLine(Lang.Get(TrUtil.LK("commandresult-willuse-fahrenheit")));
                        }
                        else {
                            resultSB.AppendLine(Lang.Get(TrUtil.LK("commandresult-willuse-celcsius")));
                        }
                    }

                    return TextCommandResult.Success(resultSB.ToString());

                });
        }


        // Command handler for subcommand `force-sample-all`
        TextCommandResult CmdForceSampleAll(TextCommandCallingArgs args) {
            if (args.Caller.Player is IServerPlayer splr) {
                var sel = splr.CurrentBlockSelection;
                var bePos = sel.Position;
                var block = sapi!.World.BlockAccessor.GetBlock(sel.Position);
                if (block is BlockAirThermoUpper) {
                    bePos = sel.Position.DownCopy();
                }
                var beAirThermo = sapi.World.BlockAccessor.GetBlockEntity(bePos) as BEAirThermo;
                if (beAirThermo == null) {
                    return TextCommandResult.Error(Lang.Get(TrUtil.LK("commandresult-notarget")));
                }
                else {
                    beAirThermo.ScheduleForceSampleOverRetentionPeriod();
                    return TextCommandResult.Success(Lang.Get(TrUtil.LK("commandresult-scheduledsampling")));
                }
            }
            return TextCommandResult.Success("");
        }

        public string FormatTemperature(double temperature) {
            var displayTemperature = $"{temperature:F1}";

            if (IsTemperatureUnitFahrenheit) {
                displayTemperature = $"{TemperatureUtil.ToFahrenheight(temperature):F1}";
            }

            return displayTemperature;
        }

        public string GetTemperatureUnitString() {
            var unit = "°C";

            if (IsTemperatureUnitFahrenheit) {
                unit = "°F";
            }

            return unit;
        }

    }
}