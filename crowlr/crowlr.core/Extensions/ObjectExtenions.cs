namespace crowlr.core
{
    public static class ObjectExtenions
    {
        public static bool IsNull<T>(this T obj)
        {
            return obj is string
                ? string.IsNullOrWhiteSpace(obj as string)
                : obj == null;
        }
    }
}
