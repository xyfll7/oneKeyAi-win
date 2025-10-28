using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oneKeyAi_win.Configuration
{
    public class UserConfig
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "zh-CN";
        public string ApiEndpoint { get; set; } = "https://api.example.com";
        public int RefreshInterval { get; set; } = 30;
    }
}
