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
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

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
                .RequiresPlayer()
                .HandleWith((args) => {
                    return TextCommandResult.Success("See `.chb` for usage", null);
                });

            // Define sub command `force-sample-all` 
            baseCommand.BeginSubCommand("force-sample-all")
                .WithDescription("Forces the targeted thermometer to get temperature data over the entire retention period (one in-game year)")
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
                .WithDescription("Pass 'c' or 'f' to specify the temperature unit. Pass 'u' to reset to the default, which automatically switches temperature unit based on other Fahrenheit unit mod (e.g. FreedomUnits)")
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

                    resultSB.AppendLine($"Current unit setting: {ClientConfig.unitSetting}");
                    if (ClientConfig.unitSetting == TemperatureUnitSetting.Unspecified) {
                        if (IsTemperatureUnitFahrenheit) {
                            resultSB.AppendLine("Will use Fahrenheit since a Fahrenheit unit mod (e.g. FreedomUnits) is enabled");
                        }
                        else {
                            resultSB.AppendLine("Will use Celsius since no Fahrenheit unit mod (e.g. FreedomUnits) is active");
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
                    return TextCommandResult.Error("Error: No thermometers targeted");
                }
                else {
                    beAirThermo.ScheduleForceSampleOverRetentionPeriod();
                    return TextCommandResult.Success("Scheduled sampling temperature over retention period (might take some seconds)");
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