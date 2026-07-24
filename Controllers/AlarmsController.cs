using System;
using System.Collections.Generic;
using FlightPlan.Core.Models;
using FlightPlan.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmsController : ControllerBase
    {
        private readonly IAlarmService _alarmService;

        public AlarmsController(IAlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        [HttpGet]
        public IEnumerable<AlarmItem> Get()
        {
            return _alarmService.GetAllAlarms();
        }

        [HttpPost]
        public IActionResult Create([FromBody] AlarmItem alarm)
        {
            if (alarm == null) return BadRequest();
            _alarmService.AddAlarm(alarm);
            return Ok(alarm);
        }

        [HttpPut]
        public IActionResult Update([FromBody] AlarmItem alarm)
        {
            if (alarm == null) return BadRequest();
            _alarmService.UpdateAlarm(alarm);
            return Ok(alarm);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _alarmService.DeleteAlarm(id);
            return Ok();
        }
    }
}
