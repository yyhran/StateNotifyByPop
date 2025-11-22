using System.IO;
using System.Reflection;

namespace StateNotifyByPop
{
    public class HarmonyLoad
    {
        public static Assembly Load0Harmony()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string currentNamespace = typeof(HarmonyLoad).Namespace;
            using (Stream stream = executingAssembly.GetManifestResourceStream(currentNamespace + ".0Harmony.dll"))
            using (MemoryStream ms = new MemoryStream())
            {
                stream?.CopyTo(ms);
                Assembly assembly = Assembly.Load(ms.ToArray());
                return assembly;
            }
        }
    }
}
