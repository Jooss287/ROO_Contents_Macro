using System;
using FishingContents.Extension;
using Newtonsoft.Json.Linq;

namespace FishingContents.VersionCheck
{
    class ProgramVersion
    {
        private const string _PROGRAM_VER = "v1.3";
        private const string LATEST_VER_API_URL = "https://api.github.com/repos/Jooss287/ROO_Contents_Macro/releases/latest";

        public static string Ver
        {
            get { return _PROGRAM_VER; }
        }

        public static string CopyRight()
        {
            string copyRight = ("CopyRight. Jooss287@gmail.com (Blurrr)\n" +
                "소스공개: https://github.com/Jooss287/Auto-clicker \n" +
                "오픈소스: EmguCV(OpenCV) / Newtonsoft.Json");
            return copyRight;
        }
        

        public static string GetLeastVersion()
        {
            string api_response = APIextension.callWebClient(LATEST_VER_API_URL);
            var r = JObject.Parse(api_response);
            return Convert.ToString(r["tag_name"]);
        }
        public static string GetLeastURL()
        {
            string api_response = APIextension.callWebClient(LATEST_VER_API_URL);
            var r = JObject.Parse(api_response);
            return Convert.ToString(r["html_url"]);
        }

        public static bool IsLastestVer()
        {
            return String.Equals(GetLeastVersion(), _PROGRAM_VER);
        }
    }
}
