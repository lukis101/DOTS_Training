
// Global input data buffer
// https://csharpindepth.com/articles/singleton
public sealed class DMXInputSingleton
{
    public static readonly DMXInputSingleton instance = new DMXInputSingleton();

    public const int UNIVERSES_MAX = 4;
    public byte[] values;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static DMXInputSingleton() { }

    private DMXInputSingleton()
    {
        values = new byte[UNIVERSES_MAX * 256];
    }
}
