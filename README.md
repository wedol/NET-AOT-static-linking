# NativeAOT + DirectPInvoke + Statically Linked Raylib

A demonstration project showing how to use **.NET 10 Native AOT** with **direct P/Invoke** to statically link and use the **raylib** graphics library in a compiled native executable.

## What is This Project?

This project demonstrates advanced .NET interop capabilities by combining:

- **Native AOT Compilation**: Compiling C# code ahead-of-time to a native binary without requiring the .NET runtime
- **Direct P/Invoke**: Using `LibraryImport` for compile-time marshaling code generation
- **Static Linking**: Embedding the raylib library directly into the executable

The result is a **self-contained native executable** (no .NET runtime dependency) that uses raylib for graphics rendering.

## How .NET Native AOT Works

### Traditional .NET Execution

```
C# Code → Compiled to IL → .NET Runtime (JIT) → Machine Code → Execution
```

The .NET runtime is required on the target machine, and compilation happens at runtime with the JIT compiler.

### Native AOT Execution

```
C# Code → AOT Compilation (Roslyn) → Machine Code → Executable → Execution
```

**Native AOT** compiles your entire application to native machine code **before** deployment. Here's what happens:

1. **Compile-Time Analysis**: The AOT compiler statically analyzes all reachable code
2. **Code Generation**: Generates optimized machine code for all code paths
3. **Native Linking**: Links against native libraries (like raylib) to create a standalone executable
4. **No Runtime Dependency**: The resulting binary doesn't need .NET installed on the target machine

### Benefits

- ✅ **Smaller Distribution**: No need to ship the .NET runtime
- ✅ **Faster Startup**: No JIT compilation or runtime initialization
- ✅ **Better Security**: Harder to reverse-engineer compared to managed IL
- ✅ **Cross-Platform**: Can produce separate binaries for Windows, Linux, macOS, etc.

## How to Compile with Native AOT

### Prerequisites

- **.NET 10 SDK** or later installed
- **Native development tools**:
  - **Windows**: Visual Studio Build Tools or MSVC compiler
  - **Linux**: GCC or Clang
- **Raylib libraries**: Static library files (`raylib.lib` or `raylib.a`) in the project directory

### Compilation Steps

#### 1. Publish for Native AOT (Windows x64)

```powershell
dotnet publish -c Release -r win-x64
```

This command:
- `-c Release` - Builds in Release mode (optimized)
- `-r win-x64` - Targets Windows x64 architecture
- Compiles C# to native code
- Links against raylib.lib
- Produces a standalone executable in `bin/Release/net10.0/win-x64/publish/`

#### 2. Publish for Native AOT (Linux x64)

```bash
dotnet publish -c Release -r linux-x64
```

Produces a Linux-compatible executable in `bin/Release/net10.0/linux-x64/publish/`

#### 3. Run the Native AOT Binary

**Windows:**
```powershell
.\bin\Release\net10.0\win-x64\publish\NativeAOTTest.exe
```

**Linux:**
```bash
./bin/Release/net10.0/linux-x64/publish/NativeAOTTest
```

The executable is completely self-contained and requires **no .NET runtime installed**.

### Development/Debugging (Standard .NET Runtime)

For faster iteration during development, you can run with the standard JIT compiler:

```powershell
dotnet run
```

This uses the .NET runtime and dynamically loads `raylib.dll`/`raylib.so` instead of the static library.

### Build Artifacts

After publishing, the output directory contains:
- `NativeAOTTest.exe` (Windows) or `NativeAOTTest` (Linux) - The main executable
- Various support files (DLLs, runtime files, etc.) for dependency resolution
- The binary is fully self-contained for the published configuration

## Static Linking with .NET

Static linking embeds the native library code directly into your executable rather than linking to a dynamic library at runtime.

### Dynamic Linking (Traditional)
```
Your App → Raylib.dll/.so (loaded at runtime)
```

### Static Linking (Native AOT)
```
Your App → [Raylib embedded inside] (no external dependency)
```

