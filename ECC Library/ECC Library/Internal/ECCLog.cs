using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECCLibrary.Internal
{
    public static class ECCLog
    {
        public static void AddMessage(string message)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                string text = string.Format("ECC: {0}", message);
                ErrorMessage.AddMessage(text);
                Debug.LogError(text);
            }
        }
        public static void AddMessage(string format, string[] args)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                string text = string.Format("ECC: {0}", string.Format(format, args));
                ErrorMessage.AddMessage(text);
                Debug.LogError(text);
            }
        }
        public static void AddMessage(string format, object arg1)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                string text = string.Format("ECC: {0}", string.Format(format, arg1));
                ErrorMessage.AddMessage(text);
                Debug.LogError(text);
            }
        }
        public static void AddMessage(string format, object arg1, object arg2)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                string text = string.Format("ECC: {0}", string.Format(format, arg1, arg2));
                ErrorMessage.AddMessage(text);
                Debug.LogError(text);
            }
        }
        public static void AddMessage(string format, object arg1, object arg2, object arg3)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                string text = string.Format("ECC: {0}", string.Format(format, arg1, arg2, arg3));
                ErrorMessage.AddMessage(text);
                Debug.LogError(text);
            }
        }
    }
}
