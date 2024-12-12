using AirThermoMod.BlockEntities;
using AirThermoMod.Blocks;
using AirThermoMod.Config;
using AirThermoMod.Items;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace AirThermoMod {
    // Main mod system class for Thermometer mod
    public class AirThermoModModSystem : ModSystem {
        public AirThermoModConfig Config { get; private set; }

        ICoreServerAPI sapi;

        public static string ModID { get; private set; } = "";

        public override void Start(ICoreAPI api) {
            ModID = Mod.Info.ModID;
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermo", typeof(BlockAirThermo));
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermoUpper", typeof(BlockAirThermoUpper));
            api.RegisterBlockEntityClass(Mod.Info.ModID + ".BEAirThermo", typeof(BEAirThermo));
            api.RegisterItemClass(Mod.Info.ModID + ".ItemTransferNote", typeof(ItemTransferNote));
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
                .Create("airthermo")
                .WithDescription("Commands for Air Thermometer Mod")
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .HandleWith((args) => {
                    return TextCommandResult.Success("See `.chb` for usage", null);
                });

            // Define sub command `force-sample-all` 
            baseCommand.BeginSubCommand("force-sample-all")
                .WithDescription("Make targeted thermometer sample temperature over retention period")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .HandleWith(CmdForceSampleAll);
        }

        // Command handler for subcommand `force-sample-all`
        TextCommandResult CmdForceSampleAll(TextCommandCallingArgs args) {
            if (args.Caller.Player is IServerPlayer splr) {
                var sel = splr.CurrentBlockSelection;
                var bePos = sel.Position;
                var block = sapi.World.BlockAccessor.GetBlock(sel.Position);
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

        public override void StartClientSide(ICoreClientAPI api) {
        }

    }
}