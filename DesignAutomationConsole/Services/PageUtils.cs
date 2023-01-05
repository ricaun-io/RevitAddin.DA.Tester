using Autodesk.Forge.DesignAutomation.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public static class PageUtils
    {
        public static async Task<List<T>> GetAllItems<T>(Func<string, Task<Page<T>>> pageGetter)
        {
            var ret = new List<T>();
            string paginationToken = null;
            do
            {
                var resp = await pageGetter(paginationToken);
                paginationToken = resp.PaginationToken;
                ret.AddRange(resp.Data);
            }
            while (paginationToken != null);
            return ret;
        }

        public static async Task<List<T>> GetAllItems<T, P>(Func<P, string, Task<Page<T>>> pageGetter, P parameter)
        {
            var ret = new List<T>();
            string paginationToken = null;
            do
            {
                var resp = await pageGetter(parameter, paginationToken);
                paginationToken = resp.PaginationToken;
                ret.AddRange(resp.Data);
            }
            while (paginationToken != null);
            return ret;
        }
    }
}