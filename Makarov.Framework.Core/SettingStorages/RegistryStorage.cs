using Microsoft.Win32;

namespace Makarov.Framework.Core.SettingStorages
{

    public sealed class RegistryStorage : IDict<string>
    {
        #region Exceptions

        public class RegistryStorageException : MakarovFrameworkException
        {

            public RegistryStorageException(string message)
                : base(message)
            { }
        }

        public sealed class CantCreateKeyException : RegistryStorageException
        {

            public CantCreateKeyException(string key)
                : base($"Can't create registry key '{key}'.")
            { }
        }

        public sealed class CantDeleteKeyException : RegistryStorageException
        {

            public CantDeleteKeyException(string key)
                : base($"Can't delete value from registry key '{key}'.")
            { }
        }
        #endregion

        #region Private members

        private RegistryKey _key;
        #endregion

        #region Constructors

        public RegistryStorage(string registryKey)
        {

            _key = Registry.CurrentUser.OpenSubKey(
                registryKey,
                RegistryKeyPermissionCheck.ReadWriteSubTree);

            if (_key is null)
            {
                _key = Registry.CurrentUser.CreateSubKey(
                    registryKey,
                    RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            if (!IsRegistryAccessible)
                throw new CantCreateKeyException(registryKey);
        }

        ~RegistryStorage()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_key != null)
            {
                _key.Flush();
                _key.Close();
                _key = null;
            }
        }
        #endregion

        #region Methods

        public string GetRegistryValue(string subKey)
        {

            if (IsRegistryAccessible)
                return (string)_key.GetValue(subKey);

            return null;
        }

        public void SetRegistryValue(string subKey, string val)
        {

            if (IsRegistryAccessible)
            {
                _key.SetValue(subKey, val);
                return;
            }
        }

        public void DeleteRegistryValue(string subKey)
        {

            if (IsRegistryAccessible)
            {
                _key.DeleteSubKey(subKey);
                return;
            }

            throw new CantDeleteKeyException(subKey);
        }

        public bool IsKeyExists(string subKey)
        {
            if (IsRegistryAccessible)
            {
                object obj = _key.GetValue(subKey);
                if (obj == null)
                    return false;

                return !string.IsNullOrEmpty((string)obj);
            }

            return false;
        }
        #endregion

        #region Properties

        public string this[string subKey]
        {
            get => GetRegistryValue(subKey);
            set => SetRegistryValue(subKey, value);
        }

        public bool IsRegistryAccessible => _key != null;

        #endregion
    }
}
