using System.Reflection.Emit;

namespace ProcessProbe;

public static class SizeOf<T>
{
    public static readonly int Size;

    static SizeOf()
    {
        DynamicMethod dm = new("GenericSizeOf", typeof(uint), Array.Empty<Type>());
        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Sizeof, typeof(T));
        il.Emit(OpCodes.Ret);
        Size = (int)dm.Invoke(null, null);
    }
}