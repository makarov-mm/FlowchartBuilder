using System.Runtime.InteropServices;

namespace Makarov.Framework.Graphics.Interop;

/// <summary>Calls COM methods through the vtable of an interface pointer.</summary>
internal static unsafe class Com
{
    /// <summary>Address of the method in <paramref name="slot"/> of the vtable of <paramref name="instance"/>.</summary>
    public static void* Method(nint instance, int slot) => (*(void***)instance)[slot];

    /// <summary>Releases the interface pointer, if any, and clears it.</summary>
    public static void Release(ref nint instance)
    {
        nint value = instance;
        if (value == 0)
            return;

        instance = 0;
        ((delegate* unmanaged[Stdcall]<nint, uint>)Method(value, Slots.IUnknown_Release))(value);
    }

    public static void Check(int hresult)
    {
        if (hresult < 0)
            Marshal.ThrowExceptionForHR(hresult);
    }
}