In this project, we use the `<NativeLibrary>` item in the `.csproj` file to:
- Include `raylib.lib` (Windows) or `raylib.a` (Linux) during native linking
- Embed raylib directly into the output executable
- Eliminate the need to distribute separate DLL/SO files

## Project Structure

```
NativeAOTTest/
├── Program.cs                 # Main application with raylib P/Invoke declarations
├── NativeAOTTest.csproj      # Project configuration for Native AOT + static linking
└── headers/
    └── raylib.h              # C header file with raylib function declarations
```

### The `headers/` Folder

The `headers/` folder contains the C header file `raylib.h`, which is the reference documentation for raylib's API. While not directly used during compilation, this header serves as a reference for understanding:
- Raylib function signatures
- Struct definitions (like `Color`)
- Constants and enumerations
- Expected behavior of native functions

This helps developers correctly declare P/Invoke methods that match the native library's interface.

## .csproj Configuration Explained

The `NativeAOTTest.csproj` file contains the critical configuration for Native AOT + static linking:

### Key Settings

```xml
<PublishAot>true</PublishAot>
```
Enables Native AOT compilation when publishing the application.

```xml
<RuntimeIdentifiers>win-x64;linux-x64</RuntimeIdentifiers>
```
Specifies target platforms. You can compile separate binaries for Windows and Linux.

```xml
<InvariantGlobalization>true</InvariantGlobalization>
```
Disables culture-specific globalization to reduce binary size (optional, but recommended for AOT).

```xml
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```
Permits unsafe code (though not used heavily in this example), which is sometimes necessary for advanced interop scenarios.

### Static Linking Configuration

```xml
<NativeLibrary Include="raylib.lib" Condition="'$(IsWindows)'=='true'" />
<NativeLibrary Include="raylib.a" Condition="'$(IsLinux)'=='true'" />
```
These lines tell the AOT compiler to link against the static raylib library:
- `raylib.lib` for Windows (MSVC static library format)
- `raylib.a` for Linux (GCC static archive format)

```xml
<DirectPInvoke Include="raylib" />
```
Enables **DirectPInvoke** mode, which generates faster, more efficient P/Invoke stubs at compile-time instead of using reflection-based marshaling.

```xml
<LinkerArg Include="gdi32.lib kernel32.lib msvcrt.lib opengl32.lib raylib.lib shell32.lib user32.lib winmm.lib" Condition="'$(IsWindows)'=='true'" />
```
Specifies additional Windows libraries needed for linking. Raylib depends on:
- `gdi32.lib` - Graphics Device Interface
- `kernel32.lib` - Windows kernel
- `opengl32.lib` - OpenGL graphics
- `raylib.lib` - The raylib library itself
- `user32.lib` - User interface functions
- `winmm.lib` - Windows multimedia
- And others for complete functionality

## P/Invoke and LibraryImport

The `Program.cs` file uses the modern `LibraryImport` attribute instead of the legacy `DllImport`:

```csharp
[LibraryImport("raylib", EntryPoint = "InitWindow", StringMarshalling = StringMarshalling.Utf8)]
internal static partial void InitWindow(int width, int height, string title);
```

### Why LibraryImport?

- **Compile-time code generation**: Marshaling code is generated at compile-time, enabling static analysis
- **Native AOT compatible**: Works seamlessly with AOT compilation
- **Type-safe**: Better compiler support and error checking
- **Modern C# feature**: Recommended for all new projects

## Building Your Own Native AOT Projects

To create your own Native AOT project with static linking:

1. Create a project with `<PublishAot>true</PublishAot>`
2. Add `<NativeLibrary>` items for your static libraries
3. Replace `DllImport` with `LibraryImport` for all P/Invoke declarations
4. Include `<DirectPInvoke>` for libraries you want to optimize
5. Add `<LinkerArg>` entries for additional required native libraries
6. Test with `dotnet publish -c Release -r <RID>`

## Resources

- [Microsoft Docs: Native AOT Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Microsoft Docs: P/Invoke Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices)
- [Raylib Official Website](https://www.raylib.com/)
- [LibraryImport vs DllImport](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)

## License

This is a demonstration project. Raylib is licensed under the zlib license.
