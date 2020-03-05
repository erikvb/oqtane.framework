﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roles;
        private readonly ILogManager _logger;

        public RoleController(IRoleRepository Roles, ILogManager logger)
        {
            _roles = Roles;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<Role> Get(string siteid)
        {
            return _roles.GetRoles(int.Parse(siteid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Role Get(int id)
        {
            return _roles.GetRole(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Role Post([FromBody] Role Role)
        {
            if (ModelState.IsValid)
            {
                Role = _roles.AddRole(Role);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Role Added {Role}", Role);
            }
            return Role;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Role Put(int id, [FromBody] Role Role)
        {
            if (ModelState.IsValid)
            {
                Role = _roles.UpdateRole(Role);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Role Updated {Role}", Role);
            }
            return Role;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _roles.DeleteRole(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Role Deleted {RoleId}", id);
        }
    }
}
