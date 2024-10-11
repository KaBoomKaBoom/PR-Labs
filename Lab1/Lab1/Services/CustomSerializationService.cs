using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Lab1.Services
{
    public class CustomSerializationService
    {
        public string Serialize<T>(T obj)
        {
            var serialized = new StringBuilder();
            var typeName = obj.GetType().Name;

            serialized.Append($"[{typeName}]");

            var propreties = obj.GetType().GetProperties();
            foreach(var prop in propreties)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);

                serialized.Append($"{name}<{value};");
            }

            return serialized.ToString();
        }

        public string SerializeList<T>(List<T> objList)
        {
            var serialized = new StringBuilder();
            
            for (int i = 0; i < objList.Count; i++)
            {
                serialized.Append(Serialize(objList[i]));
                
                // Only add the pipe if this is not the last item
                if (i < objList.Count - 1)
                {
                    serialized.Append("|");
                }
            }

            return serialized.ToString();
        }

        public T Deserialize<T>(string serializedData) where T : new()
        {
            var obj = new T();
            var typeName = typeof(T).Name;

            var dataStart = serializedData.IndexOf($"[{typeName}]") + typeName.Length + 2;

            var keyValuePairs = serializedData.Substring(dataStart)
                                            .Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach(var pair in keyValuePairs)
            {
                var keyValue = pair.Split("<");

                var propertyName = keyValue[0];
                var propertyValue = keyValue[1];

                var propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                    propertyInfo.SetValue(obj, convertedValue);
                }
            }
            return obj;
        }

         public List<T> DeserializeList<T>(string serializedData) where T : new()
        {
            var objList = new List<T>();

            // Split the serialized data by the pipe separator, which separates each object
            var serializedObjects = serializedData.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var serializedObj in serializedObjects)
            {
                var deserializedObj = Deserialize<T>(serializedObj);
                objList.Add(deserializedObj);
            }

            return objList;
        }
    }
}