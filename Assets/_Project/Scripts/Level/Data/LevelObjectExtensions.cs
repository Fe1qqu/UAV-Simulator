public static class LevelObjectExtensions
{
    public static int GetInt(this LevelObject levelObject, string key, int defaultValue = 0)
    {
        if (levelObject == null)
        {
            return defaultValue;
        }

        return levelObject.Properties.GetInt(key, defaultValue);
    }
}
