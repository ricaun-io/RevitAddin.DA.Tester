using Autodesk.Forge.DesignAutomation.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public static class PageUtils
    {
        /// <summary>
        /// Retrieves a list of items of type T by calling the provided pageGetter function repeatedly
        /// until all pages have been retrieved.
        /// </summary>
        /// <typeparam name="T">The type of the items being retrieved.</typeparam>
        /// <param name="pageGetter">A function that takes a pagination token and returns a page of items of type T.</param>
        /// <returns>A list of items of type T.</returns>
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

        /// <summary>
        /// Retrieves a list of items of type T by calling the provided pageGetter function repeatedly
        /// until all pages have been retrieved.
        /// </summary>
        /// <typeparam name="T">The type of the items being retrieved.</typeparam>
        /// <typeparam name="P">The type of the parameter being passed to the pageGetter function.</typeparam>
        /// <param name="pageGetter">A function that takes a parameter of type P and a pagination token, and returns a page of items of type T.</param>
        /// <param name="parameter">The parameter to be passed to the pageGetter function.</param>
        /// <returns>A list of items of type T.</returns>
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