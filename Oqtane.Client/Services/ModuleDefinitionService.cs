﻿using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System;
using System.Reflection;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ModuleDefinitionService : ServiceBase, IModuleDefinitionService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public ModuleDefinitionService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            _http = http;
            _siteState = sitestate;
            _navigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "ModuleDefinition"); }
        }

        public async Task<List<ModuleDefinition>> GetModuleDefinitionsAsync(int SiteId)
        {
            List<ModuleDefinition> moduledefinitions = await _http.GetJsonAsync<List<ModuleDefinition>>(apiurl + "?siteid=" + SiteId.ToString());
            return moduledefinitions.OrderBy(item => item.Name).ToList();
        }

        public async Task<ModuleDefinition> GetModuleDefinitionAsync(int ModuleDefinitionId, int SiteId)
        {
            return await _http.GetJsonAsync<ModuleDefinition>(apiurl + "/" + ModuleDefinitionId.ToString() + "?siteid=" + SiteId.ToString());
        }

        public async Task UpdateModuleDefinitionAsync(ModuleDefinition ModuleDefinition)
        {
            await _http.PutJsonAsync(apiurl + "/" + ModuleDefinition.ModuleDefinitionId.ToString(), ModuleDefinition);
        }

        public async Task InstallModuleDefinitionsAsync()
        {
            await _http.GetJsonAsync<List<string>>(apiurl + "/install");
        }

        public async Task DeleteModuleDefinitionAsync(int ModuleDefinitionId, int SiteId)
        {
            await _http.DeleteAsync(apiurl + "/" + ModuleDefinitionId.ToString() + "?siteid=" + SiteId.ToString());
        }

        public async Task LoadModuleDefinitionsAsync(int SiteId)
        {
            // get list of modules from the server
            List<ModuleDefinition> moduledefinitions = await GetModuleDefinitionsAsync(SiteId);

            // get list of loaded assemblies on the client ( in the client-side hosting module the browser client has its own app domain )
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (ModuleDefinition moduledefinition in moduledefinitions)
            {
                // if a module has dependencies, check if they are loaded
                if (moduledefinition.Dependencies != "")
                {
                    foreach (string dependency in moduledefinition.Dependencies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string assemblyname = dependency.Replace(".dll", "");
                        if (assemblies.Where(item => item.FullName.StartsWith(assemblyname + ",")).FirstOrDefault() == null)
                        {
                            // download assembly from server and load
                            var bytes = await _http.GetByteArrayAsync(apiurl + "/load/" + assemblyname + ".dll");
                            Assembly.Load(bytes);
                        }
                    }
                }
                // check if the module assembly is loaded
                if (assemblies.Where(item => item.FullName.StartsWith(moduledefinition.AssemblyName + ",")).FirstOrDefault() == null)
                {
                    // download assembly from server and load
                    var bytes = await _http.GetByteArrayAsync(apiurl + "/load/" + moduledefinition.AssemblyName + ".dll");
                    Assembly.Load(bytes);
                }
            }
        }
    }
}
