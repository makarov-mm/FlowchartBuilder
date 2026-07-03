namespace Makarov.Framework.Core.SettingStorages
{

    public sealed class FakeStorage : IDict<string>
    {
        private readonly StringDictionary<string> _dict = new StringDictionary<string>();

        public string this[string x]
        {
            get
            {
                if (string.IsNullOrEmpty(x))
                    return null;

                if (!_dict.ContainsKey(x))
                    return null;

                return _dict[x];
            }

            set
            {
                if (string.IsNullOrEmpty(x))
                    return;

                if (!_dict.ContainsKey(x))
                {
                    _dict.Add(x, value);
                    return;
                }

                _dict[x] = value;
            }
        }

        public bool IsKeyExists(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return _dict.ContainsKey(key);
        }
    }
}
