public static class GameConfigProvider
{
    private static GameConfig _config;

    public static GameConfig Config
    {
        get
        {
            if (_config == null)
                _config = new GameConfig();  
            
            return _config;
        }
    }
}