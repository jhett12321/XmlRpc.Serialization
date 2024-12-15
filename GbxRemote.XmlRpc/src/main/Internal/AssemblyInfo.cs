using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GbxRemote.XmlRpc.Tests")]

namespace GbxRemote.XmlRpc.Internal;

internal static class AssemblyInfo
{
  public static readonly AssemblyInformationalVersionAttribute VersionInfo = typeof(AssemblyInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!;
}
