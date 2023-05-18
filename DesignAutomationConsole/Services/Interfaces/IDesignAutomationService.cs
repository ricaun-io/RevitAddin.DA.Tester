﻿using System;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public interface IDesignAutomationService
    {
        public string[] CoreEngineVersions();
        public Task Initialize(string packagePath);
        public Task Delete();
        public Task<bool> Run<T>(string engine = null) where T : class;
        public Task<bool> Run<T>(Action<T> options, string engine = null) where T : class;
        public Task<bool> Run<T>(T options, string engine = null) where T : class;
    }
}