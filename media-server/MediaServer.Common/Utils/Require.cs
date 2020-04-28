using System;

namespace MediaServer.Common.Utils
{
    public static class Require
    {
        public static void NotNullOrWhiteSpace(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException();
            }
        }

        public static void NotNull<T>(T value)
            where T : class
        {
            if(value is null)
            {
                throw new ArgumentNullException($"{typeof(T).FullName} is NULL");
            }
        }
        
        public static void NotEmpty<T>(T value)
            where T : struct
        {
            if(value.Equals(default))
            {
                throw new ArgumentNullException($"{typeof(T).FullName} is empty");
            }
        }
    }
}
