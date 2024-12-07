using AirThermoMod.Common;
using AirThermoMod.Core;
using AirThermoMod.GUI;
using AirThermoMod.VS;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace AirThermoMod.BlockEntities {
    public enum AirThermoPacketId {
        GuiSettingChanged = 891001,
    }

    [ProtoContract()]
    record class BEAirThermoGuiSetting(
        [property: ProtoMember(1)] string TableSortOrder
    ) {
        public BEAirThermoGuiSetting() : this("desc") { }
    }

    internal class BEAirThermo : BlockEntity {
        protected int intervalMinute = 60;

        protected double retentionPeriodYear = 1.0;

        protected static Random rand = new();

        protected double totalHoursLastUpdate;

        protected double totalHoursNextUpdate;

        protected TemperatureRecorder temperatureRecorder = new();

        GuiDialogBlockEntityAirThermo clientDialog;

        BEAirThermoGuiSetting guiSetting = new BEAirThermoGuiSetting();

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);

            if (api is ICoreServerAPI sapi) {
                RegisterGameTickListener(Update, 3300 + rand.Next(500));

                var modSystem = sapi.ModLoader.GetModSystem<AirThermoModModSystem>();
                if (modSystem != null) {
                    intervalMinute = modSystem.Config.samplingIntervalMinutes;
                }
            }
        }

        // Update lastUpdateTotalHours and calculate nextUpdateRoundedTotalMinutes
        protected void UpdateTimes(double totalHoursLastUpdate) {
            if (Api.Side == EnumAppSide.Server) {
                this.totalHoursLastUpdate = totalHoursLastUpdate;

                var timeMinutes = NextUpdateRoundedTotalMinutes(Api.World.Calendar.HoursPerDay, totalHoursLastUpdate, intervalMinute);

                totalHoursNextUpdate = TimeUtil.RoundedTotalMinutesToTotalHours(timeMinutes);

                MarkDirty();
            }
        }

        protected void toggleGuiClient() {
            if (Api is not ICoreClientAPI capi) return;

            if (clientDialog == null) {
                clientDialog = new GuiDialogBlockEntityAirThermo("Air Thermometer", Pos, capi, temperatureRecorder.TemperatureSamples, guiSetting.TableSortOrder);
                clientDialog.ReverseOrderButtonClicked = OnReverseOrderButtonClicked;
            }

            if (clientDialog.IsOpened()) {
                clientDialog.TryClose();
            }
            else {
                clientDialog.SetupDialog(temperatureRecorder.TemperatureSamples, guiSetting.TableSortOrder);
                clientDialog.TryOpen();
            }
        }

        public bool Interact(IWorldAccessor world, IPlayer byPlayer) {
            var calendar = Api.World.Calendar;

            toggleGuiClient();
            return true;
        }

        private ClimateCondition GetClimateConditionAtSpecificTime(BlockPos pos, double totalHours, ClimateCondition previousCond = null) {
            if (previousCond == null) {
                return Api.World.BlockAccessor.GetClimateAt(
                    Pos,
                    EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly,
                    totalHours / Api.World.Calendar.HoursPerDay
                );
            }
            else {
                return Api.World.BlockAccessor.GetClimateAt(
                    Pos,
                    previousCond,
                    EnumGetClimateMode.ForSuppliedDate_TemperatureRainfallOnly,
                    totalHours / Api.World.Calendar.HoursPerDay
                );
            }
        }

        protected void Update(float dt) {
            UpdateFromLastTime();
        }

        protected void UpdateFromLastTime() {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

            var currentTotalHours = Api.World.Calendar.TotalHours;

            // Allowed oldest sample and skip sampling older than this point
            var minTotalHours = currentTotalHours - TimeUtil.TotalYearsToTotalHours(Api.World.Calendar, retentionPeriodYear);

            // Skip sampling before minTotalHours
            if (totalHoursNextUpdate < minTotalHours) {
                UpdateTimes(minTotalHours);
            }

            ClimateCondition cond = null;
            bool hasUpdate = false;

            while (!(currentTotalHours < totalHoursNextUpdate)) {
                var targetTotalHours = totalHoursNextUpdate;
                cond = GetClimateConditionAtSpecificTime(Pos, targetTotalHours, cond);
                if (cond != null) {
                    var temperature = cond.Temperature;
                    var time = TimeUtil.ToRoundedTotalMinutesN(targetTotalHours);
                    temperatureRecorder.AddSample(new TemperatureSample(time, temperature));
                    hasUpdate = true;
                }
                else {
                    var datetime = new VSDateTime(Api.World.Calendar, TimeSpan.FromHours(targetTotalHours));
                    Api.Logger.Warning($"Couldn't sample temperature at {datetime.PrettyDate()}");
                    UpdateTimes(targetTotalHours);
                    break;
                }

                // Calculates and sets new totalHoursNextUpdate
                UpdateTimes(targetTotalHours);
            }

            if (hasUpdate) {
                temperatureRecorder.CleanUpSamplesByMinTime(TimeUtil.ToRoundedTotalMinutesN(minTotalHours));
                MarkDirty();
            }
        }

        public static int NextUpdateRoundedTotalMinutes(float vsHourPerDay, double totalHours, int intervalMinutes) {
            int minutesPerDay = TimeUtil.MinutesPerDay(vsHourPerDay);

            int totalMinutesN = TimeUtil.ToRoundedTotalMinutesN(totalHours);

            int minutesInDay = totalMinutesN % minutesPerDay;
            // Update is at every {intervalMinutes} minutes from 00:00, but...
            int nextMinutesInDay = (minutesInDay / intervalMinutes + 1) * intervalMinutes;
            // If next is a new day, it should be 00:00
            int deltaMinutes = Math.Min(nextMinutesInDay, minutesPerDay) - minutesInDay;
            int nextTotalMinutesN = totalMinutesN + deltaMinutes;

            return nextTotalMinutesN;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve) {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            totalHoursLastUpdate = tree.GetDouble("totalHoursLastUpdate");
            totalHoursNextUpdate = tree.GetDouble("totalHoursNextUpdate");
            if (tree["temperatureSamples"] is TreeAttribute samplesAttribute) {
                var samplesDecoded = VSAttributeDecoder.DecodeTemperatureSamples(samplesAttribute);
                temperatureRecorder.SetSamples(samplesDecoded);
            }
            var guiSettingData = tree.GetBytes("guiSetting");
            if (guiSettingData != null) {
                guiSetting = SerializerUtil.Deserialize<BEAirThermoGuiSetting>(guiSettingData);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);

            tree.SetDouble("totalHoursLastUpdate", totalHoursLastUpdate);
            tree.SetDouble("totalHoursNextUpdate", totalHoursNextUpdate);
            var samplesAttribute = VSAttributeEncoder.EncodeTemperatureSamples(temperatureRecorder.TemperatureSamples);
            tree["temperatureSamples"] = samplesAttribute;
            tree.SetBytes("guiSetting", SerializerUtil.Serialize(guiSetting));

        }

        bool OnReverseOrderButtonClicked() {
            guiSetting = guiSetting with { TableSortOrder = (guiSetting.TableSortOrder == "asc" ? "desc" : "asc") };

            if (Api is ICoreClientAPI capi) {
                capi.Network.SendBlockEntityPacket(Pos, (int)AirThermoPacketId.GuiSettingChanged, guiSetting);
            }

            clientDialog?.SetupDialog(temperatureRecorder.TemperatureSamples, guiSetting.TableSortOrder);

            return true;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data) {
            base.OnReceivedClientPacket(fromPlayer, packetid, data);

            if (packetid == (int)AirThermoPacketId.GuiSettingChanged) {
                var received = SerializerUtil.Deserialize<BEAirThermoGuiSetting>(data);
                guiSetting = received;
                MarkDirty();
            }
        }

        /// <summary>
        /// Schedule sampling during the whole retention period.
        /// </summary>
        /// <remarks>
        /// On next update, this air thermometer will sample all temperature as if 
        /// it has been there since ([current datetime] - [retention period]) 
        /// </remarks>
        public void ScheduleForceSampleOverRetentionPeriod() {
            if (Api.Side != EnumAppSide.Server) return;

            var currentTotalHours = Api.World.Calendar.TotalHours;

            // Allowed oldest sample and sampling older than this point will be skipped
            var retentionPeriodStart = currentTotalHours - TimeUtil.TotalYearsToTotalHours(Api.World.Calendar, retentionPeriodYear);
            if (retentionPeriodStart < 0) retentionPeriodStart = 0;

            UpdateTimes(retentionPeriodStart);
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null) {
            base.OnBlockPlaced(byItemStack);

            UpdateTimes(Api.World.Calendar.TotalHours);
        }

        public override void OnBlockRemoved() {
            base.OnBlockRemoved();

            clientDialog?.TryClose();
        }

    }
}