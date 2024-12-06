using AirThermoMod.BlockEntities;
using AirThermoMod.Blocks;
using AirThermoMod.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace AirThermoMod {
    public class AirThermoModModSystem : ModSystem {
        public AirThermoModConfig Config { get; private set; }
        ICoreClientAPI capi;
        ICoreServerAPI sapi;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api) {
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermo", typeof(BlockAirThermo));
            api.RegisterBlockClass(Mod.Info.ModID + ".BlockAirThermoUpper", typeof(BlockAirThermoUpper));
            api.RegisterBlockEntityClass(Mod.Info.ModID + ".BEAirThermo", typeof(BEAirThermo));
        }

        public override void StartServerSide(ICoreServerAPI api) {
            sapi = api;
            var configName = Mod.Info.ModID + ".json";
            Config = api.LoadModConfig<AirThermoModConfig>(configName);
            if (Config == null) {
                Config = new AirThermoModConfig();
                api.StoreModConfig(Config, configName);
            }

            var baseCommand = api.ChatCommands
                .Create("airthermo")
                .WithDescription("Commands for Air Thermometer Mod")
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .HandleWith((args) => {
                    if (args.Caller.Player is IServerPlayer splr) {
                        splr.SendMessage(GlobalConstants.CurrentChatGroup, "See `/help airthermo` for usage", EnumChatType.Notification);
                    }
                    return TextCommandResult.Success("", null);
                });

            baseCommand.BeginSubCommand("force-sample-all")
                .WithDescription("Make target air thermometer sample temperature over retention period")
                .RequiresPrivilege(Privilege.controlserver)
                .RequiresPlayer()
                .HandleWith(CmdForceSampleAll);
        }

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
                    return TextCommandResult.Error("Error: No air thermometers targeted");
                }
                else {
                    beAirThermo.ScheduleForceSampleOverRetentionPeriod();
                    splr.SendMessage(GlobalConstants.CurrentChatGroup, "Scheduled sampling temperature over retention period (might take some seconds)", EnumChatType.Notification);
                }
            }
            return TextCommandResult.Success("");
        }

        public override void StartClientSide(ICoreClientAPI api) {
            capi = api;
        }

    }
}