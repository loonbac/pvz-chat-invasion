using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

// Lee metadata CRUDA (ECMA-335) de los Il2CppAssemblies sin cargar/resolver tipos.
// Robusto frente a los crashes nativos de MetadataLoadContext con assemblies IL2CPP.
// Objetivo: enum ZombieType (valores) + métodos del juego para spawnear zombies.

class Program
{
    static int Main()
    {
        string game = "/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted";
        string il2cpp = Path.Combine(game, "MelonLoader", "Il2CppAssemblies");
        string gameAsm = Path.Combine(il2cpp, "Assembly-CSharp.dll");

        if (!File.Exists(gameAsm)) { Console.WriteLine("No existe " + gameAsm); return 1; }

        using var fs = File.OpenRead(gameAsm);
        using var pe = new PEReader(fs);
        var r = pe.GetMetadataReader();

        // 1) enum ZombieType: volcar nombre = valor
        Console.WriteLine("===== enum ZombieType (valores) =====");
        foreach (var th in r.TypeDefinitions)
        {
            var td = r.GetTypeDefinition(th);
            if (r.GetString(td.Name) != "ZombieType") continue;
            Console.WriteLine("  " + Full(r, td));
            foreach (var fh in td.GetFields())
            {
                var fd = r.GetFieldDefinition(fh);
                if ((fd.Attributes & FieldAttributes.Literal) == 0) continue;
                string name = r.GetString(fd.Name);
                string val = ReadConst(r, fd.GetDefaultValue());
                Console.WriteLine($"      {name} = {val}");
            }
        }

        // 2) Métodos cuyo nombre sugiere crear/añadir zombies
        Console.WriteLine("\n===== Métodos de spawn/add de zombies (Tipo.Metodo, #params) =====");
        var hits = new List<string>();
        foreach (var th in r.TypeDefinitions)
        {
            var td = r.GetTypeDefinition(th);
            string tname = r.GetString(td.Name);
            string tns = r.GetString(td.Namespace);
            foreach (var mh in td.GetMethods())
            {
                var md = r.GetMethodDefinition(mh);
                string mn = r.GetString(md.Name);
                if (mn.IndexOf("Zombie", StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (!(mn.StartsWith("Add") || mn.StartsWith("Spawn") || mn.StartsWith("Create")
                      || mn.StartsWith("Place") || mn.StartsWith("Make") || mn.StartsWith("Summon"))) continue;
                int pc;
                try { var sig = md.DecodeSignature(new NameProvider(r), null); pc = sig.ParameterTypes.Length; }
                catch { pc = -1; }
                hits.Add($"  {(tns.Length>0?tns+".":"")}{tname}.{mn}  ({pc} params)");
            }
        }
        foreach (var h in hits.Distinct().OrderBy(x => x)) Console.WriteLine(h);
        if (hits.Count == 0) Console.WriteLine("  (ninguno con Add/Spawn/Create/Place/Make/Summon + 'Zombie')");

        // 3) Firmas EXACTAS de los métodos de spawn de Board
        Console.WriteLine("\n===== Firmas exactas (Board.AddZombie*) =====");
        var want = new HashSet<string> { "AddZombie", "AddZombieInRow", "AddZombieAtCell" };
        foreach (var th in r.TypeDefinitions)
        {
            var td = r.GetTypeDefinition(th);
            if (r.GetString(td.Name) != "Board") continue;
            foreach (var mh in td.GetMethods())
            {
                var md = r.GetMethodDefinition(mh);
                string mn = r.GetString(md.Name);
                if (!want.Contains(mn)) continue;
                try
                {
                    var sig = md.DecodeSignature(new NameProvider(r), null);
                    var ps = string.Join(", ", sig.ParameterTypes);
                    Console.WriteLine($"      {sig.ReturnType} Board.{mn}({ps})");
                }
                catch (Exception e) { Console.WriteLine($"      Board.{mn} (no decodificable: {e.Message})"); }
            }
        }

        return 0;
    }

    static string Full(MetadataReader r, TypeDefinition td)
    {
        string ns = r.GetString(td.Namespace);
        string n = r.GetString(td.Name);
        return ns.Length > 0 ? ns + "." + n : n;
    }

    static string ReadConst(MetadataReader r, ConstantHandle ch)
    {
        if (ch.IsNil) return "?";
        var c = r.GetConstant(ch);
        var br = r.GetBlobReader(c.Value);
        try
        {
            return c.TypeCode switch
            {
                ConstantTypeCode.SByte => br.ReadSByte().ToString(),
                ConstantTypeCode.Byte => br.ReadByte().ToString(),
                ConstantTypeCode.Int16 => br.ReadInt16().ToString(),
                ConstantTypeCode.UInt16 => br.ReadUInt16().ToString(),
                ConstantTypeCode.Int32 => br.ReadInt32().ToString(),
                ConstantTypeCode.UInt32 => br.ReadUInt32().ToString(),
                ConstantTypeCode.Int64 => br.ReadInt64().ToString(),
                _ => "(" + c.TypeCode + ")"
            };
        }
        catch { return "?"; }
    }
}

// Proveedor de firmas que resuelve NOMBRES de tipos (def/ref) legibles.
class NameProvider : ISignatureTypeProvider<string, object>
{
    private readonly MetadataReader _r;
    public NameProvider(MetadataReader r) => _r = r;

    public string GetArrayType(string e, ArrayShape s) => e + "[]";
    public string GetByReferenceType(string e) => "ref " + e;
    public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
    public string GetGenericInstantiation(string g, System.Collections.Immutable.ImmutableArray<string> a) => g + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool r) => u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte rk)
    { var td = r.GetTypeDefinition(h); return r.GetString(td.Name); }
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte rk)
    { var tr = r.GetTypeReference(h); return r.GetString(tr.Name); }
    public string GetTypeFromSpecification(MetadataReader r, object c, TypeSpecificationHandle h, byte rk)
    { try { return r.GetTypeSpecification(h).DecodeSignature(this, c); } catch { return "spec"; } }
}
