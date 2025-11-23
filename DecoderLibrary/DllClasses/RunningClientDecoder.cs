using System;
using System.Collections.Generic;
using System.Reflection;

namespace DecoderLibrary
{
    public class RunningClientDecoder
    {

        public RunningClientDecoder()
        {
            
        }

        public Dictionary<string, int> CallToClientDecoder<IcdDataType>(Dictionary<string, IcdDataType> icdItemDictioanry, Dictionary<string, int> frameDictionary, string dllFileAddress)
        {
            Assembly assembly = Assembly.LoadFile(dllFileAddress);

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                ConstructorInfo[] constructorInfos = type.GetConstructors();
                foreach (ConstructorInfo constructorInfo in constructorInfos)
                {
                    if (ReflectionToParametersInConstructors<IcdDataType>(constructorInfo.GetParameters()))
                    {
                        return ReflectionToFunctionsInClass(type, icdItemDictioanry, frameDictionary);
                    }
                }
            }
            return null;
        }

        private bool ReflectionToParametersInConstructors<IcdDataType>(ParameterInfo[] parameterInfos)
        {
            bool consContainIcdItems = false; bool consContainFrameDictionary = false;

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                if (parameterInfos[i].ParameterType == typeof(Dictionary<string, IcdDataType>))
                    consContainIcdItems = true;
                if (parameterInfos[i].ParameterType == typeof(Dictionary<string, int>))
                    consContainFrameDictionary = true;
            }

            if (consContainIcdItems && consContainFrameDictionary && parameterInfos.Length == 2)
                return true;

            return false;
        }

        private Dictionary<string, int> ReflectionToFunctionsInClass<IcdDataType>(Type type, Dictionary<string, IcdDataType> icdItemDictioanry, Dictionary<string, int> frameDictionary)
        {
            Dictionary<string, int> frameDictioanryClient = new Dictionary<string, int>();
            MethodInfo[] methods = type.GetMethods();

            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.IsPublic)
                {
                    if (methodInfo.ReturnType == typeof(Dictionary<string, int>))
                    {
                        object obj = Activator.CreateInstance(type, new object[] { icdItemDictioanry, frameDictionary });
                        frameDictioanryClient = (Dictionary<string, int>)methodInfo.Invoke(obj, null);
                        break;
                    }
                }
            }

            return frameDictioanryClient;
        }
    }
}
