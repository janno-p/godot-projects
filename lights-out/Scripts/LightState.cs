namespace LightsOut.Scripts;

public record On
{
    public static On Value => new();
}

public record Off
{
    public static Off Value => new();
}

public readonly union LightState(On, Off)
{
    public LightState Toggle()
    {
        return this switch
        {
            On => Off.Value,
            Off => On.Value,
        };
    }
};
