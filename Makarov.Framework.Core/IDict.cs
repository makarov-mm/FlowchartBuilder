namespace Makarov.Framework.Core
{

    public interface IDict<T>
    {

        T this[string x] { get; set; }

        bool IsKeyExists(string key);
    }
}
