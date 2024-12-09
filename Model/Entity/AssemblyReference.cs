using System.Reflection;

namespace Entity
{
    public static class AssemblyReference
    {
        public static readonly Assembly assembly = typeof(AssemblyReference).Assembly;
    }
}
