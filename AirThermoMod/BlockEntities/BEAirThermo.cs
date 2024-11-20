using AirThermoMod.Common;
using AirThermoMod.Core;
using AirThermoMod.VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace AirThermoMod.BlockEntities {

    internal class BEAirThermo : BlockEntity {
        protected int intervalMinutes = 30;

        protected double totalHoursLastUpdate;

        protected double totalHoursNextUpdate;

        protected TemperatureRecorder temperatureRecorder = new();

        public override void Initialize(ICoreAPI api) {
            api.Logger.Event("Initializing AirThermo...");
            base.Initialize(api);

            if (api is ICoreServerAPI) {
                // TODO: jitter
                //RegisterGameTickListener(Update, 3300);
            }

        }

        private string getFormattedStatus() {
            StringBuilder sb = new();
            var calendar = Api.World.Calendar;
            sb.AppendLine($"Now: {calendar.PrettyDate()}");
            sb.AppendLine($"Last Update:{new VSDateTime(calendar, TimeSpan.FromHours(totalHoursLastUpdate)).PrettyDate()}");
            var nextUpdateVSDateTime = new VSDateTime(calendar, TimeSpan.FromHours(totalHoursNextUpdate));
            sb.AppendLine($"Next Update: {nextUpdateVSDateTime.PrettyDate()}");
            sb.AppendLine($"Samples: {temperatureRecorder.SimpleDescription()}");

            return sb.ToString();
        }

        // Update lastUpdateTotalHours and calculate nextUpdateRoundedTotalMinutes
        protected void UpdateTimes(double totalHoursLastUpdate) {
            if (Api.Side == EnumAppSide.Server) {
                this.totalHoursLastUpdate = totalHoursLastUpdate;

                var timeMinutes = NextUpdateRoundedTotalMinutes(Api.World.Calendar.HoursPerDay, totalHoursLastUpdate, intervalMinutes);

                totalHoursNextUpdate = TimeUtil.RoundedTotalMinutesToTotalHours(timeMinutes);

                MarkDirty();
            }
        }

        public void OnBlockPlaced() {
            UpdateTimes(Api.World.Calendar.TotalHours);
        }

        public bool Interact(IWorldAccessor world, IPlayer byPlayer) {
            Api.Logger.Event("Interacting...");
            var calendar = Api.World.Calendar;
            if (Api.Side == EnumAppSide.Client) {
                Api.Logger.Event("[Client]" + getFormattedStatus());
            }

            IServerPlayer splr = byPlayer as IServerPlayer;
            if (splr != null) {
                UpdateFromLastTime();

                splr.SendMessage(GlobalConstants.InfoLogChatGroup, getFormattedStatus(), EnumChatType.Notification);
            }
            return true;
        }

        protected void SampleTemperaturePeriodical(double totalHoursUntil) {
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
        }

        protected void UpdateFromLastTime() {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

            var currentTotalHours = Api.World.Calendar.TotalHours;

            ClimateCondition cond = null;
            // TODO: Don't update more than one year (or configured logging range) past
            while (!(currentTotalHours < totalHoursNextUpdate)) {
                var targetTotalHours = totalHoursNextUpdate;
                cond = GetClimateConditionAtSpecificTime(Pos, targetTotalHours, cond);
                if (cond != null) {
                    var temperature = cond == null ? 0 : cond.Temperature;
                    var time = TimeUtil.ToRoundedTotalMinutesN(targetTotalHours);
                    temperatureRecorder.AddSample(new TemperatureSample { Time = time, Temperature = temperature });
                }
                else {
                    var datetime = new VSDateTime(Api.World.Calendar, TimeSpan.FromHours(targetTotalHours));
                    Api.Logger.Warning($"Couldn't sample temperature at {datetime.PrettyDate()}");
                    UpdateTimes(targetTotalHours);
                    break;
                }

                // Calculates and sets new totalHoursNextUpdate
                UpdateTimes(targetTotalHours);

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
            if (tree["temperatureSamples"] is TreeArrayAttribute samplesAttribute) {
                var samplesDecoded = samplesAttribute.value.Select(attr => VSAttributeDecoder.Decode<TemperatureSample>(attr));
                temperatureRecorder.SetSamples(samplesDecoded);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);

            tree.SetDouble("totalHoursLastUpdate", totalHoursLastUpdate);
            tree.SetDouble("totalHoursNextUpdate", totalHoursNextUpdate);
            var samplesAttribute = VSAttributeEncoder.Encode(temperatureRecorder.TemperatureSamples);
            tree["temperatureSamples"] = samplesAttribute;
        }
    }
}