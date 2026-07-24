using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;
using System.Runtime.InteropServices;
using Quartz;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly IWebHostEnvironment _env;
    private readonly ISchedulerFactory _schedulerFactory;

    public DebugController(IStorageService storageService, IWebHostEnvironment env, ISchedulerFactory schedulerFactory)
    {
        _storageService = storageService;
        _env = env;
        _schedulerFactory = schedulerFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetDebugInfo()
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var metaData = await scheduler.GetMetaData();

        var info = new
        {
            Paths = new
            {
                BaseStoragePath = Path.GetDirectoryName(_storageService.GetConfigPath()),
                ConfigPath = _storageService.GetConfigPath(),
                ProjectsPath = _storageService.GetProjectsPath(),
                RulesDirectory = _storageService.GetRulesDirectory(),
                NotesDirectory = _storageService.GetNotesDirectory(),
                WebRootPath = _env.WebRootPath,
                ContentRootPath = _env.ContentRootPath
            },
            SystemInfo = new
            {
                OS = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.OSArchitecture.ToString(),
                Framework = RuntimeInformation.FrameworkDescription,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                EntryAssembly = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
                Environment = _env.EnvironmentName
            },
            SchedulerInfo = new
            {
                Name = scheduler.SchedulerName,
                InstanceId = scheduler.SchedulerInstanceId,
                Status = scheduler.IsStarted ? (scheduler.IsShutdown ? "Shutdown" : (scheduler.InStandbyMode ? "Standby" : "Running")) : "Not Started",
                JobCount = metaData.NumberOfJobsExecuted,
                SchedulerType = scheduler.GetType().Name,
                QuartzVersion = metaData.Version,
                IsRemote = metaData.SchedulerRemote,
                ThreadCount = metaData.ThreadPoolSize
            },
            EnvironmentVariables = Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Where(e => ((string)e.Key).Contains("ASPNETCORE_") || ((string)e.Key).Contains("VITE_"))
                .ToDictionary(e => (string)e.Key, e => (string)e.Value)
        };

        return Ok(info);
    }
}
