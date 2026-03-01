using System.Runtime.InteropServices;

namespace NativeAOTTest;

// NativeAOT-compatible P/Invoke declarations using LibraryImport
// LibraryImport is preferred over DllImport for NativeAOT as it generates source code at compile-time
// instead of relying on runtime reflection, making the code statically analyzable and compatible with AOT compilation
internal static partial class Raylib
{
    // EntryPoint specifies the exact C function name to call in the native raylib library
    // StringMarshalling.Utf8 ensures .NET strings are properly converted to UTF8 C strings
    [LibraryImport("raylib", EntryPoint = "InitWindow", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void InitWindow(int width, int height, string title);

    // MarshalAs(UnmanagedType.I1) converts the C bool (4 bytes) to .NET bool (1 byte)
    [LibraryImport("raylib", EntryPoint = "WindowShouldClose")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool WindowShouldClose();

    // BeginDrawing marks the start of a frame's drawing operations
    [LibraryImport("raylib", EntryPoint = "BeginDrawing")]
    internal static partial void BeginDrawing();

    // ClearBackground fills the entire window with a specified color
    [LibraryImport("raylib", EntryPoint = "ClearBackground")]
    internal static partial void ClearBackground(Color color);

    // EndDrawing finalizes the frame and swaps the rendering buffer
    [LibraryImport("raylib", EntryPoint = "EndDrawing")]
    internal static partial void EndDrawing();

    // CloseWindow releases raylib resources and closes the window
    [LibraryImport("raylib", EntryPoint = "CloseWindow")]
    internal static partial void CloseWindow();

    // DrawRectangle renders a filled rectangle at the specified position
    [LibraryImport("raylib", EntryPoint = "DrawRectangle")]
    internal static partial void DrawRectangle(int posX, int posY, int width, int height, Color color);

    // DrawText renders a text string at the specified position with given font size and color
    [LibraryImport("raylib", EntryPoint = "DrawText", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void DrawText(string text, int posX, int posY, int fontSize, Color color);
}

// Color struct must match the C raylib Color struct memory layout exactly
// LayoutKind.Sequential ensures fields are laid out in memory in the order they're declared
// This is essential for correct interop with the native C struct
[StructLayout(LayoutKind.Sequential)]
public record struct Color(byte R, byte G, byte B, byte A);

public class Program
{
    public static void Main(string[] args)
    {
        // Initialize the raylib window with dimensions 800x450
        // The window title explains the technology stack used
        Raylib.InitWindow(800, 450, "NativeAOT + DirectPInvoke + statically linked raylib");

        // Main game loop: continues until the user closes the window (ESC key or close button)
        while (!Raylib.WindowShouldClose())
        {
            // BeginDrawing marks the start of the current frame
            Raylib.BeginDrawing();
            
            // Clear the screen with light gray color (RGB: 180, 180, 180)
            Raylib.ClearBackground(new Color(180, 180, 180, 255));
            
            // Draw a red rectangle at position (50, 50) with size 100x100 pixels
            Raylib.DrawRectangle(50, 50, 100, 100, new Color(255, 0, 0, 255));
            
            // Draw blue text "Hello, raylib!" at position (200, 50) with font size 20
            Raylib.DrawText("Hello, from native C# and raylib!", 200, 50, 20, new Color(0, 0, 255, 255));
            
            // EndDrawing finalizes the frame and presents it to the screen
            Raylib.EndDrawing();
        }

        // Clean up raylib resources when the window is closed
        Raylib.CloseWindow();
    }
}
