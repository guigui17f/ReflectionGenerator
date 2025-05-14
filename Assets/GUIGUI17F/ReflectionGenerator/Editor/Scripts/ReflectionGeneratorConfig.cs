using UnityEngine;
 
namespace GUIGUI17F.ReflectionGenerator
{
    /// <summary>
    /// scriptable object to store reflection generator configs
    /// </summary>
    public class ReflectionGeneratorConfig : ScriptableObject
    {
        [Tooltip("whether auto modify the name from fields")]
        public bool ModifyOriginName;
        [Tooltip("the prefix of \"Get\" method name")]
        public string GetMethodPrefix;
        [Tooltip("the prefix of \"Set\" method name")]
        public string SetMethodPrefix;
        [Tooltip("the postfix of \"Get\" method name")]
        public string GetMethodPostfix;
        [Tooltip("the postfix of \"Set\" method name")]
        public string SetMethodPostfix;
        [Tooltip("generated wrappers save directory")]
        public string WrapperSaveDirectory;
    }
}