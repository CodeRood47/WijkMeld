using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkMeld.App.Services
{
    public class NavigationService : INavigationService
    {
        public virtual Task GoToAsync(string route)
        {
           
            if (Shell.Current != null)
            {
                return Shell.Current.GoToAsync(route);
            }
           
            return Task.CompletedTask; 
        }
    }
}
