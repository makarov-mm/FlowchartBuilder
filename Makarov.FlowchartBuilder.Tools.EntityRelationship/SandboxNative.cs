using System.Reflection;
using System.Runtime.InteropServices;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

// Windows includes SQLite in System32. The other platforms use their system SQLite for model tests.
internal static class SandboxNative
{
    private const string Library = "FlowchartBuilder.SystemSQLite";

    static SandboxNative() => NativeLibrary.SetDllImportResolver(typeof(SandboxNative).Assembly, Resolve);

    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != Library) return IntPtr.Zero;
        if (OperatingSystem.IsWindows()) return NativeLibrary.Load("winsqlite3.dll", assembly, DllImportSearchPath.System32);
        return NativeLibrary.Load(OperatingSystem.IsMacOS() ? "/usr/lib/libsqlite3.dylib" : "libsqlite3.so.0");
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate int Authorize(IntPtr context, int action, IntPtr first, IntPtr second, IntPtr db, IntPtr trigger);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate int Progress(IntPtr context);

    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_open([MarshalAs(UnmanagedType.LPUTF8Str)] string name, out IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_close(IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr sqlite3_errmsg(IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_prepare16_v2(IntPtr database, IntPtr sql, int byteCount, out IntPtr statement, out IntPtr tail);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_step(IntPtr statement);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_finalize(IntPtr statement);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_column_count(IntPtr statement);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr sqlite3_column_name16(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_column_type(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_column_bytes(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr sqlite3_column_text16(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_column_bytes16(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_total_changes(IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_get_autocommit(IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_set_authorizer(IntPtr database, Authorize callback, IntPtr context);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern void sqlite3_progress_handler(IntPtr database, int instructions, Progress callback, IntPtr context);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_limit(IntPtr database, int category, int limit);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern double sqlite3_column_double(IntPtr statement, int column);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_null(IntPtr statement, int index);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_int64(IntPtr statement, int index, long value);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_double(IntPtr statement, int index, double value);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_text(IntPtr statement, int index, byte[] bytes, int length, IntPtr destructor);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_blob(IntPtr statement, int index, byte[] bytes, int length, IntPtr destructor);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern int sqlite3_bind_zeroblob(IntPtr statement, int index, int length);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern long sqlite3_last_insert_rowid(IntPtr database);
    [DllImport(Library, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr sqlite3_column_blob(IntPtr statement, int column);
}
