using System;
using System.Reflection;

namespace _2Scripts.Helpers
{
    static class StructureAccessMethods
    {
        /// <summary>
        /// Allow us to get the number of element in a structure
        /// </summary>
        /// <param name="structure"> structure to inspect</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetNumberOfElementsInStruct<T>(T structure) where T : struct
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fields.Length + properties.Length;
        }
        
        /// <summary>
        /// Allow us to get a element of a structure by his index
        /// </summary>
        /// <param name="structure"> structure to inspect</param>
        /// <param name="index"> index of the element we want to get</param>
        /// <typeparam name="T"> type of the element we want to get</typeparam>
        /// <returns></returns>
        public static T GetStructElementByIndex<T>(object structure, int index)
        {
            Type structType = structure.GetType();
            FieldInfo[] fields = structType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PropertyInfo[] properties = structType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (index < fields.Length)
            {
                return (T)fields[index].GetValue(structure);
            }
            else
            {
                return (T)properties[index - fields.Length].GetValue(structure);
            }
        }
    }
}