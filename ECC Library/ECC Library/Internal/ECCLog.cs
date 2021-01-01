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
            string text = string.Format("ECC: {0}", message);
            AddMessageInternal(text);
        }
        public static void AddMessage(string format, string[] args)
        {
            string text = string.Format("ECC: {0}", string.Format(format, args));
            AddMessageInternal(text);
        }
        public static void AddMessage(string format, object arg1)
        {
            string text = string.Format("ECC: {0}", string.Format(format, arg1));
            AddMessageInternal(text);
        }
        public static void AddMessage(string format, object arg1, object arg2)
        {
            string text = string.Format("ECC: {0}", string.Format(format, arg1, arg2));
            AddMessageInternal(text);
        }
        public static void AddMessage(string format, object arg1, object arg2, object arg3)
        {
            string text = string.Format("ECC: {0}", string.Format(format, arg1, arg2, arg3));
            AddMessageInternal(text);
        }

        private static void AddMessageInternal(string message)
        {
            if (ECCPatch.config.ECCLogMessages)
            {
                ErrorMessage.AddMessage(message);
            }
            Debug.LogError(message);
        }
    }
}
