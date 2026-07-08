using newrisourcecenter.Controllers;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace newrisourcecenter.Internals
{
    public class EmailSchedular
    {
        // Static reference to keep the scheduler alive in memory
        private static IScheduler _scheduler;

        public static async Task Start()
        {
            try
            {
                if (_scheduler != null && _scheduler.IsStarted) return;

                ISchedulerFactory schedFact = new StdSchedulerFactory();
                _scheduler = await schedFact.GetScheduler();
                await _scheduler.Start();

                IJobDetail job = JobBuilder.Create<RemoveUnregisteredJob>()
                    .WithIdentity("dailyJob", "group1")
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("dailyTrigger", "group1")
                    .StartNow() // Fire immediately on app start
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(24) // Run every 24 hours
                        .RepeatForever())
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);

                // LOG THIS: To prove the scheduler actually reached this line
                new CommonController().FileLog("Quartz Scheduler started successfully.", "System_Startup");
            }
            catch (Exception ex)
            {
                new CommonController().FileLog("Quartz Start Error: " + ex.Message, "System_Error");
            }
        }
    }
}