fn main() {
    csbindgen::Builder::default()
        .input_extern_file("./tegra_swizzle/src/ffi.rs")
        .input_extern_file("./tegra_swizzle/src/surface.rs")
        .csharp_namespace("TegraSwizzle")
        .csharp_dll_name("mercuryenginedata_tegra_swizzle")
        .generate_csharp_file("./Native/TegraSwizzle.g.cs")
        .unwrap();
}
