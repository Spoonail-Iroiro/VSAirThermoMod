using AirThermoMod.Common;
using AirThermoMod.Core;
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

namespace AirThermoMod.BlockEntities
{

    internal class BEAirThermo : BlockEntity
    {
        protected int intervalMinutes = 30;

        protected double totalHoursLastUpdate;

        protected int nextUpdateRoundedTotalMinutes;

        protected TemperatureRecorder temperatureRecorder = new();

        public override void Initialize(ICoreAPI api)
        {
            api.Logger.Event("Initializing AirThermo...");
            base.Initialize(api);

            if (api is ICoreServerAPI)
            {
                // TODO: jitter
                RegisterGameTickListener(Update, 3300);
            }

        }

        private string getFormatedStatus()
        {
            StringBuilder sb = new();
            var calendar = Api.World.Calendar;
            sb.AppendLine($"Now: {calendar.PrettyDate()}");
            sb.AppendLine($"Last Update:{new VSDateTime(calendar, TimeSpan.FromHours(totalHoursLastUpdate)).PrettyDate()}");
            var nextUpdateVSDateTime = new VSDateTime(calendar, TimeSpan.FromMinutes(nextUpdateRoundedTotalMinutes));
            sb.AppendLine($"Next Update: {nextUpdateVSDateTime.PrettyDate()}");
            sb.AppendLine($"Samples: {temperatureRecorder.SimpleDescription()}");

            return sb.ToString();
        }

        // Update lastUpdateTotalHours and calculate nextUpdateRoundedTotalMinutes
        protected void UpdateTimes(double totalHoursLastUpdate)
        {
            this.totalHoursLastUpdate = totalHoursLastUpdate;

            nextUpdateRoundedTotalMinutes = NextUpdateRoundedTotalMinutes(Api.World.Calendar.HoursPerDay, totalHoursLastUpdate, intervalMinutes);
        }

        public void OnBlockPlaced()
        {
            UpdateTimes(Api.World.Calendar.TotalHours);
        }

        public bool Interact(IWorldAccessor world, IPlayer byPlayer)
        {
            Api.Logger.Event("Interacting...");
            var calendar = Api.World.Calendar;
            if (Api.Side == EnumAppSide.Client)
            {
                Api.Logger.Event("[Client]" + getFormatedStatus());
            }

            IServerPlayer splr = byPlayer as IServerPlayer;
            if (splr != null)
            {
                var conds = world.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.NowValues);
                var temperature = conds == null ? 0 : conds.Temperature;
                var time = TimeUtil.ToRoundedTotalMinutesN(calendar.TotalHours);
                temperatureRecorder.AddSample(new TemperatureSample { Time = time, Temperature = temperature });
                UpdateTimes(calendar.TotalHours);

                splr.SendMessage(GlobalConstants.InfoLogChatGroup, getFormatedStatus(), EnumChatType.Notification);
            }
            return true;
        }
        public void Update(float dt)
        {
            if (!(Api as ICoreServerAPI).World.IsFullyLoadedChunk(Pos)) return;

        }
        public int NextUpdateRoundedTotalMinutes(float vsHourPerDay, double totalHours, int intervalMinutes)
        {
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

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }
    }
}
