using System;
using System.Collections.Generic;

namespace Colyseus
{
    public static class ObjectExtensions
    {
        public static T ToObject<T>(object source) where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in (IDictionary<string, object>)source) {
                var field = someObjectType.GetField(item.Key);
                if (field == null)
                {
                    continue;
                }

				try
				{
					field.SetValue(someObject, Convert.ChangeType(item.Value, field.FieldType));

				} catch (OverflowException) {
					// workaround for parsing Infinity on RoomAvailable.maxClients
					field.SetValue(someObject, uint.MaxValue);
				}
            }

            return someObject;
        }
    }
}
